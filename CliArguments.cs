namespace IPK_Project1;

public class CliArguments {
	public TransportProtocol TransportProtocol { get; set; }
	public string ServerAddress { get; set; }
	public ushort ServerPort { get; set; }
	public ushort UdpTimeout { get; set; }
	public byte MaxRetransmissions { get; set; }
}