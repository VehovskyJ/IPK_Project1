namespace IPK_Project1.Messages;

public class Msg : Message {
	public required string DisplayName { get; set; }
	public required string MessageContents { get; set; }
	public void PrintMessage() {
		Console.WriteLine($"{DisplayName}: {MessageContents}");
	}

	public override string CreateTcpMessage() {
		// MSG FROM {DisplayName} IS {MessageContent}\r\n
		return $"MSG FROM {DisplayName} IS {MessageContents}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}
}