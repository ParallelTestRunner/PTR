using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	enum ConfigProperty
	{
		LoggingLocation,
		MaxThreads,
		BeforeTestExecution,
		AfterTestExecution,
		ID,
		SemicolonSeparatedConfigResultsToBeMergedInto,
		SemicolonSeparatedFilesHavingTestCases,
		IsEnabled,
		TestingFramework,
		TestRunner,
		MakeTestRunnerAsChildProcessOfPTR,
		WorkingDirectoryOfTestRunner,
		ExecutionLocation,
		ReportingLocation,
		TestingProjectLocation,
		LoadTestingProjectBinariesFromItsOwnLocationOnly,
		TestCasesExtractor,
		TestCategories,
		TestClasses,
		SemicolonSeparatedTestCases,
		SemicolonSeparatedTestCasesToBeSkipped,
		BeforeRunConfigEditor,
		TimesToRerunFailedTestCases,
		BeforeRerunConfigEditor,
		ThreadCount,
		MinBucketSize,
		MaxBucketSize,
		ConcurrentUnit,
		ReportProcessor,
		CleanAfterCompletion		
	}

	static class ConfigPropertyExtensions
	{
		public static ConfigProperty ToProperty(this string strProperty)
		{
			foreach (ConfigProperty property in Enum.GetValues(typeof(ConfigProperty)))
			{
				if (property.ToString().ToLower() == strProperty.ToLower())
				{
					return property;
				}
			}

			throw new ArgumentException(string.Format("{0} is not a valid argument", strProperty));
		}
	}
}
