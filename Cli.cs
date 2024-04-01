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
				throw new ArgumentException("Missing value for an argument.");
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
							throw new ArgumentException("Missing value for transport protocol.");
					}
					break;
				case "-s":
					ServerAddress = args[++i];
					break;
				case "-p":
					if (!ushort.TryParse(args[++i], out ushort port) || port == 0) {
						throw new ArgumentException("Invalid value for server port.");
					}

					ServerPort = port;
					break;
				case "-d":
					if (!ushort.TryParse(args[++i], out ushort timeout)) {
						throw new ArgumentException("Invalid value for UDP timeout.");
					}

					UdpTimeout = timeout;
					break;
				case "-r":
					if (!byte.TryParse(args[++i], out byte retransmissions)) {
						throw new ArgumentException("Invalid value for UDP retransmissions.");
					}

					MaxRetransmissions = retransmissions;
					break;
				default:
					Help();
					Environment.Exit(0);
					break;
			}
		}

		// Check if all required arguments are set
		if (string.IsNullOrEmpty(ServerAddress)) {
			throw new ArgumentException("Please provide a server address.");
		}

		if (TransportProtocol == TransportProtocol.None) {
			throw new ArgumentException("Please select a transport protocol.");
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