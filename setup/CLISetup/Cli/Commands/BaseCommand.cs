using System;
using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Setup;
using Setup.Tasks;

namespace CliSetup.Commands {
	public abstract class BaseCommand : ICommand {
		protected TermInterface taskInterface = new();

		[CommandOption("steamdir", Description = "tModLoader Steam Directory")]
		public string? steamDir { get; set; }

		[CommandOption("patchmode", 'p', Description = "The patch mode to be used")]
		public int patchmode { get; set; }

		[CommandOption("no-logo", Description = "Prevents the logo from showing, useful to prevent logs from cluttering")]
		public bool noLogo { get; set; }

		public abstract ValueTask ExecuteAsync(IConsole console);

		public void StartupMessage() {
			if (!noLogo) {
				using (Stream resource = typeof(Program).Assembly.GetManifestResourceStream("CliSetup.Resources.LogoT")!) {
					using (StreamReader reader = new StreamReader(resource)) {
						Console.WriteLine(reader.ReadToEnd());
					}
				}
			}
		}

		public PatchTask NewTerrariaPatchTask() {
			return new PatchTask(taskInterface, "src/decompiled", "src/Terraria", "patches/Terraria", new ProgramSetting<DateTime>("TerrariaDiffCutoff"));
		}

		public PatchTask NewTerrariaNetCorePatchTask() {
			return new PatchTask(taskInterface, "src/Terraria", "src/TerrariaNetCore", "patches/TerrariaNetCore", new ProgramSetting<DateTime>("TerrariaNetCoreDiffCutoff"));
		}

		public PatchTask NewtMLPatchTask() {
			return new PatchTask(taskInterface, "src/TerrariaNetCore", "src/tModLoader", "patches/tModLoader", new ProgramSetting<DateTime>("tModLoaderDiffCutoff"));
		}

		public DecompileTask NewDecompileTask(bool serverOnly) {
			return new DecompileTask(taskInterface, serverOnly ? "src/decompiled_server" : "src/decompiled", serverOnly);
		}

		public RegenTask NewRegenTask() {
			return new RegenTask(taskInterface, NewTerrariaPatchTask(), NewTerrariaNetCorePatchTask(), NewtMLPatchTask());
		}
	}
}