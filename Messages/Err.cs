using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Err : Msg {
	public Err() { }

	public Err(string displayName, string messageContents) : base(displayName, messageContents) { }

	public Err(MessageType type, ushort messageId, string displayName, string messageContents) : base(type, messageId, displayName, messageContents) { }
	
	public new void PrintMessage() {
		Console.Error.WriteLine($"ERR FROM {DisplayName}: {MessageContents}");
	}

	public override string CreateTcpMessage() {
		// ERR FROM {DisplayName} IS {MessageContent}\r\n
		return $"ERR FROM {DisplayName} IS {MessageContents}\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}
}