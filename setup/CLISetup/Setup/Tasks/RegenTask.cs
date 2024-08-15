using static Setup.FileHandler;
using static Setup.FileHandler.Config;

namespace Setup.Tasks {
	public class RegenTask : CompositeTask {
		public RegenTask(ITaskInterface taskInterface, params SetupOperation[] tasks) : base(taskInterface, tasks) { }

		public override bool StartupWarning() {
			Config config = GetConfig();
			if (config.PatchMode == 2) {
				if (!taskInterface.SelectDialog<bool>("Patch mode will be reset from fuzzy to offset. Continue?", out bool proceed) || !proceed)
					return false;
			}

			if (taskInterface.SelectDialog<bool>("Any changes in /src will be lost. Continue?", out bool proceed1))
				return proceed1;

			return false;
		}

		public override void Run() {
			Config config = GetConfig();
			if (config.PatchMode == 2) {
				config.PatchMode = 1;
				SetConfig(config);
			}

			base.Run();
		}
	}
}