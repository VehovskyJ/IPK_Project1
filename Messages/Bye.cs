using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Bye : Message {
	public string CreateTcpMessage() {
		// BYE\r\n
		return "BYE\r\n";
	}

	public byte[] CreateUdpMessage() {
		//    1 byte       2 bytes
		// 	+--------+--------+--------+
		// 	|  0xFF  |    MessageID    |
		// 	+--------+--------+--------+
		var udpMessage = new List<byte>();
		
		udpMessage.Add((byte)MessageType.Bye);
		udpMessage.AddRange(BitConverter.GetBytes(MessageId));
		
		return udpMessage.ToArray();
	}

	public void DeserializeTcpMessage(string message) {
		// BYE\r\n
		const string pattern = @"^BYE(\r\n)$";
		var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);

		if (!match.Success) {
			throw new ArgumentException("Invalid message format");
		}

		Type = MessageType.Bye;
	}

	public void DeserializeUdpMessage(byte[] message) {
		if (message == null || message.Length < 3) {
			throw new ArgumentException("Invalid message format");
		}
		
		byte type = message[0];
		ushort messageId = BitConverter.ToUInt16(message, 1);
		
		if (type != (byte)MessageType.Bye) {
			throw new ArgumentException("Invalid message type");
		}

		Type = MessageType.Bye;
		MessageId = messageId;
	}
}