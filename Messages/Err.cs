using System;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Err : Msg {
	public Err() { }

	public Err(string displayName, string messageContents) : base(displayName, messageContents) { }

	public Err(MessageType type, ushort messageId, string displayName, string messageContents) : base(type, messageId, displayName, messageContents) { }
	
	// Print message to console
	public new void PrintMessage() {
		Console.Error.WriteLine($"ERR FROM {DisplayName}: {MessageContents}");
	}

	public override string CreateTcpMessage() {
		if (string.IsNullOrEmpty(DisplayName) || string.IsNullOrEmpty(MessageContents)) {
			throw new ArgumentException("DisplayName or MessageContents is empty, cannot send TCP message.");
		}
		
		// ERR FROM {DisplayName} IS {MessageContent}\r\n
		return $"ERR FROM {DisplayName} IS {MessageContents}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		// TODO: Implement
		throw new NotImplementedException();
	}
	
	public override void DeserializeTcpMessage(string message) {
		// ERR FROM {DisplayName} IS {MessageContent}\r\n
		const string pattern = @"^ERR FROM (?<DisplayName>\S+) IS (?<MessageContent>.+)$";
		Match match = Regex.Match(message, pattern);
		
		if (!match.Success) {
			throw new ArgumentException("Invalid message format");
		}
		
		Type = MessageType.Err;
		DisplayName = match.Groups["DisplayName"].Value;
		MessageContents = match.Groups["MessageContent"].Value;
	}

	public override void DeserializeUdpMessage(byte[] message) {
		// TODO: Implement
		throw new NotImplementedException();
	}
}