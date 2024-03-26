using System.Text;
using IPK_Project1.Enums;

namespace IPK_Project1;

static class Program {
	static void Main(string[] args) {
		CliArguments cliArguments = Cli.Parse(args);

		if (cliArguments.TransportProtocol == TransportProtocol.Tcp) {
			// Create TCP client
			Client tcpClient = new Client();
			tcpClient.RunTcp(cliArguments.ServerAddress, cliArguments.ServerPort);
			
			// Read data from the console
			try {
				while (true) {
					string message = Console.ReadLine();
					tcpClient.SendTcpData(Encoding.ASCII.GetBytes(message));
				}
			} catch (Exception e) {
				Console.WriteLine(e);
			} finally {
				tcpClient.CloseTcp();
			}
		} else {
			// TODO: Implement UDP client
		}
		
	}
}