using System;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Reply : Message {
	public bool Result { get; set; }
	public ushort RefMessageId { get; set; }
	private string _messageContents = String.Empty;

	private string MessageContents {
		get => _messageContents;
		set {
			if (!CheckMessageContents(value)) {
				throw new ArgumentException("MessageContents can only contain printable characters and be 1400 characters long");
			}
			
			_messageContents = value;	
		}
	}

	public Reply() { }

	public Reply(bool result, ushort refMessageId, string messageContents) {
		Result = result;
		RefMessageId = refMessageId;
		MessageContents = messageContents;
	}
	
	public Reply(MessageType type, ushort messageId, bool result, ushort refMessageId, string messageContents) : base(type, messageId) {
		Result = result;
		RefMessageId = refMessageId;
		MessageContents = messageContents;
	}

	// Prints the message to the console
	public void PrintMessage() {
		Console.Error.WriteLine($"{(Result ? "Success" : "Failure")}: {MessageContents}");
	}

	public override string CreateTcpMessage() {
		if (string.IsNullOrEmpty(MessageContents)) {
			throw new ArgumentException("MessageContents is empty, cannot send TCP message.");
		}
		
		// REPLY {"OK"|"NOK"} IS {MessageContent}\r\n
		return $"REPLY {(Result ? "OK" : "NOK")} IS {MessageContents}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		// TODO: Implement UDP message creation
		throw new NotImplementedException();
	}

	public override void DeserializeTcpMessage(string message) {
		// REPLY {"OK"|"NOK"} IS {MessageContent}\r\n
		const string pattern = @"^REPLY (OK|NOK) IS (.*)$";
		var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
		
		if (!match.Success) {
			throw new ArgumentException("Invalid message format.");
		}

		Type = MessageType.Reply;
		if (match.Groups[1].Value is "OK" or "ok") {
			Result = true;
		} else {
			Result = false;
		}
		MessageContents = match.Groups[2].Value;
	}

	public override void DeserializeUdpMessage(byte[] message) {
		// TODO: Implement UDP message deserialization
		throw new NotImplementedException();
	}
}