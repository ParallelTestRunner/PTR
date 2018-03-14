using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	abstract class AbstractTestConfigManager : ITestConfigManager
	{
		public TestConfig TestConfig { get; protected set; }
		protected ITestCaseRepository _testCaseRepository;
		protected ITestRunner _testRunner;
		protected IReportProcessor _reportProcessor;

		protected IObjectFactory Agent;

		protected List<string> TestClassesToBeConsidered = new List<string>();
		protected List<string> TestCasesToBeConsidered = new List<string>();
		protected List<string> TestCasesToBeSkipped = new List<string>();

		public AbstractTestConfigManager(TestConfig TestConfig, IObjectFactory Factory)
		{
			this.TestConfig = TestConfig;
			this.Agent = Factory;
			_testCaseRepository = this.Agent.GetTestCaseRepository(this.TestConfig);
			_testRunner = this.Agent.GetTestRunnerByTestConfig(this.TestConfig);
			_reportProcessor = this.Agent.GetReportManagerByTestConfig(this.TestConfig);

			this.Agent.LogUtil.LogMessage(string.Format("Filtering test cases for {0} ...", TestConfig.ID));
			this.FillDataMembersRelatedToTestCaseSelectionCriteria();
		}

		protected TestCategoryFilter _testCategoryFilter;
		protected virtual void FillDataMembersRelatedToTestCaseSelectionCriteria()
		{
			_testCategoryFilter = TestCategoryFilter.GetTestCategoryFilter(this.TestConfig.TestCategories);

			var testObjects = this.TestConfig.TestClasses.Split('|')
														.Where(x => !string.IsNullOrEmpty(x.Trim())).Select(x => x.Trim()).Distinct();

			foreach (var testClass in testObjects)
			{
				TestClassesToBeConsidered.Add(testClass);
			}

			testObjects = this.TestConfig.SemicolonSeparatedTestCases.Split(';')
                                            .Where(x => !string.IsNullOrEmpty(x.Trim())).Select(x => x.Trim()).Distinct();

			foreach (var testCase in testObjects)
			{
				TestCasesToBeConsidered.Add(testCase);
			}

			testObjects = this.TestConfig.SemicolonSeparatedTestCasesToBeSkipped.Split(';')
											.Where(x => !string.IsNullOrEmpty(x.Trim())).Select(x => x.Trim()).Distinct();

			foreach (var testClass in testObjects)
			{
				TestCasesToBeSkipped.Add(testClass);
			}
		}

		protected virtual List<TestCase> GetTestCasesToBeConsideredFromGivenList(IEnumerable<TestCase> testCases)
		{
			return testCases.Where(x => ((this.TestClassesToBeConsidered.Count <= 0 ||
														this.TestClassesToBeConsidered.Any(y => x.Class.Contains(y))) &&
				
													(this.TestCasesToBeConsidered.Count <= 0 ||
														this.TestCasesToBeConsidered.Any(y => x.Name.Contains(y)) ||
														this.TestCasesToBeConsidered.Any(y => x.FullName.Contains(y))) &&
					
													_testCategoryFilter.IsTestCaseToBeConsidered(x)) &&
													!this.TestCasesToBeSkipped.Any(y => x.Name.Contains(y)) &&
													!this.TestCasesToBeSkipped.Any(y => x.FullName.Contains(y))).ToList();
		}

		protected Dictionary<Action, bool> _dicActionResults = new Dictionary<Action, bool>();
		protected Dictionary<Action, string> _dicActionIDs = new Dictionary<Action, string>();
		public abstract List<Action> TestCaseExecutors { get; }

		protected int MaxReprocessLevel = 8;
		protected void ReprocessTestCaseGroup(string strGuidForCurrentAction,
															IEnumerable<string> testCaseGroup,
															string strTestCasesExecutionLocation,
															string strResultFile,
															bool bRunningFailedTestCases,
															int iNumberOfTimesRunningFailedTestCases,
															int iReprocessLevel = 1)
		{
			int iResultFilePostFix = 0;
			var testCaseGroups = testCaseGroup.SplitByBinSize(2).ToList();
			foreach (var tempTestCaseGroup in testCaseGroups)
			{
				iResultFilePostFix++;
				string strTempResultFile = strResultFile + "_" + iResultFilePostFix.ToString();
				
				_testRunner.Run(strGuidForCurrentAction,
										this.TestConfig,
										tempTestCaseGroup,
										strTestCasesExecutionLocation,
										strTempResultFile,
										bRunningFailedTestCases,
										iNumberOfTimesRunningFailedTestCases);

				if (!File.Exists(strTempResultFile) && iReprocessLevel <= MaxReprocessLevel)
				{
					ReprocessTestCaseGroup(strGuidForCurrentAction,
													tempTestCaseGroup,
													strTestCasesExecutionLocation,
													strTempResultFile,
													bRunningFailedTestCases,
													iNumberOfTimesRunningFailedTestCases,
													iReprocessLevel + 1);
				}

				_reportProcessor.ConsolidateResults(strResultFile, strTempResultFile);
			}
		}

		protected Dictionary<string, int> _dicTestReportFileNumberByLocation = new Dictionary<string, int>();
		protected string GetResultFilePath(string strTestCasesExecutionLocation)
		{
			string strResultFile;
			lock (this.TestConfig)
			{
				if (!_dicTestReportFileNumberByLocation.ContainsKey(strTestCasesExecutionLocation))
				{
					_dicTestReportFileNumberByLocation.Add(strTestCasesExecutionLocation, 1);
				}

				strResultFile = Path.Combine(strTestCasesExecutionLocation, CONSTANTS.IntermediateTestResultFileNamePrefix) +
																								_dicTestReportFileNumberByLocation[strTestCasesExecutionLocation];

				_dicTestReportFileNumberByLocation[strTestCasesExecutionLocation] = _dicTestReportFileNumberByLocation[strTestCasesExecutionLocation] + 1;
			}
			return strResultFile;
		}

		protected Dictionary<string, string> _dicConsolidatedTestReportFilePathByLocation = new Dictionary<string, string>();
		protected string GetConsolidatedTestReportFilePath(string strTestCasesExecutionLocation)
		{
			string strConsolidatedFile;
			lock (this.TestConfig)
			{
				if (!_dicConsolidatedTestReportFilePathByLocation.ContainsKey(strTestCasesExecutionLocation))
				{
					strConsolidatedFile = Path.Combine(strTestCasesExecutionLocation, CONSTANTS.ConsolidatedFileName);
					_dicConsolidatedTestReportFilePathByLocation.Add(strTestCasesExecutionLocation, strConsolidatedFile);
				}
				else
				{
					strConsolidatedFile = _dicConsolidatedTestReportFilePathByLocation[strTestCasesExecutionLocation];
				}
			}
			return strConsolidatedFile;
		}

		public abstract void ActionCompleted(Action action);

		public string ConsolidatedResultsFile
		{
			get { return Path.Combine(this.TestConfig.ExecutionLocation, CONSTANTS.ConsolidatedFileName); }
		}

		public void MergeOtherConfigManagersResultsWhereeverRequired(IEnumerable<ITestConfigManager> otherTestConfigManagers)
		{
			var configWhoseResultsAreToBeMergedWithCurrentConfig = this.TestConfig.SemicolonSeparatedConfigResultsToBeMergedInto.Split(';').ToList();
			if (configWhoseResultsAreToBeMergedWithCurrentConfig.Count > 0)
			{
				foreach (var testConfigManager in otherTestConfigManagers)
				{
					if (configWhoseResultsAreToBeMergedWithCurrentConfig.Contains(testConfigManager.TestConfig.ID))
					{
						if (File.Exists(testConfigManager.ConsolidatedResultsFile))
						{
							_reportProcessor.ConsolidateResults(this.ConsolidatedResultsFile, testConfigManager.ConsolidatedResultsFile, false);
						}
					}
				}
			}
		}

		public void CleanUp()
		{
			if (!this.TestConfig.CleanAfterCompletion.ToBool())
			{
				return;
			}

			this.Agent.InputOutputUtil.DeleteDirectoryAndAllItsContent(this.TestConfig.ExecutionLocation,
																							new List<string>()
																							{
																								CONSTANTS.IntermediateTestResultFileNamePrefix,
																								CONSTANTS.ConsolidatedFileName,
																								CONSTANTS.ConfigFileExtension
																							});
		}
	}
}
