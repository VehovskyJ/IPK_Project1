namespace IPK_Project1.Messages;

public class Join : Message {
	private string _channelID;
	private string _displayName;
	
	public required string ChannelID {
		get => _channelID;
		set {
			if (CheckUsernameOrChannelID(value)) {
				throw new ArgumentException("ChannelID can only contain characters [A-z0-9-] and be 20 characters long");
			}
			
			_channelID = value;	
		}
	}
	
	public required string DisplayName {
		get => _displayName;
		set {
			if (CheckDisplayName(value)) {
				throw new ArgumentException("DisplayName can only contain printable characters and be 20 characters long");
			}
			
			_displayName = value;	
		}
	}
	public override string CreateTcpMessage() {
		// JOIN {ChannelID} AS {DisplayName}\r\n
		return $"JOIN {ChannelID} AS {DisplayName}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}
}