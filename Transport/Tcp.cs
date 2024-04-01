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
	
	// Create a connection to the server
	public override void Run(Cli cli) {
		try {
			// Connect to the server
			// Null pointer reference is checked multiple times before calling client.Run hence is omitted
			_client.Connect(cli.ServerAddress!, cli.ServerPort);
		} catch (Exception e) {
			throw new Exception($"Error connecting to the server: {e.Message}");
		}

		// Create thread for receiving TCP data
		Thread receive = new Thread(ReceiveData);
		receive.Start();
	}

	// Close a connection to the server
	public override void Close() {
		_closed = true;
		_client.Close();
	}

	// Function responsible for receiving data from teh server. Runs in a separate thread
	protected override void ReceiveData() {
		try {
			// Read byte stream from the server
			NetworkStream stream = _client.GetStream();
			byte[] data = new byte[2048];
			int bufferOffset = 0;

			while (!_closed) {
				int bytesRead = stream.Read(data, 0, data.Length);
				if (bytesRead > 0) {
					bufferOffset += bytesRead;

					int messageEnd;
					while ((messageEnd = Array.IndexOf(data, (byte)'\n', 0, bufferOffset)) != -1) {
						// Extract the message from the buffer
						byte[] messageData = new byte[messageEnd + 1];
						Array.Copy(data, 0, messageData, 0, messageEnd + 1);

						// Process the message
						ProcessMessage(messageData, messageEnd + 1);
						
						// Remove the message from the buffer
						bufferOffset -= messageEnd + 1;
						Array.Copy(data, messageEnd + 1, data, 0, bufferOffset);
					}
				}
			}
		} catch (Exception e) {
			// Exceptions are output to the standard error output.
			// The program is not stopped and the function continues to receive data
			Error.Print(e.Message);
		}
	}

	// ProcessData function is responsible for handling different types of messages
	protected override void ProcessMessage(byte[] data, int dataLength) {
		string message = Encoding.ASCII.GetString(data, 0, dataLength);
		NetworkStream stream = _client.GetStream();
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
				data = Encoding.ASCII.GetBytes(bye.CreateTcpMessage());
				stream.Write(data, 0, data.Length);
				Environment.Exit(0);
				break;
			case 'M': case 'm':
				Msg receiveMsg = new Msg();
				try {
					receiveMsg.DeserializeTcpMessage(message);
				} catch (Exception e) {
					ByeOnInvalidMessage(e.Message);
				}
				
				// Receiving Msg in any other than Open state is not expected
				if (State != State.Open) {
					ByeOnInvalidMessage("Unexpected message");
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
					// If the current state is Authenticating, switch to Open state if the Reply is positive
					// otherwise switch back to the Default state. The user will have to resend /auth command
					State = receiveReply.Result ? State.Open : State.Default;
					return;
				}
				if (State == State.Joining) {
					// If the current state is Joining, both positive and negative Reply will result in the Default state
					State = State.Default;
					return;
				}
				
				// In any other state the Reply message is unexpected
				// The FSM does not directly state that Reply cannot be received in an Open state
				// But it doesnt make sense to receive a Reply if no command was sent to the server
				ByeOnInvalidMessage("Unexpected message");
				
				break;
			default:
				// Receiving Auth, Confirm, Join or any other message will result in error and closing the connection
				ByeOnInvalidMessage("Unexpected or malformed message.");
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
		data = Encoding.ASCII.GetBytes(err.SerializeTcpMessage());
		stream.Write(data, 0, data.Length);
		
		data = Encoding.ASCII.GetBytes(bye.CreateTcpMessage());
		stream.Write(data, 0, data.Length);
		
		Environment.Exit(1);
	}
	
	// This function send the user inputted data to the server
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
		// Message can only be sent in an Open state
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
			
			data = Encoding.ASCII.GetBytes(msg.SerializeTcpMessage());
		} catch (Exception e) {
			Error.Print(e.Message);
			return;
		}
		
		NetworkStream stream = _client.GetStream();
		stream.Write(data, 0, data.Length);
	}

	public override void SendBye() {
		NetworkStream stream = _client.GetStream();
		Bye bye = new Bye();
		byte[] data = Encoding.ASCII.GetBytes(bye.CreateTcpMessage());
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
				
			data = Encoding.ASCII.GetBytes(auth.SerializeTcpMessage());
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
				
			data = Encoding.ASCII.GetBytes(join.SerializeTcpMessage());
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
}