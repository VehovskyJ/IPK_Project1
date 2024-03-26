using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Confirm : Message {
	public required UInt16 Ref_MesssageID { get; set; }
}