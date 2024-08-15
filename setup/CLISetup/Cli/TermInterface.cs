using System;
using System.IO;
using Setup;

namespace CliSetup {
	public class TermInterface : ITaskInterface {
		int maxProgress = 0;
		string status = "";
		string currentText = "";

		public void SetMaxProgress(int max) {
			this.maxProgress = max;
		}

		public void SetProgress(int progress) {
			if (Console.IsOutputRedirected)
				return;
			Console.WriteLine($"{status}: {Math.Round((float)(progress / maxProgress * 100))}%");
		}

		public void SetStatus(string status) {
			Console.Title = "tML Setup Tool: " + status;
			this.status = status;
		}

		public void Message(string message) {
			Console.WriteLine(message);
		}

		public void Warning(string warning) {
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Yellow;
			Console.Write("Warning");
			Console.ResetColor();
			Console.Write(": " + warning);
		}

		public void Error(string error) {
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Red;
			Console.Write("Error");
			Console.ResetColor();
			Console.Write(": " + error);
		}

		// Despite the fact that SelectDialog and FileSelectDialog return bools, they should never actually return false as the behavior doesn't make sense for a CLI application but is useful in a GUI context. If someone mistypes something they shouldn't have to restart the application entirely.
		public bool SelectDialog<T>(string request, out T result) {
			result = default!;
			if (typeof(T) == typeof(int)) {
				while (true) {
					Console.Write(request + " (num): ");
					string? input = Console.ReadLine();

					if (int.TryParse(input, out int nInput)) {
						result = (T)(object)nInput;
						return true;
					}
					else
						Console.WriteLine("Enter a number");
				}
			}
			else if (typeof(T) == typeof(string)) {
				while (true) {
					Console.Write(request + ": ");
					string? input = Console.ReadLine();

					if (!string.IsNullOrEmpty(input)) {
						result = (T)(object)input;
						return true;
					}
					else
						Console.WriteLine("Enter a string");
				}
			}
			else if (typeof(T) == typeof(bool)) {
				while (true) {
					Console.Write(request + " (y/n): ");
					string? input = Console.ReadLine();

					if (string.IsNullOrEmpty(input)) {
						Console.WriteLine("Enter y/n");
						continue;
					}

					if (TryParseYesNo(input, out bool bInput)) {
						result = (T)(object)bInput;
						return true;
					}
					else
						Console.WriteLine("Enter y/n");
				}
			}
			else {
				throw new System.NotImplementedException();
			}
		}

		public static bool TryParseYesNo(string input, out bool output) {
			string lInput = input.ToLower();
			if (lInput.StartsWith("y")) {
				output = true;
				return true;
			}
			else if (lInput.StartsWith("n")) {
				output = false;
				return true;
			}
			else {
				output = false;
				return false;
			}
		}

		public bool FileSelectDialog(string request, out string result) {
			while (true) {
				SelectDialog<string>(request, out string input);

				if (File.Exists(input) || Directory.Exists(input)) {
					result = input;
					return true;
				}
				else
					Console.WriteLine("File or directory does not exist");
			}
		}
	}
}