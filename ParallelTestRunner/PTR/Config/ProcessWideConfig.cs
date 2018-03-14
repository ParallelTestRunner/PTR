using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class ProcessWideConfig : BaseConfig
	{
		public string LoggingLocation { get; private set; }

		public int MaxThreads { get; private set; }

		public string BeforeTestExecution { get; private set; }

		public string AfterTestExecution { get; private set; }

		public string SemicolonSeparatedFilesHavingTestCases { get; private set; }

		public string TestingFramework { get; private set; }

		public string TestRunner { get; private set; }

		public bool MakeTestRunnerAsChildProcessOfPTR { get; private set; }

		public string WorkingDirectoryOfTestRunner { get; private set; }

		public string ExecutionLocation { get; private set; }

		public string TestingProjectLocation { get; private set; }

		public bool LoadTestingProjectBinariesFromItsOwnLocationOnly { get; private set; }

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

		public bool CleanAfterCompletion { get; private set; }

		public ProcessWideConfig(ProcessWideConfigSection processWideConfigSection)
		{
			this.LoggingLocation = processWideConfigSection.LoggingLocation.Trim('\\');
			this.MaxThreads = processWideConfigSection.MaxThreads != -1 ? processWideConfigSection.MaxThreads : Environment.ProcessorCount;
			this.BeforeTestExecution = processWideConfigSection.BeforeTestExecution;
			this.AfterTestExecution = processWideConfigSection.AfterTestExecution;
			this.SemicolonSeparatedFilesHavingTestCases = processWideConfigSection.SemicolonSeparatedFilesHavingTestCases.Trim('\\');
			this.TestingFramework = processWideConfigSection.TestingFramework;
			this.TestRunner = processWideConfigSection.TestRunner;
			this.MakeTestRunnerAsChildProcessOfPTR = processWideConfigSection.MakeTestRunnerAsChildProcessOfPTR;
			this.WorkingDirectoryOfTestRunner = processWideConfigSection.WorkingDirectoryOfTestRunner.Trim('\\');
			this.ExecutionLocation = processWideConfigSection.ExecutionLocation.Trim('\\');
            this.TestingProjectLocation = processWideConfigSection.TestingProjectLocation.Trim('\\');
            this.LoadTestingProjectBinariesFromItsOwnLocationOnly = processWideConfigSection.LoadTestingProjectBinariesFromItsOwnLocationOnly;
			this.TestCasesExtractor = processWideConfigSection.TestCasesExtractor;
			this.TestCategories = processWideConfigSection.TestCategories;
			this.TestClasses = processWideConfigSection.TestClasses;
			this.SemicolonSeparatedTestCases = processWideConfigSection.SemicolonSeparatedTestCases;
			this.SemicolonSeparatedTestCasesToBeSkipped = processWideConfigSection.SemicolonSeparatedTestCasesToBeSkipped;
			this.BeforeRunConfigEditor = processWideConfigSection.BeforeRunConfigEditor;
			this.TimesToRerunFailedTestCases = processWideConfigSection.TimesToRerunFailedTestCases;
			this.BeforeRerunConfigEditor = processWideConfigSection.BeforeRerunConfigEditor;
			this.ThreadCount = processWideConfigSection.ThreadCount != -1 ? processWideConfigSection.ThreadCount : Environment.ProcessorCount;
            this.MinBucketSize = processWideConfigSection.MinBucketSize;
			this.MaxBucketSize = processWideConfigSection.MaxBucketSize;
			this.ConcurrentUnit = processWideConfigSection.ConcurrentUnit;
			this.ReportProcessor = processWideConfigSection.ReportProcessor;
			this.CleanAfterCompletion = processWideConfigSection.CleanAfterCompletion;
		}

		protected override void InvalidPropertyAccessHandler(string propertyName)
		{
			throw new ArgumentException(string.Format("{0} is not valid attribute of ProcessWideConfig", propertyName));
		}

		public override string Identifier { get { return "configuration.ProcessWideConfig"; } }
	}
}
