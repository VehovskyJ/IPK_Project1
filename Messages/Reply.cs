namespace IPK_Project1.Messages;

public class Reply : Message {
	public required bool Result { get; set; }
	public required UInt16 Ref_MessageID { get; set; }
	public required string MessageContents { get; set; }
	public void PrintMessage() {
		Console.Error.WriteLine($"{(Result ? "Success" : "Failure")}: {MessageContents}");
	}

	public override string CreateTcpMessage() {
		// REPLY {"OK"|"NOK"} IS {MessageContent}\r\n
		return $"REPLY {(Result ? "OK" : "NOK")} IS {MessageContents}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}
}