using System;
using System.Collections.Generic;
using System.Text;
using IPK_Project1.Enums;
using IPK_Project1.Interfaces;

namespace IPK_Project1.Messages;

public class Auth : Message, ISerializeTcpMessage, ISerializeUdpMessage {
	private string _username = string.Empty;
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
	
	// Check if all required attributes are set
	private void ValidateMessage() {
		if (!CheckMessageContents(Username) || !CheckMessageContents(DisplayName) || !CheckMessageContents(Secret)) {
			throw new ArgumentException(
				"Username, DisplayName or Secret contain invalid characters or exceed the maximum length.");
		}
	}
	
	public string SerializeTcpMessage() {
		ValidateMessage();
		
		// AUTH {Username} AS {DisplayName} USING {Secret}\r\n
		return $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
	}
	
	public byte[] SerializeUdpMessage() {
		// 	 1 byte       2 bytes
		// +--------+--------+--------+-----~~-----+---+-------~~------+---+----~~----+---+
		// |  0x02  |    MessageID    |  Username  | 0 |  DisplayName  | 0 |  Secret  | 0 |
		// +--------+--------+--------+-----~~-----+---+-------~~------+---+----~~----+---+
		ValidateMessage();
		
		var udpMessage = new List<byte>();
		
		udpMessage.Add((byte)MessageType.Auth);
		udpMessage.AddRange(BitConverter.GetBytes(MessageId));
		udpMessage.AddRange(Encoding.ASCII.GetBytes(Username));
		udpMessage.AddRange(new byte[1]);
		udpMessage.AddRange(Encoding.ASCII.GetBytes(DisplayName));
		udpMessage.AddRange(new byte[1]);
		udpMessage.AddRange(Encoding.ASCII.GetBytes(Secret));
		udpMessage.AddRange(new byte[1]);
		
		return udpMessage.ToArray();
	}
}