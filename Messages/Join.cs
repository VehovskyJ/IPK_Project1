namespace IPK_Project1.Messages;

public class Join : Message {
	public required string ChannelID { get; set; }
	public required string DisplayName { get; set; }
}