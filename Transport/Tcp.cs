using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using IPK_Project1.Enums;
using IPK_Project1.Messages;

namespace IPK_Project1.Transport;

public class Tcp : Client {
	private TcpClient _client = new();
	private bool _closed;
	
	public override void Run(string server, ushort port) {
		try {
			// Connect to the server
			_client.Connect(server, port);
		} catch (Exception e) {
			Error.Print($"Error connecting to the server: {e.Message}");
			return;
		}

		// Create thread for receiving TCP data
		Thread receive = new Thread(ReceiveData);
		receive.Start();
	}

	public override void Close() {
		_closed = true;
		_client.Close();
	}

	protected override void ReceiveData() {
		try {
			// Read byte stream from the server
			NetworkStream stream = _client.GetStream();
			byte[] data = new byte[2048];

			while (true) {
				// If the client is closed, stop receiving data
				if (_closed) {
					return;
				}
				
				int bytesRead = stream.Read(data, 0, data.Length);
				if (bytesRead > 0) {
					string receivedMessage = Encoding.ASCII.GetString(data, 0, bytesRead);
					ProcessMessage(receivedMessage);
				}
			}
		} catch (Exception e) {
			Error.Print(e.Message);
		}
	}

	protected override void ProcessMessage(string message) {
		// Identify the message type and create a message object
		switch (message[0]) {
			case 'B': case 'b':
				// Receiving Bye message results in closing the connection and terminating the program
				Bye receiveBye = new Bye();
				
				try {
					// Try to deserialize bye message
					receiveBye.DeserializeTcpMessage(message);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				// Recycling :)
				SendData(receiveBye.CreateTcpMessage());
				Environment.Exit(0);
				break;
			case 'E': case 'e':
				Err receiveErr = new Err();
				try {
					receiveErr.DeserializeTcpMessage(message);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				// Print out the error message and close the connection
				receiveErr.PrintMessage();
				Bye bye = new Bye();
				SendData(bye.CreateTcpMessage());
				Environment.Exit(0);
				break;
			case 'M': case 'm':
				Msg receiveMsg = new Msg();
				try {
					receiveMsg.DeserializeTcpMessage(message);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				if (State != State.Open) {
					ByeOnInvalidMessage("Message Unexpected");
				}
				
				receiveMsg.PrintMessage();
				break;
			case 'R': case 'r':
				Reply receiveReply = new Reply();
				try {
					receiveReply.DeserializeTcpMessage(message);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				// Print incoming reply message
				receiveReply.PrintMessage();

				if (State == State.Authenticating) {
					State = receiveReply.Result ? State.Open : State.Default;
					return;
				}
				if (State == State.Joining) {
					State = State.Default;
					return;
				}
				// In any other state the Reply message is unexpected
				// The FSM does not directly state that Reply cannot be received in open state
				// But it doesnt make sense to receive a Reply if no command was sent to the server
				ByeOnInvalidMessage("Message Unexpected");
				
				break;
			default:
				// Receiving Auth, Confirm, Join or any other message will result in error and closing the connection
				ByeOnInvalidMessage("Invalid or malformed message.");
				break;
		}
	}

	// Sends Err message to the server, closes the connection with Bye and terminates the program
	private void ByeOnInvalidMessage(string error) {
		NetworkStream stream = _client.GetStream();
		Error.Print("Received invalid message. Closing the connection.");

		Bye bye = new Bye();
		byte[] data;
		
		// In Default state only Bye message can be sent as the DisplayName is not set
		if (State == State.Default) {
			data = Encoding.ASCII.GetBytes(bye.CreateTcpMessage());
			stream.Write(data, 0, data.Length);
			Environment.Exit(1);
		}
		
		Err err = new Err {
			Type = MessageType.Err,
			DisplayName = DisplayName,
			MessageContents = error
		};
		data = Encoding.ASCII.GetBytes(err.CreateTcpMessage());
		stream.Write(data, 0, data.Length);
		
		data = Encoding.ASCII.GetBytes(bye.CreateTcpMessage());
		stream.Write(data, 0, data.Length);
		
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
		
		try {
			Msg msg = new Msg {
				Type = MessageType.Msg,
				DisplayName = DisplayName,
				MessageContents = message
			};
			
			data = Encoding.ASCII.GetBytes(msg.CreateTcpMessage());
		} catch (Exception e) {
			Error.Print(e.Message);
			return;
		}
		
		NetworkStream stream = _client.GetStream();
		stream.Write(data, 0, data.Length);
	}

	// Local command handlers
	protected override void HandleAuthCommand(string message) {
		byte[] data;
		AuthCommand ac = ValidateAuthCommand(message);
		if (ac.IsEmpty()) {
			return;
		}
		
		DisplayName = ac.DisplayName;
		try {
			Auth auth = new Auth {
				Type = MessageType.Auth,
				DisplayName = ac.DisplayName,
				Secret = ac.Secret,
				Username = ac.Username
			};
				
			data = Encoding.ASCII.GetBytes(auth.CreateTcpMessage());
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
		
		NetworkStream stream = _client.GetStream();
		stream.Write(data, 0, data.Length);
	}

	protected override void HandleJoinCommand(string message) {
		byte[] data;
		string channelId = ValidateJoinCommand(message);
		if (string.IsNullOrEmpty(channelId)) {
			return;
		}
			
		try {
			Join join = new Join {
				Type = MessageType.Join,
				ChannelId = channelId,
				DisplayName = DisplayName
			};
				
			data = Encoding.ASCII.GetBytes(join.CreateTcpMessage());
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
		
		NetworkStream stream = _client.GetStream();
		stream.Write(data, 0, data.Length);
	}

	protected override void HandleRenameCommand(string message) {
		string name = ValidateRenameCommand(message);
		if (!string.IsNullOrEmpty(name)) {
			DisplayName = name;
		}
	}

	protected override void HandleGetNameCommand(string message) {
		Console.WriteLine("[" + DisplayName + "]");
	}

	protected override void HandleGetStateCommand(string message) {
		Console.WriteLine("[" + State + "]");
	}

	protected override void HandleHelpCommand(string message) {
		PrintHelp();
	}
}