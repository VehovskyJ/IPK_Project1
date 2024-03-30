namespace IPK_Project1.Enums;

public enum State {
	Default, // Default state, does nothing
	Authenticating, // Authenticating state, waiting for the server
	AuthenticationFailed, // Authentication failed, got !REPLY
	Open, // Client is connected to the server
}