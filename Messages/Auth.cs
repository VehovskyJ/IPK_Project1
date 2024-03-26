namespace IPK_Project1.Messages;

public class Auth : Message {
	public required string Username { get; set; }
	public required string DisplayName { get; set; }
	public required string Secret { get; set; }
	public override string CreateTcpMessage() {
		// AUTH {Username} AS {DisplayName} USING {Secret}\r\n
		return $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}
}