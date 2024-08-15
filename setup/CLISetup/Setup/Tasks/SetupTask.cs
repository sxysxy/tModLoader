namespace Setup.Tasks {
	public class SetupTask : CompositeTask {
		public SetupTask(ITaskInterface taskInterface, params SetupOperation[] tasks) : base(taskInterface, tasks) { }

		public override bool StartupWarning() {
			taskInterface.SelectDialog<bool>("Any changes in /src will be lost. Contiue?", out bool proceed);
			return proceed;
		}
	}
}