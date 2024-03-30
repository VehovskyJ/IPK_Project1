using System;
using System.Linq;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;
using IPK_Project1.Interfaces;

namespace IPK_Project1.Messages;

public abstract class Message : ISerializeMessage, IDeserializeMessage {
	public required MessageType Type { get; set; }
	public ushort MessageId { get; set; }

	protected Message() { }

	protected Message(MessageType type, ushort messageId) {
		Type = type;
		MessageId = messageId;
	}

	public abstract string CreateTcpMessage();
	public abstract byte[] CreateUdpMessage();
	public abstract void DeserializeTcpMessage(string message);
	public abstract void DeserializeUdpMessage(byte[] message);

	// The following methods check the input for invalid characters or length
	protected static bool CheckMessageContents(string value) {
		// MessageContents should be at most 1400 characters long and contain only printable characters
		return value.Length <= 1400 && !value.Any(char.IsControl);
	}
	
	protected static bool CheckDisplayName(string value) {
		// DisplayName should be at most 20 characters long and contain only printable characters
		return value.Length <= 20 && !value.Any(char.IsControl);
	}
	
	protected static bool CheckUsernameOrChannelId(string value) {
		// Username should be at most 20 characters long and contain only characters [A-Za-z0-9-]
		return value.Length <= 20 && Regex.IsMatch(value, @"^[A-Za-z0-9-]+$");
	}
	
	protected static bool CheckSecret(string value) {
		// Secret should be at most 128 characters long and contain only characters [A-Za-z0-9-]
		return value.Length <= 128 && Regex.IsMatch(value, @"^[A-Za-z0-9-]+$");
	}
}