using System;
using IPK_Project1.Enums;

namespace IPK_Project1;

public class Cli {
	public TransportProtocol TransportProtocol { get; set; }
	public string? ServerAddress { get; set; }
	public ushort ServerPort { get; set; }
	public ushort UdpTimeout { get; set; }
	public byte MaxRetransmissions { get; set; }

	public Cli() {
		// Default values
		TransportProtocol = TransportProtocol.None;
		ServerPort = 4567;
		UdpTimeout = 250;
		MaxRetransmissions = 3;
	}

	// Parse command line arguments
	public void Parse(string[] args) {
		if (args.Length == 0 || args[0] == "-h") {
			Help();
			Environment.Exit(0);
		}

		for (int i = 0; i < args.Length; i++) {
			// Check if there is a value for the argument
			if (i + 1 >= args.Length) {
				Error.Print("Missing value for argument.");
				Environment.Exit(1);
			}

			switch (args[i]) {
				case "-t":
					switch (args[++i]) {
						case "tcp":
							TransportProtocol = TransportProtocol.Tcp;
							break;
						case "udp":
							TransportProtocol = TransportProtocol.Udp;
							break;
						default:
							Error.Print("Missing value for argument.");
							Environment.Exit(1);
							break;
					}

					break;
				case "-s":
					ServerAddress = args[++i];
					break;
				case "-p":
					try {
						ServerPort = ushort.Parse(args[++i]);
					} catch (FormatException) {
						Error.Print("Invalid format for server port.");
						Environment.Exit(1);
					} catch (OverflowException) {
						Error.Print("Server port must be between 0 and 65535.");
						Environment.Exit(1);
					}

					break;
				case "-d":
					try {
						UdpTimeout = ushort.Parse(args[++i]);
					} catch (Exception) {
						Error.Print("Invalid input for UDP timeout.");
						Environment.Exit(1);
					}

					break;
				case "-r":
					try {
						MaxRetransmissions = byte.Parse(args[++i]);
					} catch (Exception) {
						Error.Print("Invalid input for UDP retransmissions.");
						Environment.Exit(1);
					}

					break;
				default:
					Help();
					Environment.Exit(0);
					break;
			}
		}

		// Check if all required arguments are set
		if (string.IsNullOrEmpty(ServerAddress) || TransportProtocol == TransportProtocol.None) {
			Error.Print("Missing required arguments.");
			Environment.Exit(1);
		}
	}

	// Print help message
	private static void Help() {
		Console.WriteLine(
			"Usage: ipk24chat-client [-h] [-t transport_protocol] [-s server_address] [-p server_port] [-d udp_timeout] [-r max_retransmissions]");
		Console.WriteLine("-t: Transport protocol (tcp or udp)");
		Console.WriteLine("-s: Server address (IP or hostname)");
		Console.WriteLine("-p: Server port (number)");
		Console.WriteLine("-d: UDP confirmation timeout (number in milliseconds)");
		Console.WriteLine("-r: Maximum number of UDP retransmissions (number)");
	}
}