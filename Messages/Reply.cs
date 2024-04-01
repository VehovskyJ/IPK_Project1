using System;
using System.Text;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;
using IPK_Project1.Interfaces;

namespace IPK_Project1.Messages;

public class Reply : Message, IDeserializeTcpMessage, IDeserializeUdpMessage {
	public bool Result { get; set; }
	public ushort RefMessageId { get; set; }
	private string _messageContents = string.Empty;

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

	public void DeserializeTcpMessage(string message) {
		// REPLY {"OK"|"NOK"} IS {MessageContent}\r\n
		const string pattern = @"^REPLY (OK|NOK) IS (.*)(\r\n)$";
		var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
		
		if (!match.Success) {
			throw new ArgumentException("Invalid message format.");
		}

		Type = MessageType.Reply;
		Result = match.Groups[1].Value is "OK" or "ok";
		MessageContents = match.Groups[2].Value;
	}

	public void DeserializeUdpMessage(byte[] message) {
		if (message == null || message.Length < 7) {
			throw new ArgumentException("Invalid message format");
		}
		
		byte type = message[0];
		ushort messageId = BitConverter.ToUInt16(message, 1);
		bool result = message[3] != 0;
		ushort refMessageId = BitConverter.ToUInt16(message, 4);
		
		if (type != (byte)MessageType.Reply) {
			throw new ArgumentException("Invalid message type");
		}
		
		// Find start and end of the message contents
		int messageContentsStart = 6;
		int messageContentsEnd = Array.IndexOf(message, (byte)0, messageContentsStart);
		if (messageContentsEnd < 0) {
			throw new ArgumentException("Invalid message format");
		}

		// Extract MessageContents
		string messageContents = Encoding.ASCII.GetString(message, messageContentsStart, messageContentsEnd - messageContentsStart);

		Type = MessageType.Reply;
		MessageId = messageId;
		Result = result;
		RefMessageId = refMessageId;
		MessageContents = messageContents;
	}
}