using System.Text.RegularExpressions;
using IPK_Project1.Enums;
using IPK_Project1.Messages;

namespace IPK_Project1.Transport;

public abstract class Client {
	protected string DisplayName { get; set; } = string.Empty;
	protected State State { get; set; } = State.Default;
	
	// Struct for the /auth command
	protected struct AuthCommand {
		public string Username;
		public string Secret;
		public string DisplayName;

		public bool IsEmpty() {
			return string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Secret) && string.IsNullOrEmpty(DisplayName);
		}
	}
	
	//Dictionary for the supported local commands
	protected Dictionary<string, Action<string>> commandHandlers;

	protected Client() {
		commandHandlers = new() {
			{"/auth", HandleAuthCommand},
			{"/join", HandleJoinCommand},
			{"/rename", HandleRenameCommand},
			{"/getname",HandleGetNameCommand},
			{"/getstate",HandleGetStateCommand},
			{"/help", HandleHelpCommand},
		};
	}
	
	// Run the client
	public abstract void Run(string server, ushort port);

	// Close the connection
	public abstract void Close();

	// Handles receiving data from the server
	protected abstract void ReceiveData();
	
	// Processes the message received from the server
	protected abstract void ProcessMessage(string message);
	
	// Handles processing and sending the data to the server
	public abstract void SendData(string message);

	// Local command handlers
	// /auth sends authentication request to the server 
	protected abstract void HandleAuthCommand(string message);
	// /join sends join request to the server
	protected abstract void HandleJoinCommand(string message);
	// /rename changes the display name of the user
	protected abstract void HandleRenameCommand(string message);
	// /getname prints out the current display name
	protected abstract void HandleGetNameCommand(string message);
	// /getstate prints out the current state of the client
	protected abstract void HandleGetStateCommand(string message);
	// /help prints out supported local commands
	protected abstract void HandleHelpCommand(string message);
	
	// Print supported local commands
	protected static void PrintHelp() {
		Console.WriteLine("Supported local commands:");
		Console.WriteLine("/auth {Username} {Secret} {DisplayName}");
		Console.WriteLine("		Sends AUTH command to the server with the provided parameters");
		Console.WriteLine("/join {ChannelID}");
		Console.WriteLine("		Sends JOIN command to the server with the provided parameters");
		Console.WriteLine("/rename {DisplayName}");
		Console.WriteLine("		Locally changes the display name of the user to be sent with new messages");
		Console.WriteLine("/getname");
		Console.WriteLine("		Prints out the current display name");
		Console.WriteLine("/help");
		Console.WriteLine("		Prints out supported local commands with their parameters and a description");
	}

	// Validates the /rename command, if valid, returns the new display name otherwise returns empty string
	protected static string ValidateRenameCommand(string message) {
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
	protected static string ValidateJoinCommand(string message) {
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
	protected static AuthCommand ValidateAuthCommand(string message) {
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