using System;
using System.Threading.Tasks;
using CliFx;

namespace CliSetup {
	public static class Program {
		public static async Task<int> Main(string[] args) {
			Console.WriteLine("Press any key to continue:");

			// Console.ReadKey may trigger the debugger interrupting here, may bring trouble, so here catch the exception to ignore it.
			// This has no impact on normal use.
			try {
				Console.ReadKey();
			}catch (InvalidOperationException) {
			
			}

			if (OperatingSystem.IsWindows())
				WindowsAnsiBootstrapper.Bootstrap();

			return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
		}
	}
}