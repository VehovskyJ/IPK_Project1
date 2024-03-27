using System;

namespace IPK_Project1;

public class Error {
	public static void Print(string message) {
		Console.Error.WriteLine($"ERR: {message}");
	}
}