using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Timers;

namespace PTR
{
	class PTRManager : IPTRManager
	{
		private ConcurrentQueue<Action> ActionQueue { get; set; }
		IObjectFactory Agent;

		public PTRManager(IObjectFactory Factory)
		{
			this.Agent = Factory;
		}

		List<ITestConfigManager> _testConfigManagers = null;
		Timer _timerForReportMerging = null;
		public void Run()
		{
			Console.Write("Copyright (c) 2017 Aseem");
			Console.WriteLine();
			try
			{
				///////////////////////////
				Threads.ILicenseChecker licenseChecker = new Threads.LicenseChecker();
				if (!licenseChecker.DoesThisMachineHaveAValidLicense())
				{
					Console.Write("Your license is expired. Please get it renewed to use further.");
					Console.WriteLine();
					System.Threading.Thread.Sleep(5000);
					return;
				}
				///////////////////////////

				this.Agent.LogUtil.LogMessage("Running ProcessWideConfig.BeforeTestExecution if any specified...");
				RunExecutable(this.Agent.TestingEnvironment.ProcessWideConfig.BeforeTestExecution);

				this.Agent.LogUtil.LogMessage("Creating config runners for each test configurations...");
				_testConfigManagers = this.Agent.TestingEnvironment.TestingConfigurations.Select(x => this.Agent.GetConfigManagerByTestConfig(x)).ToList();

				this.Agent.LogUtil.LogMessage("Distributing actions in the queue...");
				this.ActionQueue = new ConcurrentQueue<Action>();
				this.DistributeActionsInTheQueue(_testConfigManagers);

				_timerForReportMerging = new Timer();
				_timerForReportMerging.Elapsed += _timerForReportMerging_Elapsed;
				_timerForReportMerging.Interval = 60000;
				_timerForReportMerging.Enabled = true;

				this.Agent.LogUtil.LogMessage("Starting execution of test cases...");
				this.RunTestCases();
				_timerForReportMerging.Stop();

				this.Agent.LogUtil.LogMessage("Doing final processing of test-reports of all configurations...");
				this.ProcessConfigResults();

				this.Agent.LogUtil.LogMessage("Closing ProcessWideConfig.BeforeTestExecution if any specified...");
				CloseExecutable(this.Agent.TestingEnvironment.ProcessWideConfig.BeforeTestExecution);

				this.Agent.LogUtil.LogMessage("Doing clean-up...");
				CleanUp();

				this.Agent.LogUtil.LogMessage("Running ProcessWideConfig.AfterTestExecution if any specified...");
				RunExecutable(this.Agent.TestingEnvironment.ProcessWideConfig.AfterTestExecution);
			}
			catch(Exception exp)
			{
				this.Agent.LogUtil.LogError(exp.Message);
			}

			System.Threading.Thread.Sleep(5000);
		}

		private Process RunExecutable(string strExecutableFile)
		{
			if (string.IsNullOrEmpty(strExecutableFile))
			{
				return null;
			}

			string strExecutableFileName = Path.GetFileNameWithoutExtension(strExecutableFile);
			if (Process.GetProcessesByName(strExecutableFileName).Length > 0)
			{
				return null;
			}

			return Process.Start(strExecutableFile);
		}

		private enum ProcessOperation
		{
			CloseMainWindow,
			Kill
		}

		private void CloseExecutable(string strExecutableFile)
		{
			if (string.IsNullOrEmpty(strExecutableFile))
			{
				return;
			}

			string strExecutableFileName = Path.GetFileNameWithoutExtension(strExecutableFile);
			DoProcessOperation(strExecutableFileName, ProcessOperation.CloseMainWindow);
			DoProcessOperation(strExecutableFileName, ProcessOperation.Kill);
		}

		private void DoProcessOperation(string strExecutableFileName, ProcessOperation processOperation)
		{
			foreach (var process in Process.GetProcessesByName(strExecutableFileName))
			{
				try
				{
					switch (processOperation)
					{
						case ProcessOperation.CloseMainWindow:
							process.CloseMainWindow();
							return;

						case ProcessOperation.Kill:
							process.Kill();
							return;
					}
				}
				catch (Exception)
				{ }
			}
		}

