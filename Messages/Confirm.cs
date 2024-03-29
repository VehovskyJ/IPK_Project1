using System;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Confirm : Message {
	public required ushort RefMessageId { get; set; }

	public Confirm() { }

	public Confirm(ushort refMessageId) {
		RefMessageId = refMessageId;
	}
	
	public Confirm(MessageType type, ushort messageId, ushort refMessageId) : base(type, messageId) {
		RefMessageId = refMessageId;
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