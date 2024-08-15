using System;
using System.Threading.Tasks;
using CliFx;

namespace CliSetup {
	public static class Program {
		public static async Task<int> Main(string[] args) {
			Console.WriteLine("Press any key to continue:");
			Console.ReadKey();

			if (OperatingSystem.IsWindows())
				WindowsAnsiBootstrapper.Bootstrap();

			return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
		}
	}
}