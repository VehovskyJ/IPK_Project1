namespace IPK_Project1.Messages;

public class Reply : Message {
	public required bool Result { get; set; }
	public required UInt16 Ref_MessageID { get; set; }
	public required string MessageContents { get; set; }
}