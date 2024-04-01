using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;
using IPK_Project1.Interfaces;

namespace IPK_Project1.Messages;

public class Msg : Message, IDeserializeUdpMessage, IDeserializeTcpMessage, ISerializeTcpMessage, ISerializeUdpMessage {
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

	// Check if all required attributes are set
	protected void ValidateMessage() {
		if (string.IsNullOrEmpty(DisplayName) || string.IsNullOrEmpty(MessageContents)) {
			throw new ArgumentException("DisplayName or MessageContents is empty, cannot send TCP message.");
		}
	}
	
	public string SerializeTcpMessage() {
        ValidateMessage();
        
		// MSG FROM {DisplayName} IS {MessageContent}\r\n
		return $"MSG FROM {DisplayName} IS {MessageContents}\r\n";
	}

	public byte[] SerializeUdpMessage() {
		//   1 byte       2 bytes
		// +--------+--------+--------+-------~~------+---+--------~~---------+---+
		// |  0x04  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 |
		// +--------+--------+--------+-------~~------+---+--------~~---------+---+
		ValidateMessage();

		var udpMessage = new List<byte>();
		
		udpMessage.Add((byte)MessageType.Msg);
		udpMessage.AddRange(BitConverter.GetBytes(MessageId));
		udpMessage.AddRange(Encoding.ASCII.GetBytes(DisplayName));
		udpMessage.AddRange(new byte[1]);
		udpMessage.AddRange(Encoding.ASCII.GetBytes(MessageContents));
		udpMessage.AddRange(new byte[1]);
		
		return udpMessage.ToArray();
	}

	public void DeserializeTcpMessage(string message) {
		// MSG FROM {DisplayName} IS {MessageContent}\r\n
		const string pattern = @"^MSG FROM (?<DisplayName>\S+) IS (?<MessageContent>.+)(\r\n)$";
		var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);

		if (!match.Success) {
			throw new ArgumentException("Invalid message format");
		}

		Type = MessageType.Msg;
		DisplayName = match.Groups["DisplayName"].Value;
		MessageContents = match.Groups["MessageContent"].Value;
	}

	public void DeserializeUdpMessage(byte[] message) {
		if (message == null || message.Length < 7) {
			throw new ArgumentException("Invalid message format");
		}
		
		byte type = message[0];
		ushort messageId = BitConverter.ToUInt16(message, 1);
		
		if (type != (byte)MessageType.Msg) {
			throw new ArgumentException("Invalid message type");
		}

		// Find start and end of the DisplayName
		int displayNameStart = 3;
		int displayNameEnd = Array.IndexOf(message, (byte)0, displayNameStart);
		if (displayNameEnd < 0) {
			throw new ArgumentException("Invalid message format");
		}
		
		// Find start and end of the MessageContents
		int messageContentsStart = displayNameEnd + 1;
		int messageContentsEnd = Array.IndexOf(message, (byte)0, messageContentsStart);
		if (messageContentsEnd < 0) {
			throw new ArgumentException("Invalid message format");
		}
		
		// Extract DisplayName and MessageContents
		string displayName = Encoding.ASCII.GetString(message, displayNameStart, displayNameEnd - displayNameStart);
		string messageContents = Encoding.ASCII.GetString(message, messageContentsStart, messageContentsEnd - messageContentsStart);

		Type = MessageType.Msg;
		MessageId = messageId;
		DisplayName = displayName;
		MessageContents = messageContents;
	}
}