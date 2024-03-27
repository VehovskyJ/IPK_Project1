using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Confirm : Message {
	public required UInt16 Ref_MesssageID { get; set; }

	public Confirm() { }

	public Confirm(ushort refMesssageId) {
		Ref_MesssageID = refMesssageId;
	}
	
	public Confirm(MessageType type, ushort messageId, ushort refMesssageId) : base(type, messageId) {
		Ref_MesssageID = refMesssageId;
	}

	// Method is not implemented in TCP
	public override string CreateTcpMessage() {
		throw new NotImplementedException();
	}

	public override byte[] CreateUdpMessage() {
		// TODO: Implement
		throw new NotImplementedException();
	}

	// Method is not implemented in TCP
	public override void DeserializeTcpMessage(string message) {
		throw new NotImplementedException();
	}

	public override void DeserializeUdpMessage(byte[] message) {
		// TODO: Implement
		throw new NotImplementedException();
	}
}