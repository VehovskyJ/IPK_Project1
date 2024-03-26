namespace IPK_Project1.Messages;

public interface IPrepareForSending {
	// Prepares message for sending over TCP in text format
	string CreateTcpMessage();
	
	// Prepares message for sending over UDP in binary format
	byte[] CreateUdpMessage();
}