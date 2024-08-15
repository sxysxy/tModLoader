namespace Setup {
	public interface ITaskInterface {
		void SetMaxProgress(int max);
		void SetStatus(string status);
		void Message(string message);
		void Warning(string warning);
		void Error(string error);
		void SetProgress(int progress);
		/// <summary>
		/// Gets input from the user. Should handle string (file paths), bool, and int input
		/// </summary>
		bool SelectDialog<T>(string request, out T result);
		bool FileSelectDialog(string request, out string result);
	}
}