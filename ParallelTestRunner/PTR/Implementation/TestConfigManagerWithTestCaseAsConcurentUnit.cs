using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class TestConfigManagerWithTestCaseAsConcurentUnit : AbstractTestConfigManager
	{
		protected ConcurrentQueue<string> TestCaseQueue { get; set; }
		public TestConfigManagerWithTestCaseAsConcurentUnit(TestConfig TestConfig, IObjectFactory Factory)
			: base(TestConfig, Factory)
		{
			this.TestCaseQueue = new ConcurrentQueue<string>();
		}

		int _totalNumberOfTestCases = 0;
		int _totalNumberOfExecutedTestCases = 0;

		int _iTimesToRerunFailedTestCases = -1;
		private Dictionary<Action, string> _dicExecutionLocationsByAction;
		protected bool _bTestCaseExecutorsCreated = false;
		public override List<Action> TestCaseExecutors
		{
			get
			{
				if (!_bTestCaseExecutorsCreated)
				{
					this.Agent.LogUtil.LogMessage(string.Format("Getting test case executors for {0} ...", TestConfig.ID));

					_iTimesToRerunFailedTestCases = this.TestConfig.TimesToRerunFailedTestCases;
					_dicExecutionLocationsByAction = new Dictionary<Action, string>();

					var testCasesToBeConsidered = this.GetTestCasesToBeConsideredFromGivenList(_testCaseRepository.AllTestCases);
					_totalNumberOfTestCases = testCasesToBeConsidered.Count();
					_totalNumberOfExecutedTestCases = 0;
					if (_totalNumberOfTestCases > 0)
					{
						testCasesToBeConsidered.ForEach(x => this.TestCaseQueue.Enqueue(x.FullName));

						for (int i = 0; i < this.TestConfig.ThreadCount; i++)
						{
							var guidForNewAction = Guid.NewGuid().ToString();
							string strActionExecutionLocation = GetNextActionExecutionLocation();
							Action action = this.GetAction(guidForNewAction, strActionExecutionLocation);

							_dicExecutionLocationsByAction.Add(action, strActionExecutionLocation);
							_dicActionResults.Add(action, false);
							_dicActionIDs.Add(action, guidForNewAction);
						}
					}

					_bTestCaseExecutorsCreated = true;
				}
				return _dicActionResults.Keys.ToList();
			}
		}

		protected Dictionary<string, int> _dicThreadCountByLocation = new Dictionary<string, int>();
		protected string GetNextActionExecutionLocation(bool bActionForRunningFailedTestCases = false, int iNumberOfTimesRunningFailedTestCases = -1)
		{
			string strNextActionExecutionLocation = this.TestConfig.ExecutionLocation;
			lock (this.TestConfig)
			{
				string strTempThreadExecutionFolderPrefix = bActionForRunningFailedTestCases
																		? CONSTANTS.ThreadExecutionFolderPrefixForFailedTestCases +
																			iNumberOfTimesRunningFailedTestCases.ToString() + "_"
																		: CONSTANTS.ThreadExecutionFolderPrefix;

				strNextActionExecutionLocation = Path.Combine(strNextActionExecutionLocation, strTempThreadExecutionFolderPrefix);

				if (!_dicThreadCountByLocation.ContainsKey(strNextActionExecutionLocation))
				{
					_dicThreadCountByLocation.Add(strNextActionExecutionLocation, 0);
				}

				_dicThreadCountByLocation[strNextActionExecutionLocation] = _dicThreadCountByLocation[strNextActionExecutionLocation] + 1;

				strNextActionExecutionLocation = strNextActionExecutionLocation + _dicThreadCountByLocation[strNextActionExecutionLocation];
			}
			return strNextActionExecutionLocation;
		}

		private Action GetAction(string strGuidForNewAction,
									string strExecutionLocation,
									bool bRunningFailedTestCases = false,
									int iNumberOfTimesRunningFailedTestCases = -1)
		{
			return new Action(() =>
			{
                if (this.TestCaseQueue.IsEmpty)
                {
                    return;
                }
                this.Agent.LogUtil.LogMessage(string.Format("Starting the execution of {0}test cases in {1} folder for {2}...",
																bRunningFailedTestCases ? "failed " : "", strExecutionLocation, TestConfig.ID));

				List<string> lstTestCaseGroup = null;
				var tasksOfTestGroupWindupActions = new List<Task>();
				while (!this.TestCaseQueue.IsEmpty)
				{
					lstTestCaseGroup = new List<string>();
					int iBucketSize = this.TestConfig.MaxBucketSize;
					lock (this.TestConfig)
					{
						iBucketSize = GetRecalculatedBucketSize(iBucketSize);

						while (iBucketSize != 0 && !this.TestCaseQueue.IsEmpty)
						{
							string strTestCase;
							this.TestCaseQueue.TryDequeue(out strTestCase);
							lstTestCaseGroup.Add(strTestCase);
							iBucketSize--;
						}
					}

					if (lstTestCaseGroup.Count > 0)
					{
						string strResultFile = GetResultFilePath(strExecutionLocation);

						_testRunner.Run(strGuidForNewAction,
												this.TestConfig,
												lstTestCaseGroup,
												strExecutionLocation,
												strResultFile,
												bRunningFailedTestCases,
												iNumberOfTimesRunningFailedTestCases);

						if (!File.Exists(strResultFile))
						{
							this.Agent.LogUtil.LogMessage(string.Format("Reprocessing current set of test cases in {0} folder for {1} ...",
																	strExecutionLocation, TestConfig.ID));

							ReprocessTestCaseGroup(strGuidForNewAction,
															lstTestCaseGroup,
															strExecutionLocation,
															strResultFile,
															bRunningFailedTestCases,
															iNumberOfTimesRunningFailedTestCases);
						}

						_totalNumberOfExecutedTestCases += lstTestCaseGroup.Count;

						Action actionTestGroupWindup = new Action(() =>
						{
							if (bRunningFailedTestCases)
							{
								_reportProcessor.Merge_TestCasePassedInAnyOneFileWillBeConsideredAsPassedOnly
														(GetConsolidatedTestReportFilePath(this.TestConfig.ExecutionLocation),
															strResultFile,
															this.TestConfig.CleanAfterCompletion.ToBool());
							}
							else
							{
								_reportProcessor.ConsolidateResults(GetConsolidatedTestReportFilePath(this.TestConfig.ExecutionLocation),
																				strResultFile,
																				this.TestConfig.CleanAfterCompletion.ToBool());
							}

							StringBuilder sbStatus = new StringBuilder();
							sbStatus.AppendLine("----------------------------");
							sbStatus.AppendLine("Status of configuration: {0}");
							sbStatus.AppendLine("Total {1}test cases: {2}, Executed: {3}({4}%)");
							//sbStatus.AppendLine("Remaining/In-Progress: {5}({6}%)");
							sbStatus.AppendLine();
							string strStatus = string.Format(sbStatus.ToString()
																		, TestConfig.ID
																		, bRunningFailedTestCases ? "failed " : ""
																		, _totalNumberOfTestCases
																		, _totalNumberOfExecutedTestCases
																		, (_totalNumberOfExecutedTestCases * 100) / _totalNumberOfTestCases
								//,_totalNumberOfTestCases - _totalNumberOfExecutedTestCases
								//,100 - ((_totalNumberOfExecutedTestCases * 100) / _totalNumberOfTestCases)
																		);

							this.Agent.LogUtil.LogStatus(strStatus);
						});

						tasksOfTestGroupWindupActions.Add(Task.Run(actionTestGroupWindup));
					}
				}

				Task.WhenAll(tasksOfTestGroupWindupActions).Wait();
			});
		}

		private int GetRecalculatedBucketSize(int iInitialBucketSize)
		{
			while (iInitialBucketSize > this.TestConfig.MinBucketSize &&
						(iInitialBucketSize * this.TestConfig.ThreadCount) > this.TestCaseQueue.Count)
			{
				iInitialBucketSize = Math.Max(iInitialBucketSize/2, this.TestConfig.MinBucketSize);
			}
			return iInitialBucketSize;
		}

		public override void ActionCompleted(Action action)
		{
			lock (this.TestConfig)
			{
				_dicActionResults[action] = true;
				_testRunner.AllTestCasesInAThreadAreExecuted(_dicActionIDs[action]);
				if (_iTimesToRerunFailedTestCases > 0 && !_dicActionResults.Values.Any(x => !x))
				{
					List<TestResultOutcomes> lstOutcomes = new List<TestResultOutcomes>()
																						{
																							TestResultOutcomes.Failed,
																							TestResultOutcomes.Pending
																						};

					var lstTestCasesWithGivenOutcomes = _reportProcessor.GetListOfTestCasesWithGivenOutomes(GetConsolidatedTestReportFilePath(
																																			this.TestConfig.ExecutionLocation),
																																			lstOutcomes);

					if (lstTestCasesWithGivenOutcomes.Count > 0)
					{
						_totalNumberOfTestCases = lstTestCasesWithGivenOutcomes.Count;
						_totalNumberOfExecutedTestCases = 0;

						lstTestCasesWithGivenOutcomes.ForEach(x => this.TestCaseQueue.Enqueue(x));
						for (int i = 0; i < this.TestConfig.ThreadCount; i++)
						{
							var guidForNewAction = Guid.NewGuid().ToString();

							int iTimesRerunningFailedTestCases = this.TestConfig.TimesToRerunFailedTestCases - _iTimesToRerunFailedTestCases + 1;
							string strActionExecutionLocation = GetNextActionExecutionLocation(true, iTimesRerunningFailedTestCases);

							Action actionForFailedTestCases = this.GetAction(guidForNewAction,
																								strActionExecutionLocation,
																								true,
																								iTimesRerunningFailedTestCases);

							_dicExecutionLocationsByAction.Add(actionForFailedTestCases, strActionExecutionLocation);
							_dicActionResults.Add(actionForFailedTestCases, false);
							_dicActionIDs.Add(actionForFailedTestCases, guidForNewAction);
							this.Agent.PTRManager.EnqueueTestCaseExecutor(this, actionForFailedTestCases);
						}
						_iTimesToRerunFailedTestCases--;
					}
					else
					{
						_iTimesToRerunFailedTestCases = -1;
					}
				}
			}
		}
	}
}
