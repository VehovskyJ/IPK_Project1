using System;
using System.Text;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Join : Message {
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

	public override string CreateTcpMessage() {
		if (string.IsNullOrEmpty(ChannelId) || string.IsNullOrEmpty(DisplayName)) {
			throw new ArgumentException("ChannelId or DisplayName is empty, cannot send TCP message.");
		}
		
		// JOIN {ChannelID} AS {DisplayName}\r\n
		return $"JOIN {ChannelId} AS {DisplayName}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		//    1 byte       2 bytes
		// 	+--------+--------+--------+-----~~-----+---+-------~~------+---+
		// 	|  0x03  |    MessageID    |  ChannelID | 0 |  DisplayName  | 0 |
		// 	+--------+--------+--------+-----~~-----+---+-------~~------+---+
		if (string.IsNullOrEmpty(ChannelId) || string.IsNullOrEmpty(DisplayName)) {
			throw new ArgumentException("ChannelId or DisplayName is empty, cannot send TCP message.");
		}
		
		byte[] messageIdBytes = BitConverter.GetBytes(MessageId);
		byte[] channelIdBytes = Encoding.ASCII.GetBytes(ChannelId);
		byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
		byte[] zeroByte = new byte[1];
		
		byte[] udpMessage = new byte[1 + messageIdBytes.Length + channelIdBytes.Length +  1 + displayNameBytes.Length + 1];
		udpMessage[0] = (byte)MessageType.Join;
		
		Buffer.BlockCopy(messageIdBytes, 0, udpMessage, 1, messageIdBytes.Length);
		Buffer.BlockCopy(channelIdBytes, 0, udpMessage, 3, channelIdBytes.Length);
		Buffer.BlockCopy(zeroByte, 0, udpMessage, 3 + channelIdBytes.Length, zeroByte.Length);
		Buffer.BlockCopy(displayNameBytes, 0, udpMessage, 4 + channelIdBytes.Length, displayNameBytes.Length);
		Buffer.BlockCopy(zeroByte, 0, udpMessage, 4 + channelIdBytes.Length + displayNameBytes.Length, zeroByte.Length);

		return udpMessage;
	}

	// Message is not expected to be received
	public override void DeserializeTcpMessage(string message) {
		throw new NotImplementedException();
	}

	// Message is not received by the client
	public override void DeserializeUdpMessage(byte[] message) {
		throw new NotImplementedException();
	}
}