		private Dictionary<Action, ITestConfigManager> _dicITestConfigManagerByActions = new Dictionary<Action, ITestConfigManager>();
		private void DistributeActionsInTheQueue(List<ITestConfigManager> testConfigManagers)
		{
			int iActionsCount = 0;
			testConfigManagers.ForEach(configManager => iActionsCount += configManager.TestCaseExecutors.Count);
			while (true)
			{
				foreach (var testConfigManager in testConfigManagers)
				{
					int iThreadCount = testConfigManager.TestConfig.ThreadCount;
					foreach (var testCaseExecutor in testConfigManager.TestCaseExecutors)
					{
						if (!this.ActionQueue.Contains(testCaseExecutor))
						{
							EnqueueTestCaseExecutor(testConfigManager, testCaseExecutor);
							iThreadCount--;
						}

						if (iThreadCount == 0)
						{
							break;
						}
					}
				}

				if (iActionsCount == this.ActionQueue.Count)
				{
					break;
				}
			}
		}

		public void EnqueueTestCaseExecutor(ITestConfigManager testConfigManager, Action testCaseExecutor)
		{
			_dicITestConfigManagerByActions.Add(testCaseExecutor, testConfigManager);
			this.ActionQueue.Enqueue(testCaseExecutor);
		}

		private Dictionary<Task, Action> _dicActionsByTasks = new Dictionary<Task, Action>();
		private void RunTestCases()
		{
			var tasks = new List<Task>();
			while (true)
			{
				System.Threading.Thread.Sleep(500);
				Action action;
				if (this.ActionQueue.TryDequeue(out action))
				{
					var task = Task.Run(action);
					_dicActionsByTasks.Add(task, action);
					tasks.Add(task);
					System.Threading.Thread.Sleep(500);
				}
				else if (tasks.Count() == 0)
				{
					break;
				}

				var tasksNotCompletedCount = tasks.Where(t => !t.IsCompleted).Count();
				if (this.ActionQueue.Count > 0 && tasksNotCompletedCount < this.Agent.TestingEnvironment.ProcessWideConfig.MaxThreads)
				{
					continue;
				}

				var tasksCompleted = tasks.Where(t => t.IsCompleted).ToList();
				foreach (var task in tasksCompleted)
				{
					tasks.Remove(task);
					this.NotifyTaskCompletion(task);
				}

				tasksNotCompletedCount = tasks.Where(t => !t.IsCompleted).Count();
				if (this.ActionQueue.Count == 0 || tasksNotCompletedCount >= this.Agent.TestingEnvironment.ProcessWideConfig.MaxThreads)
				{
					if (tasks.Count > 0)
					{
						Task.WhenAny(tasks).Wait();
					}
				}
			}
		}

		private void NotifyTaskCompletion(Task task)
		{
			var action = _dicActionsByTasks[task];
			_dicITestConfigManagerByActions[action].ActionCompleted(action);
		}

		void _timerForReportMerging_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (this)
			{
				ProcessConfigResults();
			}
		}

		private void ProcessConfigResults()
		{
			foreach (var testConfigManager in _testConfigManagers)
			{
				testConfigManager.MergeOtherConfigManagersResultsWhereeverRequired(_testConfigManagers);
                if (!string.IsNullOrEmpty(testConfigManager.TestConfig.ReportingLocation) && File.Exists(testConfigManager.ConsolidatedResultsFile))
                {
                    string strReportingFile = Path.GetFileName(testConfigManager.TestConfig.ReportingLocation);
                    if (!string.IsNullOrEmpty(strReportingFile))
                    {
                        string strReportingDirectory = Path.GetDirectoryName(testConfigManager.TestConfig.ReportingLocation);
                        if (string.IsNullOrEmpty(strReportingDirectory))
                        {
                            strReportingDirectory = Path.Combine(testConfigManager.TestConfig.LocationFromWhereTestingProjectBinariesAreToBeLoaded,
                                                                    testConfigManager.TestConfig.SemicolonSeparatedFilesHavingTestCases.FirstFile());

                            strReportingDirectory = Path.GetDirectoryName(strReportingDirectory);
                        }
                        strReportingFile = Path.Combine(strReportingDirectory, strReportingFile);
                        File.Copy(testConfigManager.ConsolidatedResultsFile, strReportingFile, true);
                    }
                }
			}
		}

		private void CleanUp()
		{
			foreach (var testConfigManager in _testConfigManagers)
			{
				testConfigManager.CleanUp();
			}

			//this.Agent.InputOutputUtil.DeleteDirectoryAndAllItsContent(Path.GetTempPath());
		}
	}
}
