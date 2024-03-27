using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Reply : Message {
	public required bool Result { get; set; }
	public required UInt16 Ref_MessageID { get; set; }
	private string _messageContents;

	public required string MessageContents {
		get => _messageContents;
		set {
			if (CheckMessageContents(value)) {
				throw new ArgumentException("MessageContents can only contain printable characters and be 1400 characters long");
			}
			
			_messageContents = value;	
		}
	}

	public Reply() { }

	public Reply(bool result, ushort refMessageId, string messageContents) {
		Result = result;
		Ref_MessageID = refMessageId;
		MessageContents = messageContents;
	}
	
	public Reply(MessageType type, ushort messageId, bool result, ushort refMessageId, string messageContents) : base(type, messageId) {
		Result = result;
		Ref_MessageID = refMessageId;
		MessageContents = messageContents;
	}

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