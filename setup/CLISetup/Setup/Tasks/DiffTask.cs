using CodeChicken.DiffPatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Setup.Tasks
{
	public class DiffTask : SetupOperation
	{
		private static string[] extensions = { ".cs", ".csproj", ".ico", ".resx", ".png", "App.config", ".json", ".targets", ".txt", ".bat", ".sh" };
		private static bool IsDiffable(string relPath) => extensions.Any(relPath.EndsWith);

		public static readonly string RemovedFileList = "removed_files.list";
		public static readonly Regex HunkOffsetRegex = new Regex(@"@@ -(\d+),(\d+) \+([_\d]+),(\d+) @@", RegexOptions.Compiled);

		public readonly string baseDir;
		public readonly string patchedDir;
		public readonly string patchDir;
		public readonly ProgramSetting<DateTime> cutoff;

		public DiffTask(ITaskInterface taskInterface, string baseDir, string srcDir, string patchDir,
			ProgramSetting<DateTime> cutoff) : base(taskInterface)
		{
			this.baseDir = baseDir;
			this.patchedDir = srcDir;
			this.patchDir = patchDir;
			this.cutoff = cutoff;
		}

		public override void Run()
		{
			var items = new List<WorkItem>();

			foreach (var (file, relPath) in PatchTask.EnumerateSrcFiles(patchedDir))
			{
				if (File.GetLastWriteTime(file) < cutoff.Get())
					continue;

				if (!File.Exists(Path.Combine(baseDir, relPath)))
					items.Add(new WorkItem("Copying: " + relPath, () => FileHandler.Copy(file, Path.Combine(patchDir, relPath))));
				else if (IsDiffable(relPath))
					items.Add(new WorkItem("Diffing: " + relPath, () => Diff(relPath)));
			}

			ExecuteParallel(items);

			taskInterface.SetStatus("Deleting Unnecessary Patches");
			foreach (var (file, relPath) in FileHandler.EnumerateFiles(patchDir)) {
				var targetPath = relPath.EndsWith(".patch") ? relPath.Substring(0, relPath.Length - 6) : relPath;
				if (!File.Exists(Path.Combine(patchedDir, targetPath)))
					FileHandler.DeleteFile(file);
			}

			FileHandler.DeleteEmptyDirs(patchDir);

			taskInterface.SetStatus("Noting Removed Files");
			var removedFiles = PatchTask.EnumerateSrcFiles(baseDir)
				.Where(f => !File.Exists(Path.Combine(patchedDir, f.relPath)))
				.Select(f => f.relPath)
				.ToArray();

			var removedFileList = Path.Combine(patchDir, RemovedFileList);
			if (removedFiles.Length > 0)
				File.WriteAllLines(removedFileList, removedFiles);
			else
				FileHandler.DeleteFile(removedFileList);

			cutoff.Set(DateTime.Now);
		}

		private void Diff(string relPath)
		{
			var patchFile = Differ.DiffFiles(new LineMatchedDiffer(),
				Path.Combine(baseDir, relPath).Replace('\\', '/'),
				Path.Combine(patchedDir, relPath).Replace('\\', '/'));

			var patchPath = Path.Combine(patchDir, relPath + ".patch");
			if (!patchFile.IsEmpty) {
				FileHandler.CreateParentDirectory(patchPath);
				File.WriteAllText(patchPath, patchFile.ToString(true));
			}
			else
				FileHandler.DeleteFile(patchPath);
		}
	}
}