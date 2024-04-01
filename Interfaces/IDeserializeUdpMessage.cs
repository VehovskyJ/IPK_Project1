namespace IPK_Project1.Interfaces;

public interface IDeserializeUdpMessage {
	// Deserializes a UDP message from byte array.
	void DeserializeUdpMessage(byte[] message);
}