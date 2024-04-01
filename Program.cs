using System;
using IPK_Project1.Enums;
using IPK_Project1.Transport;

namespace IPK_Project1;

static class Program {
	static void Main(string[] args) {
		Cli cli = new();
		try {
			cli.Parse(args);

			// Run the client based on the transport protocol
			if (cli.TransportProtocol == TransportProtocol.Tcp) {
				Tcp tcp = new Tcp();
				RunClient(tcp, cli);
			} else {
				Udp udp = new Udp();
				RunClient(udp, cli);
			}
		} catch (Exception e) {
			Error.Print(e.Message);
			Environment.Exit(1);
		}
	}

	static void RunClient(Client client, Cli cli) {
		if (string.IsNullOrEmpty(cli.ServerAddress)) {
			Error.Print("Missing required arguments: Server Address.");
			Environment.Exit(1);
		}

		// Run the client
		try {
			client.Run(cli);
		} catch (Exception e) {
			Error.Print(e.Message);
			Environment.Exit(1);
		}
		
		// Handle Ctrl+C
		Console.CancelKeyPress += (sender, eventArgs) => {
			eventArgs.Cancel = true;
			client.Close();
			Environment.Exit(0);
		};
		
		// Read data from the console and send it to the server
		try {
			while (true) {
				string? message = Console.ReadLine();
				if (!string.IsNullOrEmpty(message)) {
					client.SendData(message);
				} else {
					Error.Print("Input cannot be empty.");
				}
			}
		} catch (Exception e) {
			Error.Print(e.Message);
		} finally {
			client.Close();
		}
	}
}