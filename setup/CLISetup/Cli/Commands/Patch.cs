using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Setup.Tasks;

namespace CliSetup.Commands {
	[Command("patch", Description = "patch")]
	public class Patch : BaseCommand {
		[CommandOption("terraria", Description = "")]
		public bool terraria { get; set; }

		[CommandOption("terrariaNetCore", Description = "")]
		public bool terrariaNetCore { get; set; }

		[CommandOption("tModLoader", Description = "")]
		public bool tModLoader { get; set; }

		public override ValueTask ExecuteAsync(IConsole console) {
			StartupMessage();

			if (terraria)
				SetupOperation.RunTask(NewTerrariaPatchTask());

			if (terrariaNetCore)
				SetupOperation.RunTask(NewTerrariaNetCorePatchTask());

			if (tModLoader)
				SetupOperation.RunTask(NewtMLPatchTask());

			return default;
		}
	}
}