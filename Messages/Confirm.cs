using System;
using IPK_Project1.Enums;
using IPK_Project1.Interfaces;

namespace IPK_Project1.Messages;

public class Confirm : Message, ISerializeUdpMessage, IDeserializeUdpMessage {
	public ushort RefMessageId { get; set; }

	public Confirm() { }

	public Confirm(ushort refMessageId) {
		RefMessageId = refMessageId;
	}
	
	public Confirm(MessageType type, ushort messageId, ushort refMessageId) : base(type, messageId) {
		RefMessageId = refMessageId;
	}

	public byte[] SerializeUdpMessage() {
		//    1 byte       2 bytes
		//	+--------+--------+--------+
		//	|  0x00  |  Ref_MessageID  |
		//	+--------+--------+--------+
		var udpMessage = new List<byte>();
		
		udpMessage.Add((byte)MessageType.Confirm);
		udpMessage.AddRange(BitConverter.GetBytes(RefMessageId));
		
		return udpMessage.ToArray();
	}

	public void DeserializeUdpMessage(byte[] message) {
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