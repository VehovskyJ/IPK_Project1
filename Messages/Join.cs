namespace IPK_Project1.Messages;

public class Join : Message {
	public required string ChannelID { get; set; }
	public required string DisplayName { get; set; }
	public override string CreateTcpMessage() {
		// JOIN {ChannelID} AS {DisplayName}\r\n
		return $"JOIN {ChannelID} AS {DisplayName}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}
}