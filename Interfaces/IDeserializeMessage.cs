namespace IPK_Project1.Interfaces;

public interface IDeserializeMessage {
	// DeserializeTcpMessage parses a message from a string
	void DeserializeTcpMessage(string message);
	
	// DeserializeUdpMessage parses a message from a byte array
	void DeserializeUdpMessage(byte[] message);
}