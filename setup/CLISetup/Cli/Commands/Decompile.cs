using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Setup.Tasks;

namespace CliSetup.Commands {
	[Command("decompile", Description = "Uses ILSpy to decompile Terraria\r\nAlso decompiles server classes not included in the client binary. Outputs to src/decompiled")]
	public class Decompile : BaseCommand {
		[CommandOption("server-only", Description = "Decompile only the server")]
		public bool serverOnly { get; set; } = false;

		public override ValueTask ExecuteAsync(IConsole console) {
			StartupMessage();

			SetupOperation.RunTask(NewDecompileTask(serverOnly));

			return default;
		}
	}
}