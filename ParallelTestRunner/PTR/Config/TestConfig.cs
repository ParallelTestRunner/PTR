using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class TestConfig : BaseConfig
	{
		public string ID { get; private set; }

		public string SemicolonSeparatedConfigResultsToBeMergedInto { get; private set; }

		public string SemicolonSeparatedFilesHavingTestCases { get; private set; }

		public bool IsEnabled { get; private set; }

		public string TestingFramework { get; private set; }

		public string TestRunner { get; private set; }

		public bool? MakeTestRunnerAsChildProcessOfPTR { get; private set; }

		public string WorkingDirectoryOfTestRunner { get; private set; }

		public string ExecutionLocation { get; private set; }

		public string ReportingLocation { get; private set; }

		public string TestingProjectLocation { get; private set; }

		public bool? LoadTestingProjectBinariesFromItsOwnLocationOnly { get; private set; }

		public string TestCasesExtractor { get; private set; }

		public string TestCategories { get; private set; }

		public string TestClasses { get; private set; }

		public string SemicolonSeparatedTestCases { get; private set; }

		public string SemicolonSeparatedTestCasesToBeSkipped { get; private set; }

		public string BeforeRunConfigEditor { get; private set; }

		public int TimesToRerunFailedTestCases { get; private set; }

		public string BeforeRerunConfigEditor { get; private set; }

		public int ThreadCount { get; private set; }

		public int MinBucketSize { get; private set; }

		public int MaxBucketSize { get; private set; }

		public int ConcurrentUnit { get; private set; }

		public string ReportProcessor { get; private set; }

		public bool? CleanAfterCompletion { get; private set; }

		// Properties not specific to configuration
		public string OriginalID { get; private set; }

		public bool IsFirstConfig { get; private set; }

		public string LocationFromWhereTestingProjectBinariesAreToBeLoaded
		{
			get
			{
				if (LoadTestingProjectBinariesFromItsOwnLocationOnly.ToBool())
				{
					return this.TestingProjectLocation;
				}
				return this.ExecutionLocation;
			}
		}

		public TestConfig(TestConfigSection testConfigSection)
		{
			this.ID = testConfigSection.ID;
			this.SemicolonSeparatedConfigResultsToBeMergedInto = testConfigSection.SemicolonSeparatedConfigResultsToBeMergedInto;
			this.SemicolonSeparatedFilesHavingTestCases = testConfigSection.SemicolonSeparatedFilesHavingTestCases.Trim('\\');
			this.IsEnabled = testConfigSection.IsEnabled;
			this.TestingFramework = testConfigSection.TestingFramework;
			this.TestRunner = testConfigSection.TestRunner;
			this.MakeTestRunnerAsChildProcessOfPTR = testConfigSection.MakeTestRunnerAsChildProcessOfPTR;
			this.WorkingDirectoryOfTestRunner = testConfigSection.WorkingDirectoryOfTestRunner.Trim('\\');
			this.ExecutionLocation = testConfigSection.ExecutionLocation.Trim('\\');
			this.ReportingLocation = testConfigSection.ReportingLocation;
			this.TestingProjectLocation = testConfigSection.TestingProjectLocation.Trim('\\');
            this.LoadTestingProjectBinariesFromItsOwnLocationOnly = testConfigSection.LoadTestingProjectBinariesFromItsOwnLocationOnly;
			this.TestCasesExtractor = testConfigSection.TestCasesExtractor;
			this.TestCategories = testConfigSection.TestCategories;
			this.TestClasses = testConfigSection.TestClasses;
			this.SemicolonSeparatedTestCases = testConfigSection.SemicolonSeparatedTestCases;
			this.SemicolonSeparatedTestCasesToBeSkipped = testConfigSection.SemicolonSeparatedTestCasesToBeSkipped;
			this.BeforeRunConfigEditor = testConfigSection.BeforeRunConfigEditor;
			this.TimesToRerunFailedTestCases = testConfigSection.TimesToRerunFailedTestCases;
			this.BeforeRerunConfigEditor = testConfigSection.BeforeRerunConfigEditor;
			this.ThreadCount = testConfigSection.ThreadCount;
			this.MinBucketSize = testConfigSection.MinBucketSize;
			this.MaxBucketSize = testConfigSection.MaxBucketSize;
			this.ConcurrentUnit = testConfigSection.ConcurrentUnit;
			this.ReportProcessor = testConfigSection.ReportProcessor;
			this.CleanAfterCompletion = testConfigSection.CleanAfterCompletion;

			this.OriginalID = testConfigSection.ID;
			this.IsFirstConfig = true;
		}

		public TestConfig Clone()
		{
			return (TestConfig)this.MemberwiseClone();
		}

		protected override void InvalidPropertyAccessHandler(string propertyName)
		{
			throw new ArgumentException(string.Format("{0} is not valid attribute of a testing configuration", propertyName));
		}

		public override string Identifier { get { return string.Format("configuration.TestingConfigurations.Config[{0}]", this.OriginalID); } }
	}
}
