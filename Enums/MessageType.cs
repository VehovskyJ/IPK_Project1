namespace IPK_Project1.Enums;

public enum MessageType {
	Confirm = 0x00,
	Reply = 0x01,
	Auth = 0x02,
	Join = 0x03,
	Msg = 0x04,
	Err = 0xFE,
	Bye = 0xFF
}