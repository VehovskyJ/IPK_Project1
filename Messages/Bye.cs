namespace IPK_Project1.Messages;

public class Bye : Message {
	public override string CreateTcpMessage() {
		// BYE\r\n
		return "BYE\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}
}