using System;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Auth : Message {
	private string _username = String.Empty;
	private string _displayName = string.Empty;
	private string _secret = string.Empty;

	public required string Username {
		get => _username;
		set {
			if (!CheckUsernameOrChannelId(value)) {
				throw new ArgumentException("Username can only contain characters [A-z0-9-] and be 20 characters long");
			}
			
			_username = value;	
		}
	}
	
	public required string DisplayName {
		get => _displayName;
		set {
			if (!CheckDisplayName(value)) {
				throw new ArgumentException("DisplayName can only contain printable characters and be 20 characters long");
			}
			
			_displayName = value;	
		}
	}
	
	public required string Secret { 
		get => _secret;
		set {
			if (!CheckSecret(value)) {
				throw new ArgumentException("Secret can only contain characters [A-z0-9-] and be 128 characters long");
			}
			
			_secret = value;
		} 
	}

	public Auth() { }

	public Auth(string username, string displayName, string secret) {
		Username = username;
		DisplayName = displayName;
		Secret = secret;
	}
	
	public Auth(MessageType type, ushort messageId, string username, string displayName, string secret) : base(type, messageId) {
		Username = username;
		DisplayName = displayName;
		Secret = secret;
	}

	public override string CreateTcpMessage() {
		if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(DisplayName) || string.IsNullOrEmpty(Secret)) {
			throw new ArgumentException("Username, DisplayName or Secret is empty, cannot send TCP message.");
		}
		
		// AUTH {Username} AS {DisplayName} USING {Secret}\r\n
		return $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
	}

	// Auth method is not used in UDP
	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}

	// Auth message is not received
	public override void DeserializeTcpMessage(string message) {
		throw new NotImplementedException();
	}

	// Auth message is not received
	public override void DeserializeUdpMessage(byte[] message) {
		throw new NotImplementedException();
	}
}