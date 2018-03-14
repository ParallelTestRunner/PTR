using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;

namespace PTR
{
	class TestConfigManagerWithTestClassAsConcurentUnit : AbstractTestConfigManager
	{
		public TestConfigManagerWithTestClassAsConcurentUnit(TestConfig TestConfig, IObjectFactory Factory)
			: base(TestConfig, Factory)
		{
		}

		Dictionary<string, int> _dicTotalNumberOfTestCasesByClass = new Dictionary<string,int>();
		Dictionary<string, int> _dicTotalNumberOfExecutedTestCasesByClass = new Dictionary<string, int>();

		Dictionary<string, int> _dicTimesToRerunFailedTestCasesByClass;
		private Dictionary<string, Action> _dicActionsByClass;
		public override List<Action> TestCaseExecutors
		{
			get
			{
				if (_dicActionsByClass == null)
				{
					this.Agent.LogUtil.LogMessage(string.Format("Getting test case executors for {0} ...", TestConfig.ID));

					_dicActionsByClass = new Dictionary<string, Action>();
					_dicTimesToRerunFailedTestCasesByClass = new Dictionary<string, int>();
					foreach (var keyValPair in _testCaseRepository.TestCasesByClass)
					{
						var testCasesToBeConsideredByClass = this.GetTestCasesToBeConsideredFromGivenList(keyValPair.Value);

						if (testCasesToBeConsideredByClass.Count() > 0)
						{
							_dicTotalNumberOfTestCasesByClass.Add(keyValPair.Key, testCasesToBeConsideredByClass.Count());
							_dicTotalNumberOfExecutedTestCasesByClass.Add(keyValPair.Key, 0);

							_dicTimesToRerunFailedTestCasesByClass.Add(keyValPair.Key, 0);

							var guidForNewAction = Guid.NewGuid().ToString();
							Action action = this.GetAction(guidForNewAction, keyValPair.Key, testCasesToBeConsideredByClass.Select(t => t.FullName));

							_dicActionsByClass.Add(keyValPair.Key, action);
							_dicActionResults.Add(action, false);
							_dicActionIDs.Add(action, guidForNewAction);
						}
					}
				}

				return _dicActionResults.Keys.ToList();
			}
		}

		private string GetClassNameWithoutProjectNameOrNamespace(string strFullTestClassName)
		{
			return strFullTestClassName.Split('.').Last();
		}

		private Action GetAction(string strGuidForNewAction,
											string strFullTestClassName,
											IEnumerable<string> testCases,
											bool bRunningFailedTestCases = false,
											int iNumberOfTimesRunningFailedTestCases = -1)
		{
			string strClassName = GetClassNameWithoutProjectNameOrNamespace(strFullTestClassName);
			return new Action(() =>
			{
				this.Agent.LogUtil.LogMessage(string.Format("Starting the execution of {0}test cases for class {1} for {2} ...",
														bRunningFailedTestCases ? "failed " : "", strFullTestClassName, TestConfig.ID));

				var testCaseGroups = testCases.SplitByChunk(this.TestConfig.MaxBucketSize).ToList();
				string strTestCasesExecutionLocation = GetExecutionLocationForTestClass(strClassName);
				var tasksOfTestGroupWindupActions = new List<Task>();
				foreach (var testCaseGroup in testCaseGroups)
				{
					string strResultFile = bRunningFailedTestCases ?
													GetResultFilePathForFailedTestCases(strFullTestClassName, strTestCasesExecutionLocation) :
													GetResultFilePath(strTestCasesExecutionLocation);

					_testRunner.Run(strGuidForNewAction,
											this.TestConfig,
											testCaseGroup,
											strTestCasesExecutionLocation,
											strResultFile,
											bRunningFailedTestCases,
											iNumberOfTimesRunningFailedTestCases);

					if (!File.Exists(strResultFile))
					{
						this.Agent.LogUtil.LogMessage(string.Format("Reprocessing current set of test cases in {0} class for {1} ...",
																	strFullTestClassName, TestConfig.ID));

						ReprocessTestCaseGroup(strGuidForNewAction,
														testCaseGroup,
														strTestCasesExecutionLocation,
														strResultFile,
														bRunningFailedTestCases,
														iNumberOfTimesRunningFailedTestCases);
					}

					_dicTotalNumberOfExecutedTestCasesByClass[strFullTestClassName] += testCaseGroup.Count();

					Action actionTestGroupWindup = new Action(() =>
					{
						if (bRunningFailedTestCases)
						{
							_reportProcessor.Merge_TestCasePassedInAnyOneFileWillBeConsideredAsPassedOnly
													(GetConsolidatedTestReportFilePath(strTestCasesExecutionLocation),
														strResultFile,
														this.TestConfig.CleanAfterCompletion.ToBool());
						}
						else
						{
							_reportProcessor.ConsolidateResults(GetConsolidatedTestReportFilePath(strTestCasesExecutionLocation),
																			strResultFile,
																			this.TestConfig.CleanAfterCompletion.ToBool());
						}

						string strClassConsolidatedFile = Path.Combine(strTestCasesExecutionLocation, CONSTANTS.ConsolidatedFileName);
						_reportProcessor.Merge_TestCasePassedInAnyOneFileWillBeConsideredAsPassedOnly(this.ConsolidatedResultsFile, strClassConsolidatedFile, false);

						int iTotalNumberOfTestCases = _dicTotalNumberOfTestCasesByClass[strFullTestClassName];
						int iTotalNumberOfExecutedTestCases = _dicTotalNumberOfExecutedTestCasesByClass[strFullTestClassName];

						StringBuilder sbStatus = new StringBuilder();
						sbStatus.AppendLine("----------------------------");
						sbStatus.AppendLine("Status of configuration: {0}");
						sbStatus.AppendLine("Total {1}test cases in class \"{2}\": {3}, Executed: {4}({5}%)");
						//sbStatus.AppendLine("Remaining/In-Progress: {6}({7}%)");
						sbStatus.AppendLine();
						string strStatus = string.Format(sbStatus.ToString()
																		, TestConfig.ID
																		, bRunningFailedTestCases ? "failed " : ""
																		, strFullTestClassName
																		, iTotalNumberOfTestCases
																		, iTotalNumberOfExecutedTestCases
																		, (iTotalNumberOfExecutedTestCases * 100) / iTotalNumberOfTestCases
							//,iTotalNumberOfTestCases - iTotalNumberOfExecutedTestCases
							//,100 - ((iTotalNumberOfExecutedTestCases * 100) / iTotalNumberOfTestCases)
																		);

						this.Agent.LogUtil.LogStatus(strStatus);
					});

					tasksOfTestGroupWindupActions.Add(Task.Run(actionTestGroupWindup));
				}

				Task.WhenAll(tasksOfTestGroupWindupActions).Wait();
			});
		}

