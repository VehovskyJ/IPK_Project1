using System;
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

	public override string CreateTcpMessage() {
		if (string.IsNullOrEmpty(DisplayName) || string.IsNullOrEmpty(MessageContents)) {
			throw new ArgumentException("DisplayName or MessageContents is empty, cannot send TCP message.");
		}
		
		// ERR FROM {DisplayName} IS {MessageContent}\r\n
		return $"ERR FROM {DisplayName} IS {MessageContents}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		//   1 byte       2 bytes
		// +--------+--------+--------+-------~~------+---+--------~~---------+---+
		// |  0x04  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 |
		// +--------+--------+--------+-------~~------+---+--------~~---------+---+
		if (string.IsNullOrEmpty(DisplayName) || string.IsNullOrEmpty(MessageContents)) {
			throw new ArgumentException("DisplayName or MessageContents is empty, cannot send TCP message.");
		}
		
		byte[] messageIdBytes = BitConverter.GetBytes(MessageId);
		byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
		byte[] messageContentsBytes = Encoding.ASCII.GetBytes(MessageContents);
		byte[] zeroByte = new byte[1];
		
		byte[] udpMessage = new byte[1 + messageIdBytes.Length + displayNameBytes.Length + 1 + messageContentsBytes.Length + 1];
		udpMessage[0] = (byte)MessageType.Err;
		
		Buffer.BlockCopy(messageIdBytes, 0, udpMessage, 1, messageIdBytes.Length);
		Buffer.BlockCopy(displayNameBytes, 0, udpMessage, 3, displayNameBytes.Length);
		Buffer.BlockCopy(zeroByte, 0, udpMessage, 3 + displayNameBytes.Length, zeroByte.Length);
		Buffer.BlockCopy(messageContentsBytes, 0, udpMessage, 4 + displayNameBytes.Length, messageContentsBytes.Length);
		Buffer.BlockCopy(zeroByte, 0, udpMessage, 4 + displayNameBytes.Length + messageContentsBytes.Length, zeroByte.Length);

		return udpMessage;
	}
	
	public override void DeserializeTcpMessage(string message) {
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

	public override void DeserializeUdpMessage(byte[] message) {
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