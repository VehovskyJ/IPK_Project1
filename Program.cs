using System.Text;
using IPK_Project1.Enums;
using Microsoft.VisualBasic;

namespace IPK_Project1;

static class Program {
	static void Main(string[] args) {
		CliArguments cliArguments = Cli.Parse(args);

		if (cliArguments.TransportProtocol == TransportProtocol.Tcp) {
			// Create TCP client
			Client tcpClient = new Client();
			if (string.IsNullOrEmpty(cliArguments.ServerAddress)) {
				Error.Print("Missing required arguments: Server Address.");
				Environment.Exit(1);
			} else {
				// Else is not needed, since the program exits on error,
				// but the warning about possible null persist when else is not present
				tcpClient.RunTcp(cliArguments.ServerAddress, cliArguments.ServerPort);
			}
			// Read data from the console
			try {
				while (true) {
					string? message = Console.ReadLine();
					if (!string.IsNullOrEmpty(message)) {
						tcpClient.SendTcpData(Encoding.ASCII.GetBytes(message));
					} else {
						Error.Print("Input cannot be empty.");
					}
				}
			} catch (Exception e) {
				Error.Print(e.Message);
			} finally {
				tcpClient.CloseTcp();
			}
		} else {
			// TODO: Implement UDP client
		}
	}
}