		private string GetExecutionLocationForTestClass(string strClassName)
		{
			return Path.Combine(this.TestConfig.ExecutionLocation, strClassName.Replace("\"", ""));
		}

		protected string GetResultFilePathForFailedTestCases(string strFullTestClassName, string strTestCasesExecutionLocation)
		{
			return Path.Combine(strTestCasesExecutionLocation, CONSTANTS.IntermediateTestResultFileNamePrefixForFailedTestCases) +
																				_dicTimesToRerunFailedTestCasesByClass[strFullTestClassName];
		}

		public override void ActionCompleted(Action action)
		{
			_dicActionResults[action] = true;
			_testRunner.AllTestCasesInAThreadAreExecuted(_dicActionIDs[action]);

			string strFullTestClassName = _dicActionsByClass
													.Where(p => p.Value == action)
													.Select(p => p.Key)
													.First();

			string strClassNameWithoutProjectNameOrNamespace = GetClassNameWithoutProjectNameOrNamespace(strFullTestClassName);
			string strExecutionLocation = GetExecutionLocationForTestClass(strClassNameWithoutProjectNameOrNamespace);

			string strClassConsolidatedFile = Path.Combine(strExecutionLocation, CONSTANTS.ConsolidatedFileName);
			//_reportProcessor.Merge_TestCasePassedInAnyOneFileWillBeConsideredAsPassedOnly(this.ConsolidatedResultsFile, strClassConsolidatedFile, false);

			if (_dicTimesToRerunFailedTestCasesByClass[strFullTestClassName] < this.TestConfig.TimesToRerunFailedTestCases)
			{
				_dicTimesToRerunFailedTestCasesByClass[strFullTestClassName]++;
				List<TestResultOutcomes> lstOutcomes = new List<TestResultOutcomes>()
																						{
																							TestResultOutcomes.Failed,
																							TestResultOutcomes.Pending
																						};

				var lstTestCasesWithGivenOutcomes = _reportProcessor.GetListOfTestCasesWithGivenOutomes(strClassConsolidatedFile, lstOutcomes);
				if (lstTestCasesWithGivenOutcomes.Count > 0)
				{
					_dicTotalNumberOfTestCasesByClass[strFullTestClassName] = lstTestCasesWithGivenOutcomes.Count;
					_dicTotalNumberOfExecutedTestCasesByClass[strFullTestClassName] = 0;

					var guidForNewAction = Guid.NewGuid().ToString();
					Action actionForFailedTestCases = this.GetAction(guidForNewAction,
																						strFullTestClassName,
																						lstTestCasesWithGivenOutcomes,
																						true,
																						_dicTimesToRerunFailedTestCasesByClass[strFullTestClassName]);

					_dicActionsByClass[strFullTestClassName] = actionForFailedTestCases;
					_dicActionResults.Add(actionForFailedTestCases, false);
					_dicActionIDs.Add(actionForFailedTestCases, guidForNewAction);
					this.Agent.PTRManager.EnqueueTestCaseExecutor(this, actionForFailedTestCases);
				}
			}
		}
	}
}
