using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Setup.Tasks;

namespace CliSetup.Commands {
	[Command("regen", Description = "Regenerates source")]
	public class Regen : BaseCommand {
		public override ValueTask ExecuteAsync(IConsole console) {
			StartupMessage();

			SetupOperation.RunTask(NewRegenTask());

			return default;
		}
	}
}