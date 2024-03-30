using System.Net.Sockets;
using System.Text;
using IPK_Project1.Enums;
using IPK_Project1.Messages;

namespace IPK_Project1;

public class Tcp : Client {
	private TcpClient _client = new();
	
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
		_client.Close();
	}

	protected override void ReceiveData() {
		try {
			// Read byte stream from the server
			NetworkStream stream = _client.GetStream();
			byte[] data = new byte[2048];

			while (true) {
				int bytesRead = stream.Read(data, 0, data.Length);
				if (bytesRead > 0) {
					string receivedMessage = Encoding.ASCII.GetString(data, 0, bytesRead);
					// TODO: process message
					Console.Write(receivedMessage);
				}
			}
		} catch (Exception e) {
			Error.Print(e.Message);
		}
	}

	public override void SendData(string message) {
		byte[] data = Encoding.ASCII.GetBytes(message + "\r\n");
		// Check if the message is ASCII encoded and replace non ascii characters with '?'
		StringBuilder stringBuilder = new();
		foreach (char c in message) {
			stringBuilder.Append(c <= 127 ? c : '?');
		}

		message = stringBuilder.ToString();

		// Check if message is command
		if (message.StartsWith("/")) {
			string command = message.Split(' ')[0];
			if (commandHandlers.TryGetValue(command, out var handler)) {
				handler(message);
			}

			return;
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
				MessageContents = message,
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

		try {
			Auth auth = new Auth {
				Type = MessageType.Auth,
				DisplayName = ac.DisplayName,
				Secret = ac.Secret,
				Username = ac.Username,
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
				DisplayName = DisplayName,
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