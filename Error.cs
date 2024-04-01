using System;

namespace IPK_Project1;

public class Error {
	// Print the error message in the required format
	public static void Print(string message) {
		Console.Error.WriteLine($"ERR: {message}");
	}
}