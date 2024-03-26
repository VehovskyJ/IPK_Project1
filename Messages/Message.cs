using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Message {
	public required MessageType Type { get; set; }
	public required UInt16 MessageId { get; set; }
}