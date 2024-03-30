using System;
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
		// TODO: Implement UDP message serialization
		throw new NotImplementedException();
	}

	// Message is not expected to be received
	public override void DeserializeTcpMessage(string message) {
		// // JOIN {ChannelID} AS {DisplayName}\r\n
		// const string pattern = @"^JOIN (?<ChannelID>\S+) AS (?<DisplayName>\S+)$";
		// var match = Regex.Match(message, pattern);
		//
		// if (!match.Success) {
		// 	throw new ArgumentException("Invalid message format");
		// }
		//
		// ChannelId = match.Groups["ChannelID"].Value;
		// DisplayName = match.Groups["DisplayName"].Value;
	}

	public override void DeserializeUdpMessage(byte[] message) {
		// TODO: Implement UDP message deserialization
		throw new NotImplementedException();
	}
}