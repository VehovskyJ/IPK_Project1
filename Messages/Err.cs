using System;
using System.Collections.Generic;
using System.Text;
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

	public new string SerializeTcpMessage() {
		ValidateMessage();
		
		// ERR FROM {DisplayName} IS {MessageContent}\r\n
		return $"ERR FROM {DisplayName} IS {MessageContents}\r\n";
	}

	public new byte[] SerializeUdpMessage() {
		//   1 byte       2 bytes
		// +--------+--------+--------+-------~~------+---+--------~~---------+---+
		// |  0x04  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 |
		// +--------+--------+--------+-------~~------+---+--------~~---------+---+
		ValidateMessage();

		var udpMessage = new List<byte>();
		
		udpMessage.Add((byte)MessageType.Err);
		udpMessage.AddRange(BitConverter.GetBytes(MessageId));
		udpMessage.AddRange(Encoding.ASCII.GetBytes(DisplayName));
		udpMessage.AddRange(new byte[1]);
		udpMessage.AddRange(Encoding.ASCII.GetBytes(MessageContents));
		udpMessage.AddRange(new byte[1]);
		
		return udpMessage.ToArray();
	}
	
	public new void DeserializeTcpMessage(string message) {
		// ERR FROM {DisplayName} IS {MessageContent}\r\n
		const string pattern = @"^ERR FROM (?<DisplayName>\S+) IS (?<MessageContent>.+)(\r\n)$";
		Match match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
		
		if (!match.Success) {
			throw new ArgumentException("Invalid message format");
		}
		
		Type = MessageType.Err;
		DisplayName = match.Groups["DisplayName"].Value;
		MessageContents = match.Groups["MessageContent"].Value;
	}

	public new void DeserializeUdpMessage(byte[] message) {
		if (message == null || message.Length < 7) {
			throw new ArgumentException("Invalid message format");
		}
		
		byte type = message[0];
		ushort messageId = BitConverter.ToUInt16(message, 1);
		
		if (type != (byte)MessageType.Err) {
			throw new ArgumentException("Invalid message type");
		}

		// Find start and end of the display name
		int displayNameStart = 3;
		int displayNameEnd = Array.IndexOf(message, (byte)0, displayNameStart);
		if (displayNameEnd < 0) {
			throw new ArgumentException("Invalid message format");
		}
		
		// Find start and end of the message contents
		int messageContentsStart = displayNameEnd + 1;
		int messageContentsEnd = Array.IndexOf(message, (byte)0, messageContentsStart);
		if (messageContentsEnd < 0) {
			throw new ArgumentException("Invalid message format");
		}
		
		// Extract the information
		string displayName = Encoding.ASCII.GetString(message, displayNameStart, displayNameEnd - displayNameStart);
		string messageContents = Encoding.ASCII.GetString(message, messageContentsStart, messageContentsEnd - messageContentsStart);

		Type = MessageType.Err;
		MessageId = messageId;
		DisplayName = displayName;
		MessageContents = messageContents;
	}
}