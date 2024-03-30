using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using IPK_Project1.Enums;
using IPK_Project1.Messages;

namespace IPK_Project1;

public class Client {
	private string DisplayName { get; set; } = string.Empty;
	private State State { get; set; } = State.Default;
	
	private TcpClient _client = new();
	
	// Struct for the /auth command
	private struct AuthCommand {
		public string Username;
		public string Secret;
		public string DisplayName;

		public bool IsEmpty() {
			return string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Secret) && string.IsNullOrEmpty(DisplayName);
		}
	}

	// Run the TCP client
	public void RunTcp(string server, ushort port) {
		try {
			// Connect to the server
			_client.Connect(server, port);
		} catch (Exception e) {
			Error.Print($"Error connecting to the server: {e.Message}");
			return;
		}

		// Create thread for receiving TCP data
		Thread receive = new Thread(ReceiveTcpData);
		receive.Start();
	}

	// Close the TCP connection
	public void CloseTcp() {
		_client.Close();
	}

	// Receive data from the server
	// This method does not further process the data
	private void ReceiveTcpData() {
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

	// Send data to the server
	public void SendTcpData(string message) {
		byte[] data = Encoding.ASCII.GetBytes(message + "\r\n");
		// Check if the message is ASCII encoded and replace non ascii characters with '?'
		StringBuilder stringBuilder = new();
		foreach (char c in message) {
			stringBuilder.Append(c <= 127 ? c : '?');
		}

		message = stringBuilder.ToString();

		// Check if a message is a command
		if (message.StartsWith("/auth ")) {
			AuthCommand ac = ValidateAuthCommand(message);
			if (ac.IsEmpty()) {
				return;
			}

			Auth auth = new Auth {
				Type = MessageType.Auth,
				DisplayName = ac.DisplayName,
				Secret = ac.Secret,
				Username = ac.Username,
			};

			try {
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
		} else if (message.StartsWith("/join ")) {
			string channelId = ValidateJoinCommand(message);
			Join join = new Join {
				Type = MessageType.Join,
				ChannelId = channelId,
				DisplayName = DisplayName,
			};
			
			try {
				data = Encoding.ASCII.GetBytes(join.CreateTcpMessage());
			} catch (Exception e) {
				Error.Print(e.Message);
				return;
			}

			if (State != State.Open) {
				Error.Print("Cannot join a channel when not connected to the server.");
				return;
			}
		} else if (message.StartsWith("/rename ")) {
			string name = ValidateRenameCommand(message);
			if (!string.IsNullOrEmpty(name)) {
				DisplayName = name;
			}

			return;
		} else if (message.StartsWith("/help ")) {
			PrintHelp();
			return;
		} else {
			// Handle standard message
			Msg msg = new Msg {
				Type = MessageType.Msg,
				DisplayName = DisplayName,
				MessageContents = message,
			};
			
			try {
				data = Encoding.ASCII.GetBytes(msg.CreateTcpMessage());
			} catch (Exception e) {
				Error.Print(e.Message);
				return;
			}
			
			// Message can only be sent in open state
			if (State != State.Open) {
				Error.Print("Cannot send a message when not connected to the server.");
				return;
			}
		}
		
		// Sends the message to the server
		NetworkStream stream = _client.GetStream();
		stream.Write(data, 0, data.Length);
	}

	// Print supported local commands
	private static void PrintHelp() {
		Console.WriteLine("Supported local commands:");
		Console.WriteLine("/auth {Username} {Secret} {DisplayName}");
		Console.WriteLine("		Sends AUTH command to the server with the provided parameters");
		Console.WriteLine("/join {ChannelID}");
		Console.WriteLine("		Sends JOIN command to the server with the provided parameters");
		Console.WriteLine("/rename {DisplayName}");
		Console.WriteLine("		Locally changes the display name of the user to be sent with new messages");
		Console.WriteLine("/help");
		Console.WriteLine("		Prints out supported local commands with their parameters and a description");
	}

	// Validates the /rename command, if valid, returns the new display name otherwise returns empty string
	private static string ValidateRenameCommand(string message) {
		var match = Regex.Match(message, @"/rename\s(.{1,20})");
		string displayName = match.Groups[1].Value;
		if (Message.CheckDisplayName(displayName)) {
			return displayName;
		}

		Error.Print("Invalid rename command. Use /rename {DisplayName}");
		Error.Print("{DisplayName} has to contain only printable characters and be between 1 and 20 characters long.");
		return String.Empty;
	}
	
	// Validates the /join command, if valid, returns the channel ID otherwise returns empty string
	private static string ValidateJoinCommand(string message) {
		var match = Regex.Match(message, @"/join\s(.{1,20})");
		string channelId = match.Groups[1].Value;
		if (Message.CheckUsernameOrChannelId(channelId)) {
			return channelId;
		}

		Error.Print("Invalid join command. Use /join {ChannelID}");
		Error.Print("{ChannelID} has to contain only characters [A-z0-9-] and be between 1 and 20 characters long.");
		return String.Empty;
	}

	// Validates the /auth command, if valid, returns the AuthCommand otherwise returns empty AuthCommand
	private static AuthCommand ValidateAuthCommand(string message) {
		var match = Regex.Match(message, @"/auth\s(.{1,20})\s(.{1,20})\s(.{1,20})");
		AuthCommand ac = new AuthCommand {
			Username = match.Groups[1].Value,
			Secret = match.Groups[2].Value,
			DisplayName = match.Groups[3].Value,
		};
		if (Message.CheckUsernameOrChannelId(ac.Username) && Message.CheckSecret(ac.Secret) && Message.CheckDisplayName(ac.DisplayName)) {
			return ac;
		}

		Error.Print("Invalid auth command. Use /auth {Username} {Secret} {DisplayName}");
		Error.Print("{Username} has to contain only characters [A-z0-9-] and be between 1 and 20 characters long.");
		Error.Print("{Secret} has to contain only characters [A-z0-9-] and be between 1 and 20 characters long.");
		Error.Print("{DisplayName} has to contain only printable characters and be between 1 and 20 characters long.");
		return new AuthCommand();
	}
}