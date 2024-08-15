using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Setup {
	public class FileHandler {
		static FileHandler() {
			string configDir = Path.GetDirectoryName(GetConfigPath())!;
			if (!Directory.Exists(configDir)) {
				Directory.CreateDirectory(configDir);
			}
		}

		public static readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
		public static readonly string logsDir = Path.Combine("setup", "logs");
		public static string[] steamMD5s = new string[3] {
			"6352ded8d64d0f67fdf10ff2a6f9e51f", // Windows
			"0b253fbe529ea3e2ac61a0658f43af94", // Mac
			"ab57cfd9076ab0c0eab9f46a412b8422"  //  Linux
		};
		public static string[] gogMD5s = new string[3] {
			"201707bba92e27f09d05529e2f051c60", // Windows
			"7b8d96e0ef583164d565dc01be4b5627", // Mac
			"fa53f0a39be5698da7a15a1cc9e56689"  // Linux
		};

		public class Config {
			private DateTime _terrariaDiffCutoff;
			private DateTime _tModLoaderDiffCutoff;
			private string _clientVersion = null!;
			private string _serverVersion = null!;
			private int? _patchMode;
			private bool? _formatAfterDecompiling;
			private string _terrariaPath = null!;
			private string _serverPath = null!;

			public string TerrariaSteamDir { get; set; } = null!;

			public DateTime TerrariaDiffCutoff {
				get {
					if (_terrariaDiffCutoff == default)
						_terrariaDiffCutoff = Convert.ToDateTime("2015-01-01");
					return _terrariaDiffCutoff;
				}
				set => _terrariaDiffCutoff = value;
			}

			public DateTime tModLoaderDiffCutoff {
				get {
					if (_tModLoaderDiffCutoff == default)
						_tModLoaderDiffCutoff = Convert.ToDateTime("2015-01-01");
					return _tModLoaderDiffCutoff;
				}
				set => _tModLoaderDiffCutoff = value;
			}

			public string ClientVersion {
				get {
					if (string.IsNullOrEmpty(_clientVersion))
						//_clientVersion = "1.4.3.6";
						_clientVersion = "1.4.4.9";
					return _clientVersion;
				}
				set => _clientVersion = value;
			}

			public string ServerVersion {
				get {
					if (string.IsNullOrEmpty(_serverVersion))
						//_serverVersion = "1.4.3.6";
						_serverVersion = "1.4.4.9";
					return _serverVersion;
				}
				set => _serverVersion = value;
			}

			public int PatchMode {
				get {
					if (_patchMode == null)
						_patchMode = 0;
					return (int)_patchMode;
				}
				set => _patchMode = value;
			}

			public bool FormatAfterDecompiling {
				get {
					if (_formatAfterDecompiling == null)
						_formatAfterDecompiling = true;
					return (bool)_formatAfterDecompiling;
				}
				set => _formatAfterDecompiling = value;
			}

			public string tmlDevSteamDir { get; set; } = null!;

			public string terrariaPath {
				get {
					if (_terrariaPath == null)
						_terrariaPath = "";
					return _terrariaPath;
				}
				set => _terrariaPath = value;
			}

			public string serverPath {
				get {
					if (_serverPath == null)
						_serverPath = "";
					return _serverPath;
				}
				set => _serverPath = value;
			}

			private static Config? instance = null;

			public static Config GetConfig() {
				if (instance is null) {
					if (File.Exists(GetConfigPath())) {
						IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();

						return deserializer.Deserialize<Config>(File.ReadAllText(GetConfigPath()));
					}
					else {
						Config config = new Config();
						SetConfig(config);
						return instance!;
					}
				}
				else
					return instance;
			}

			public static void SetConfig(Config config) {
				ISerializer serializer = new SerializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();

				File.WriteAllText(GetConfigPath(), serializer.Serialize(config));

				instance = config;
			}
		}

		private static string configPath = null!;

		public static string GetConfigPath() {
			if (!string.IsNullOrEmpty(configPath))
				return configPath;

			if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
				configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tMLSetup", "Config.yaml");
			else if (OperatingSystem.IsLinux()) {
				string? xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

				if (!string.IsNullOrEmpty(xdgConfig))
					configPath = Path.Combine(xdgConfig, "tMLSetup", "Config.yaml");
				else
					configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "tMLSetup", "Config.yaml");
			}
			else
				throw new PlatformNotSupportedException();

			return configPath;
		}

		public static string PreparePath(string path)
			=> path.Replace('/', Path.DirectorySeparatorChar);

		public static string RelPath(string basePath, string path) {
			if (path.Last() == Path.DirectorySeparatorChar)
				path = path.Substring(0, path.Length - 1);

			if (basePath.Last() != Path.DirectorySeparatorChar)
				basePath += Path.DirectorySeparatorChar;

			if (path + Path.DirectorySeparatorChar == basePath) return "";

			if (!path.StartsWith(basePath)) {
				path = Path.GetFullPath(path);
				basePath = Path.GetFullPath(basePath);
			}

			if(!path.StartsWith(basePath))
				throw new ArgumentException("Path \""+path+"\" is not relative to \""+basePath+"\"");

			return path.Substring(basePath.Length);
		}

		public static void CreateDirectory(string dir) {
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		public static void CreateParentDirectory(string path) {
			CreateDirectory(Path.GetDirectoryName(path)!);
		}

		public static void Copy(string from, string to) {
			CreateParentDirectory(to);

			if (File.Exists(to)) {
				File.SetAttributes(to, FileAttributes.Normal);
			}

			File.Copy(from, to, true);
		}

		public static void DeleteFile(string path) {
			if (File.Exists(path)) {
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}
		}

		public static bool DeleteEmptyDirs(string dir) {
			if (!Directory.Exists(dir))
				return true;

			return DeleteEmptyDirsRecursion(dir);
		}

		private static bool DeleteEmptyDirsRecursion(string dir) {
			bool allEmpty = true;

			foreach (string subDir in Directory.EnumerateDirectories(dir))
				allEmpty &= DeleteEmptyDirsRecursion(subDir);

			if (!allEmpty || Directory.EnumerateFiles(dir).Any())
				return false;

			Directory.Delete(dir);

			return true;
		}

		public static IEnumerable<(string file, string relPath)> EnumerateFiles(string dir) =>
			Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
			.Select(path => (file: path, relPath: RelPath(dir, path)));

		private static string terrariaFileName = null!;

		public static string GetTerrariaFileName() {
			if (!string.IsNullOrEmpty(terrariaFileName))
				return terrariaFileName;

			if (OperatingSystem.IsWindows())
				terrariaFileName = "Terraria.exe";
			else if (OperatingSystem.IsMacOS())
				//terrariaFileName = "Terraria.App";
				terrariaFileName = "Terraria.bin.osx";
			else if (OperatingSystem.IsLinux())
				terrariaFileName = "Terraria.bin.x86_64";
			else
				throw new PlatformNotSupportedException();

			return terrariaFileName;
		}

		private static string terrariaServerFileName = null!;

		public static string GetTerrariaServerFileName() {
			if (!string.IsNullOrEmpty(terrariaServerFileName))
				return terrariaServerFileName;

			if (OperatingSystem.IsWindows())
				terrariaServerFileName = "TerrariaServer.exe";
			else if (OperatingSystem.IsMacOS())
				//terrariaServerFileName = "TerrariaServer.App";
				terrariaServerFileName = "TerrariaServer.bin.osx";
			else if (OperatingSystem.IsLinux())
				terrariaServerFileName = "TerrariaServer.bin.x86_64";
			else
				throw new PlatformNotSupportedException();

			return terrariaServerFileName;
		}

		public static bool SelectAndSetTerrariaDirectoryDialog(ITaskInterface taskInterface) {
			while (true) {
				string? err = null;
				string? serverPath = null;
				if (taskInterface.SelectDialog<string>("Enter the location of the Terraria executable/binary", out string path)) {
					if (Path.GetFileName(path) != "Terraria.exe")
					//if (Path.GetFileName(path) != GetTerrariaFileName())
						err = $"File must be named Terraria.exe. If you are using Linux or MacOS, you can copy a Terraria.exe from windows.\n";
					//else if (!File.Exists(Path.Combine(Path.GetDirectoryName(path)!, GetTerrariaServerFileName())))
					serverPath = Path.Combine(Path.GetDirectoryName(path)!, "TerrariaServer.exe");
					if(!File.Exists(serverPath)) {
						err = $"TerrariaServer.exe must exist in the same directory\n";
					} 

				}

				if (err != null)
					taskInterface.Error(err);
				else {
					Config nc = Config.GetConfig();
					nc.terrariaPath = path;
					nc.serverPath = serverPath!;
					Config.SetConfig(nc);

					return true;
				}
			}
		}

		internal static void UpdateTargetsFiles() {
			UpdateFileText("src/WorkspaceInfo.targets", GetWorkspaceInfoTargetsText());
			UpdateFileText(Path.Combine(Config.GetConfig().tmlDevSteamDir, "tMLMod.targets"), File.ReadAllText("patches/tModLoader/Terraria/release_extras/tMLMod.targets"));
		}

		private static void UpdateFileText(string path, string text) {
			CreateParentDirectory(path);

			if (!File.Exists(path) || text != File.ReadAllText(path))
				File.WriteAllText(path, text);
		}

		private static string GetWorkspaceInfoTargetsText() {
			Config config = Config.GetConfig();
			string gitsha = "";
			RunCmd("", "git", "rev-parse HEAD", s => gitsha = s.Trim());

			string branch = "";
			RunCmd("", "git", "rev-parse --abbrev-ref HEAD", s => branch = s.Trim());

			string GITHUB_HEAD_REF = Environment.GetEnvironmentVariable("GITHUB_HEAD_REF")!;
			if (!string.IsNullOrWhiteSpace(GITHUB_HEAD_REF)) {
				Console.WriteLine($"GITHUB_HEAD_REF found: {GITHUB_HEAD_REF}");
				branch = GITHUB_HEAD_REF;
			}
			string HEAD_SHA = Environment.GetEnvironmentVariable("HEAD_SHA")!;
			if (!string.IsNullOrWhiteSpace(HEAD_SHA)) {
				Console.WriteLine($"HEAD_SHA found: {HEAD_SHA}");
				gitsha = HEAD_SHA;
			}

			return
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <!-- This file will always be overwritten, do not edit it manually. -->
  <PropertyGroup>
	<BranchName>{branch}</BranchName>
	<CommitSHA>{gitsha}</CommitSHA>
	<TerrariaSteamPath>{config.TerrariaSteamDir}</TerrariaSteamPath>
    <tModLoaderSteamPath>{config.tmlDevSteamDir}</tModLoaderSteamPath>
  </PropertyGroup>
</Project>";
		}

		public static int RunCmd(string dir, string cmd, string args,
				Action<string> output = null,
				Action<string> error = null,
				string input = null,
				CancellationToken cancel = default(CancellationToken)) {

			using (var process = new Process()) {
				process.StartInfo = new ProcessStartInfo {
					FileName = cmd,
					Arguments = args,
					WorkingDirectory = dir,
					UseShellExecute = false,
					RedirectStandardInput = input != null,
					CreateNoWindow = true
				};

				if (output != null) {
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
				}

				if (error != null) {
					process.StartInfo.RedirectStandardError = true;
					process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
				}

				if (!process.Start())
					throw new Exception($"Failed to start process: \"{cmd} {args}\"");

				if (input != null) {
					var w = new StreamWriter(process.StandardInput.BaseStream, new UTF8Encoding(false));
					w.Write(input);
					w.Close();
				}

				while (!process.HasExited) {
					if (cancel.IsCancellationRequested) {
						process.Kill();
						throw new OperationCanceledException(cancel);
					}
					process.WaitForExit(100);

					output?.Invoke(process.StandardOutput.ReadToEnd());
					error?.Invoke(process.StandardError.ReadToEnd());
				}

				return process.ExitCode;
			}
		}
	}
}