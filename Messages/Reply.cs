using System;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Reply : Message {
	public required bool Result { get; set; }
	public required UInt16 Ref_MessageID { get; set; }
	private string _messageContents;

	public required string MessageContents {
		get => _messageContents;
		set {
			if (CheckMessageContents(value)) {
				throw new ArgumentException("MessageContents can only contain printable characters and be 1400 characters long");
			}
			
			_messageContents = value;	
		}
	}

	public Reply() { }

	public Reply(bool result, ushort refMessageId, string messageContents) {
		Result = result;
		Ref_MessageID = refMessageId;
		MessageContents = messageContents;
	}
	
	public Reply(MessageType type, ushort messageId, bool result, ushort refMessageId, string messageContents) : base(type, messageId) {
		Result = result;
		Ref_MessageID = refMessageId;
		MessageContents = messageContents;
	}

	// Prints the message to the console
	public void PrintMessage() {
		Console.Error.WriteLine($"{(Result ? "Success" : "Failure")}: {MessageContents}");
	}

	public override string CreateTcpMessage() {
		// REPLY {"OK"|"NOK"} IS {MessageContent}\r\n
		return $"REPLY {(Result ? "OK" : "NOK")} IS {MessageContents}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		// TODO: Implement UDP message creation
		throw new NotImplementedException();
	}

	public override void DeserializeTcpMessage(string message) {
		// REPLY {"OK"|"NOK"} IS {MessageContent}\r\n
		string pattern = @"^REPLY (OK|NOK) IS (.*)$";
		var match = Regex.Match(message, pattern);
		
		if (!match.Success) {
			throw new ArgumentException("Invalid message format.");
		}

		if (match.Groups[1].Value == "OK") {
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