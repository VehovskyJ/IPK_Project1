using IPK_Project1.Enums;
using IPK_Project1.Transport;

namespace IPK_Project1;

static class Program {
	static void Main(string[] args) {
		Cli cli = new();
		try {
			cli.Parse(args);
		} catch (Exception e) {
			Error.Print(e.Message);
			Environment.Exit(1);
		}
		
		if (cli.TransportProtocol == TransportProtocol.Tcp) {
			// Create TCP client
			Tcp tcp = new Tcp();
			if (string.IsNullOrEmpty(cli.ServerAddress)) {
				Error.Print("Missing required arguments: Server Address.");
				Environment.Exit(1);
			} else {
				// Else is not needed, since the program exits on error,
				// but the warning about possible null persist when else is not present
				try {
					tcp.Run(cli.ServerAddress, cli.ServerPort);
				} catch (Exception e) {
					Error.Print(e.Message);
					Environment.Exit(1);
				}
			}
			
			// Handle Ctrl+C
			Console.CancelKeyPress += (sender, eventArgs) => {
				eventArgs.Cancel = true;
				tcp.Close();
				Environment.Exit(0);
			};
			
			// Read data from the console
			try {
				while (true) {
					string? message = Console.ReadLine();
					if (!string.IsNullOrEmpty(message)) {
						tcp.SendData(message);
					} else {
						Error.Print("Input cannot be empty.");
					}
				}
			} catch (Exception e) {
				Error.Print(e.Message);
			} finally {
				tcp.Close();
			}
		} else {
			// TODO: Implement UDP client
		}
	}
}