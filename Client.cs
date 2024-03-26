using System.Net.Sockets;
using System.Text;

namespace IPK_Project1;

public class Client {
	private TcpClient _client = new TcpClient();
	
	// Run the TCP client
	public void RunTcp(string server, ushort port) {
		try {
			// Connect to the server
			_client.Connect(server, port);
		} catch (Exception e) {
			Console.WriteLine($"Error connecting to the server: {e.Message}");
			return;
		}

		// Create thread for receiving TCP data
		Thread receive = new Thread(() => ReceiveTcpData());
		receive.Start();
	}

	// Close the TCP connection
	public void CloseTcp() {
		_client.Close();
	}
	
	// Receive data from the server
	// This method does not further process the data
	private void ReceiveTcpData() {
		try {
			// Read byte stream from the server
			NetworkStream stream = _client.GetStream();
			byte[] data = new byte[2048];

			while (true) {
				int bytesRead = stream.Read(data, 0, data.Length);
				if (bytesRead > 0) {
					string receivedMessage = Encoding.ASCII.GetString(data, 0, bytesRead);
					// TODO: process message
					Console.Write(receivedMessage);
				}
			}

		} catch (Exception e) {
			Console.WriteLine(e);
		}
	}

	// Send data to the server
	public void SendTcpData(byte[] data) {
		NetworkStream stream = _client.GetStream();
		stream.Write(data, 0, data.Length);
	}
	
}