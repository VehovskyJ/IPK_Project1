using System;
using System.Text.RegularExpressions;
using IPK_Project1.Enums;

namespace IPK_Project1.Messages;

public class Bye : Message {
	public override string CreateTcpMessage() {
		// BYE\r\n
		return "BYE\r\n";
	}

	public override byte[] CreateUdpMessage() {
		throw new NotImplementedException();
	}

	public override void DeserializeTcpMessage(string message) {
		// BYE\r\n
		const string pattern = @"^BYE$";
		var match = Regex.Match(message, pattern);

		if (!match.Success) {
			throw new ArgumentException("Invalid message format");
		}

		Type = MessageType.Bye;
	}

	public override void DeserializeUdpMessage(byte[] message) {
		// TODO: Implement
		throw new NotImplementedException();
	}
}