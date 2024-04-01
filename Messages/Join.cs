using System;
using System.Collections.Generic;
using System.Text;
using IPK_Project1.Enums;
using IPK_Project1.Interfaces;

namespace IPK_Project1.Messages;

public class Join : Message, ISerializeTcpMessage, ISerializeUdpMessage {
	private string _channelId = string.Empty;
	private string _displayName = string.Empty;
	
	public required string ChannelId {
		get => _channelId;
		set {
			if (!CheckUsernameOrChannelId(value)) {
				throw new ArgumentException("ChannelID can only contain characters [A-z0-9-] and be 20 characters long");
			}
			
			_channelId = value;	
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

	public Join() { }

	public Join(string channelId, string displayName) {
		ChannelId = channelId;
		DisplayName = displayName;
	}
	
	public Join(MessageType type, ushort messageId, string channelId, string displayName) : base(type, messageId) {
		ChannelId = channelId;
		DisplayName = displayName;
	}

	// Check if all required attributes are set
	private void ValidateMessage() {
		if (string.IsNullOrEmpty(ChannelId) || string.IsNullOrEmpty(DisplayName)) {
			throw new ArgumentException("ChannelId or DisplayName is empty, cannot send TCP message.");
		}
	}

	public string SerializeTcpMessage() {
		ValidateMessage();
		
		// JOIN {ChannelID} AS {DisplayName}\r\n
		return $"JOIN {ChannelId} AS {DisplayName}\r\n";
	}

	public byte[] SerializeUdpMessage() {
		//    1 byte       2 bytes
		// 	+--------+--------+--------+-----~~-----+---+-------~~------+---+
		// 	|  0x03  |    MessageID    |  ChannelID | 0 |  DisplayName  | 0 |
		// 	+--------+--------+--------+-----~~-----+---+-------~~------+---+
		ValidateMessage();

		var udpMessage = new List<byte>();
		
		udpMessage.Add((byte)MessageType.Join);
		udpMessage.AddRange(BitConverter.GetBytes(MessageId));
		udpMessage.AddRange(Encoding.ASCII.GetBytes(ChannelId));
		udpMessage.AddRange(new byte[1]);
		udpMessage.AddRange(Encoding.ASCII.GetBytes(DisplayName));
		udpMessage.AddRange(new byte[1]);
		
		return udpMessage.ToArray();
	}
}