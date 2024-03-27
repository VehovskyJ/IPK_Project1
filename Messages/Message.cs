using System.Text.RegularExpressions;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public abstract class Message : IPrepareForSending {
	public required MessageType Type { get; set; }
	public required UInt16 MessageId { get; set; }

	protected Message() { }

	protected Message(MessageType type, ushort messageId) {
		Type = type;
		MessageId = messageId;
	}

	public abstract string CreateTcpMessage();
	public abstract byte[] CreateUdpMessage();

	// The following methods check the input for invalid characters or length
	protected bool CheckMessageContents(string value) {
		// MessageContents should be at most 1400 characters long and contain only printable characters
		if (value.Length > 1400 || value.Any(char.IsControl)) {
			return false;
		}

		return true;
	}
	
	protected bool CheckDisplayName(string value) {
		// DisplayName should be at most 20 characters long and contain only printable characters
		if (value.Length > 20 || value.Any(char.IsControl)) {
			return false;
		}

		return true;
	}
	
	protected bool CheckUsernameOrChannelID(string value) {
		// Username should be at most 20 characters long and contain only characters [A-Za-z0-9-]
		if (value.Length > 20 || !Regex.IsMatch(value, @"^[A-Za-z0-9-]+$")) {
			return false;
		}

		return true;
	}
	
	protected bool CheckSecret(string value) {
		// Secret should be at most 128 characters long and contain only characters [A-Za-z0-9-]
		if (value.Length > 128 || !Regex.IsMatch(value, @"^[A-Za-z0-9-]+$")) {
			return false;
		}

		return true;
	}
}