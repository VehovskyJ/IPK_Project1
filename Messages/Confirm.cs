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
		//    1 byte       2 bytes
		//	+--------+--------+--------+
		//	|  0x00  |  Ref_MessageID  |
		//	+--------+--------+--------+
		byte[] refMessageIdBytes = BitConverter.GetBytes(RefMessageId);
		
		byte[] udpMessage = new byte[1 + refMessageIdBytes.Length];
		udpMessage[0] = (byte)MessageType.Confirm;
		
		Buffer.BlockCopy(refMessageIdBytes, 0, udpMessage, 1, refMessageIdBytes.Length);
		
		return udpMessage;
	}

	// Method is not implemented in TCP
	public override void DeserializeTcpMessage(string message) {
		throw new NotImplementedException();
	}

	public override void DeserializeUdpMessage(byte[] message) {
		if (message == null || message.Length != 3) {
			throw new ArgumentException("Invalid message format");
		}
		
		byte type = message[0];
		ushort refMessageId = BitConverter.ToUInt16(message, 1);

		if (type != (byte)MessageType.Confirm) {
			throw new ArgumentException("Invalid message type");
		}

		Type = MessageType.Confirm;
		RefMessageId = refMessageId;
	}
}