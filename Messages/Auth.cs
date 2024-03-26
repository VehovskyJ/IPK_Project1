namespace IPK_Project1.Messages;

public class Auth : Message {
	public required string Username { get; set; }
	public required string DisplayName { get; set; }
	public required string Secret { get; set; }
}