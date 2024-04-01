using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using IPK_Project1.Enums;
using IPK_Project1.Messages;

namespace IPK_Project1.Transport;

public class Udp : Client {
	private UdpClient _client = new();
	private IPEndPoint _ipEndPoint;
	private ushort _messageId = 0;
	private int _maxRetransmissions;
	private int _udpTimeout;
	private bool _closed;
	
	private Dictionary<int, CancellationTokenSource> _cancelRetransmission = new();
	
	public override void Run(Cli cli) {
		// Resolve the server address
		// Null pointer reference is checked multiple times before calling Run
		IPAddress[] addresses = Dns.GetHostAddresses(cli.ServerAddress!);
		if (addresses.Length == 0) {
			throw new Exception("Unable to resolve server address");
			
		}
		_ipEndPoint = new IPEndPoint(addresses[0], cli.ServerPort);

		_udpTimeout = cli.UdpTimeout;
		_maxRetransmissions = cli.MaxRetransmissions;
		
		_client.Connect(_ipEndPoint);
		
		// Thread for receiving data
		Thread receive = new Thread(ReceiveData);
		receive.Start();
	}

	public override void Close() {
		_closed = true;
		_client.Close();
	}

	protected override void ReceiveData() {
		while (!_closed) {
			try {
				byte[] data = _client.Receive(ref _ipEndPoint);
				ProcessMessage(data, data.Length);
			} catch (Exception e) {
				Error.Print(e.Message);
			}
		}
	}

