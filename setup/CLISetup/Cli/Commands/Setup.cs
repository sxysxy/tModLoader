using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Setup.Tasks;

namespace CliSetup.Commands {
	[Command("setup", Description = "Setup")]
	public class Setup : BaseCommand {
		public override ValueTask ExecuteAsync(IConsole console) {
			StartupMessage();

			SetupOperation.RunTask(new SetupTask(taskInterface, NewDecompileTask(false), NewRegenTask()));

			return default;
		}
	}
}