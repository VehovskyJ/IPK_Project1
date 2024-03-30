using System;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Msg : Message {
	private string _displayName = string.Empty;
	private string _messageContents = string.Empty;

	public string DisplayName {
		get => _displayName;
		set {
			if (!CheckDisplayName(value)) {
				throw new ArgumentException("DisplayName can only contain printable characters and be 20 characters long");
			}
			
			_displayName = value;	
		}
	}
	
	public string MessageContents {
		get => _messageContents;
		set {
			if (!CheckMessageContents(value)) {
				throw new ArgumentException("MessageContents can only contain printable characters and be 1400 characters long");
			}
			
			_messageContents = value;	
		}
	}

	public Msg() { }

	public Msg(string displayName, string messageContents) {
		DisplayName = displayName;
		MessageContents = messageContents;
	}
	
	public Msg(MessageType type, ushort messageId, string displayName, string messageContents) : base(type, messageId) {
		DisplayName = displayName;
		MessageContents = messageContents;
	}

	// Print message to console
	public void PrintMessage() {
		Console.WriteLine($"{DisplayName}: {MessageContents}");
	}

	public override string CreateTcpMessage() {
        if (string.IsNullOrEmpty(DisplayName) || string.IsNullOrEmpty(MessageContents)) {
        	throw new ArgumentException("DisplayName or MessageContents is empty, cannot send TCP message.");
        }
        
		// MSG FROM {DisplayName} IS {MessageContent}\r\n
		return $"MSG FROM {DisplayName} IS {MessageContents}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}

	public override void DeserializeTcpMessage(string message) {
		// MSG FROM {DisplayName} IS {MessageContent}\r\n
		const string pattern = @"^MSG FROM (?<DisplayName>\S+) IS (?<MessageContent>.+)$";
		var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);

		if (!match.Success) {
			throw new ArgumentException("Invalid message format");
		}

		Type = MessageType.Msg;
		DisplayName = match.Groups["DisplayName"].Value;
		MessageContents = match.Groups["MessageContent"].Value;
	}

	public override void DeserializeUdpMessage(byte[] message) {
		throw new NotImplementedException();
	}
}