	protected override void ProcessMessage(byte[] data, int dataLength) {
		// Identify the message type and create a message object
		switch ((MessageType)data[0]) {
			case MessageType.Bye:
				try {
					Bye receivedBye = new Bye();
					receivedBye.DeserializeUdpMessage(data);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				_client.Send(data, data.Length);
				Environment.Exit(0);
				break;
			case MessageType.Err:
				Err receiveErr = new Err();
				try {
					receiveErr.DeserializeUdpMessage(data);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				// Print out the error message
				receiveErr.PrintMessage();
				Bye bye = new Bye();
				_client.Send(bye.CreateUdpMessage(), bye.CreateUdpMessage().Length);
				Environment.Exit(0);
				break;
			case MessageType.Confirm:
				Confirm receiveConfirm = new Confirm();
				try {
					receiveConfirm.DeserializeUdpMessage(data);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}

				if (_cancelRetransmission.TryGetValue(receiveConfirm.MessageId, out CancellationTokenSource token)) {
					token.Cancel();
					_cancelRetransmission.Remove(receiveConfirm.MessageId);
				}
				return;
			case MessageType.Reply:
				Reply receiveReply = new Reply();
				Confirm confirmReply = new Confirm();
				try {
					receiveReply.DeserializeUdpMessage(data);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				receiveReply.PrintMessage();

				try {
					confirmReply.RefMessageId = receiveReply.MessageId;
					_client.Send(confirmReply.SerializeUdpMessage(), confirmReply.SerializeUdpMessage().Length);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				
				if (State == State.Authenticating) {
					State = receiveReply.Result ? State.Open : State.Default;
					if (State == State.Default) {
						return;
					}
					
					ushort newPort = BitConverter.ToUInt16(data, 1);
					_client.Close();
					_client = new UdpClient();
					_ipEndPoint.Port = newPort;
					_client.Connect(_ipEndPoint);
					return;
				}

				if (State == State.Joining) {
					State = State.Default;
					return;
				}
				
				ByeOnInvalidMessage("Unexpected message");
				
				break;
			case MessageType.Msg:
				Msg receiveMsg = new Msg();
				Confirm confirmMsg = new Confirm();
				try {
					receiveMsg.DeserializeUdpMessage(data);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				try {
					confirmMsg.RefMessageId = receiveMsg.MessageId;
					_client.Send(confirmMsg.SerializeUdpMessage(), confirmMsg.SerializeUdpMessage().Length);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				receiveMsg.PrintMessage();
				break;
			default:
				ByeOnInvalidMessage("Unexpected mesage");
				break;
		}
	}

	private void ByeOnInvalidMessage(string error) {
		Error.Print("Received invalid message. Closing the connection.");
		Bye bye = new Bye();
		byte[] data;

		if (State == State.Default) {
			data = bye.CreateUdpMessage();
			_client.Send(data, data.Length);
			Environment.Exit(1);
		}
		
		Err err = new Err {
			Type = MessageType.Err,
			MessageId = ++_messageId,
			DisplayName = DisplayName,
			MessageContents = error
		};

		data = err.SerializeUdpMessage();
		_client.Send(data, data.Length);

		data = bye.CreateUdpMessage();
		_client.Send(data, data.Length);
		
		Environment.Exit(1);
	}
	
	public override void SendData(string message) {
		if (State == State.Authenticating) {
			Error.Print("Cannot process messages or commands while authenticating.");
			return;
		}
		
		byte[] data;
		// Check if the message is ASCII encoded and replace non ascii characters with '?'
		StringBuilder stringBuilder = new();
		foreach (char c in message) {
			stringBuilder.Append(c <= 127 ? c : '?');
		}

		message = stringBuilder.ToString();
		
		// Check if message is command
		if (message.StartsWith('/')) {
			string command = message.Split(' ')[0];
			if (CommandHandlers.TryGetValue(command, out var handler)) {
				handler(message);
				return;
			}
		}
		
		// Message is not a command, process as a message
		// Message can only be sent in open state
		if (State != State.Open) {
			Error.Print("Cannot send a message when not connected to the server.");
			return;
		}

		Msg msg;
		try {
			 msg = new Msg {
				Type = MessageType.Msg,
				MessageId = ++_messageId,
				DisplayName = DisplayName,
				MessageContents = message
			};

			data = msg.SerializeUdpMessage();
		} catch (Exception e) {
			Error.Print(e.Message);
			return;
		}
		
		CancellationTokenSource cts = new();
		_cancelRetransmission[msg.MessageId] = cts;
		Thread sendThread = new Thread(() => SendDataWithRetransmissions(data, cts.Token));
		sendThread.Start();
	}

	public override void SendBye() {
		Bye bye = new Bye();
		_client.Send(bye.CreateUdpMessage(), bye.CreateUdpMessage().Length);
	}

	private void SendDataWithRetransmissions(byte[] data, CancellationToken token) {
		int retransmissions = 0;
		while (retransmissions <= _maxRetransmissions) {
			if (token.IsCancellationRequested) {
				return;
			}
			
			_client.Send(data, data.Length);
			Thread.Sleep(_udpTimeout);
			retransmissions++;
		}
	}
	
	protected override void HandleAuthCommand(string message) {
		byte[] data;
		AuthCommand ac = ValidateAuthCommand(message);
		if (ac.IsEmpty()) {
			return;
		}
		
		DisplayName = ac.DisplayName;
		Auth auth;
		try {
			auth = new Auth {
				Type = MessageType.Auth,
				MessageId = ++_messageId, 
				DisplayName = ac.DisplayName,
				Secret = ac.Secret,
				Username = ac.Username
			};

			data = auth.SerializeUdpMessage();
		} catch (Exception e) {
			Error.Print(e.Message);
			return;
		}
		
		// User can only authenticate when in default state
		if (State != State.Default) {
			Error.Print("Cannot authenticate at this moment.");
			return;
		}
		
		// Change the state to authenticating
		State = State.Authenticating;
		
		CancellationTokenSource cts = new();
		_cancelRetransmission[auth.MessageId] = cts;
		Thread sendThread = new Thread(() => SendDataWithRetransmissions(data, cts.Token));
		sendThread.Start();
	}

	protected override void HandleJoinCommand(string message) {
		byte[] data;
		string channelId = ValidateJoinCommand(message);
		if (string.IsNullOrEmpty(channelId)) {
			return;
		}

		Join join;
		try {
			join = new Join {
				Type = MessageType.Join,
				MessageId = ++_messageId,
				ChannelId = channelId,
				DisplayName = DisplayName
			};

			data = join.SerializeUdpMessage();
		} catch (Exception e) {
			Error.Print(e.Message);
			return;
		}

		if (State != State.Open) {
			Error.Print("Cannot join a channel when not connected to the server.");
			return;
		}
		// Change current state to joining
		State = State.Joining;

		CancellationTokenSource cts = new();
		_cancelRetransmission[join.MessageId] = cts;
		Thread sendThread = new Thread(() => SendDataWithRetransmissions(data, cts.Token));
		sendThread.Start();
	}
}