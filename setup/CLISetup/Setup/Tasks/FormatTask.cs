using System.Collections;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Setup.Tasks.Formatting;

namespace Setup.Tasks {
	public class FormatTask : SetupOperation {
		public FormatTask(ITaskInterface taskInterface) : base(taskInterface) { }

		private static AdhocWorkspace workspace = new();
		static FormatTask() {
			workspace.Options = workspace.Options
				.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, LanguageNames.CSharp), true)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false);
		}

		private static string projectPath = null!;
		public override bool ConfigurationDialog() {
			bool dialog = taskInterface.SelectDialog<string>("Select C# Project", out string path);
			projectPath = path;
			return dialog && File.Exists(projectPath);
		}

		public override void Run() {
			string dir = Path.GetDirectoryName(projectPath)!;
			var workItems = Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)
				.Select(path => new FileInfo(path))
				.OrderByDescending(f => f.Length)
				.Select(f => new WorkItem("Formatting: " + f.Name, () => FormatFile(f.FullName, false)));

			ExecuteParallel(workItems.ToList());
		}

		public static void FormatFile(string path, bool aggressive) {
			string source = File.ReadAllText(path);
			string formatted = Format(source, aggressive);
			if (source != formatted)
				File.WriteAllText(path, formatted);
		}

		public static SyntaxNode Format(SyntaxNode node, bool aggressive) {
			if (aggressive) {
				node = new NoNewlineBetweenFieldsRewriter().Visit(node);
				node = new RemoveBracesFromSingleStatementRewriter().Visit(node);
			}

			node = new AddVisualNewlinesRewriter().Visit(node);
			node = Formatter.Format(node, workspace);
			node = new CollectionInitializerFormatter().Visit(node);
			return node;
		}

		public static string Format(string source, bool aggressive) {
			var tree = CSharpSyntaxTree.ParseText(source);
			return Format(tree.GetRoot(), aggressive).ToFullString();
		}
	}
}