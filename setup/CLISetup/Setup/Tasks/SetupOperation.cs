using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Setup.Tasks {
	public abstract class SetupOperation {
		protected readonly ITaskInterface taskInterface;
		protected int progress;

		public SetupOperation(ITaskInterface taskInterface) {
			this.taskInterface = taskInterface;
		}

		public abstract void Run();

		public virtual bool ConfigurationDialog() => true;

		public virtual bool StartupWarning() => true;

		public virtual bool Failed() => false;

		public virtual bool Warnings() => false;

		public virtual void FinishedDialog() {}

		protected delegate void UpdateStatus(string status);
		protected delegate void Worker(UpdateStatus updateStatus);

		protected class WorkItem {
			public readonly string status;
			public readonly Worker worker;

			public WorkItem(string status, Worker worker) {
				this.status = status;
				this.worker = worker;
			}

			public WorkItem(string status, Action action) : this(status, _ => action()) { }
		}

		protected void ExecuteParallel(List<WorkItem> items, bool resetProgress = true, int maxDegree = 0) {
			try {
				if (resetProgress) {
					taskInterface.SetMaxProgress(items.Count);
					progress = 0;
				}

				var working = new List<Ref<string>>();
				void UpdateStatus() => taskInterface.SetStatus(string.Join("\r\n", working.Select(r => r.item)));

				Parallel.ForEach(Partitioner.Create(items, EnumerablePartitionerOptions.NoBuffering),
					new ParallelOptions { MaxDegreeOfParallelism = maxDegree > 0 ? maxDegree : Environment.ProcessorCount },
					item => {
						//taskInterface.CancellationToken.ThrowIfCancellationRequested();
						var status = new Ref<string>(item.status);
						lock (working) {
							working.Add(status);
							UpdateStatus();
						}

						void SetStatus(string s) {
							lock(working) {
								status.item = s;
								UpdateStatus();
							}
						}

						item.worker(SetStatus);

						lock (working) {
							working.Remove(status);
							taskInterface.SetProgress(++progress);
							UpdateStatus();
						}
					});
			} catch (AggregateException ex) {
				var actual = ex.Flatten().InnerExceptions.Where(e => !(e is OperationCanceledException));
				if (!actual.Any())
					throw new OperationCanceledException();

				throw new AggregateException(actual);
			}
		}

		public static void RunTask(SetupOperation task) {
			new Thread(() => RunTaskThread(task)).Start();
		}

		public static void RunTaskThread(SetupOperation task) {
			string errorLogFile = Path.Combine(FileHandler.logsDir, "Error.log");

			FileHandler.DeleteFile(errorLogFile);

			if (!task.ConfigurationDialog())
				return;

			if (!task.StartupWarning())
				return;

			task.Run();

			if (task.Failed() || task.Warnings())
				task.FinishedDialog();
		}
	}
}