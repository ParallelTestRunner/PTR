using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace PTR
{
	class AppConfigReader
	{
		public static ProcessWideConfigSection ProcessWideConfigSection
		{
			get { return (ProcessWideConfigSection)ConfigurationManager.GetSection("ProcessWideConfig"); }
		}

		public static TestingConfigurationsSection TestingConfigurationsSection
		{
			get { return (TestingConfigurationsSection)ConfigurationManager.GetSection("TestingConfigurations"); }
		}
	}

	class ProcessWideConfigSection : ConfigurationSection
	{
		[ConfigurationProperty("LoggingLocation", DefaultValue = "", IsRequired = false)]
		public string LoggingLocation { get { return (string)base[ConfigProperty.LoggingLocation.ToString()]; } }

		[ConfigurationProperty("MaxThreads", DefaultValue = -1, IsRequired = false)]
		public int MaxThreads { get { return (int)base[ConfigProperty.MaxThreads.ToString()]; } }

		[ConfigurationProperty("BeforeTestExecution", DefaultValue = "", IsRequired = false)]
		public string BeforeTestExecution { get { return (string)base[ConfigProperty.BeforeTestExecution.ToString()]; } }

		[ConfigurationProperty("AfterTestExecution", DefaultValue = "", IsRequired = false)]
		public string AfterTestExecution { get { return (string)base[ConfigProperty.AfterTestExecution.ToString()]; } }

		[ConfigurationProperty("SemicolonSeparatedFilesHavingTestCases", DefaultValue = "", IsRequired = false)]
		public string SemicolonSeparatedFilesHavingTestCases { get { return (string)base[ConfigProperty.SemicolonSeparatedFilesHavingTestCases.ToString()]; } }

		[ConfigurationProperty("TestingFramework", DefaultValue = "", IsRequired = false)]
		public string TestingFramework { get { return (string)base[ConfigProperty.TestingFramework.ToString()]; } }

		[ConfigurationProperty("TestRunner", DefaultValue = "", IsRequired = false)]
		public string TestRunner { get { return (string)base[ConfigProperty.TestRunner.ToString()]; } }

		[ConfigurationProperty("MakeTestRunnerAsChildProcessOfPTR", DefaultValue = "false", IsRequired = false)]
		public bool MakeTestRunnerAsChildProcessOfPTR { get { return (bool)base[ConfigProperty.MakeTestRunnerAsChildProcessOfPTR.ToString()]; } }

		[ConfigurationProperty("WorkingDirectoryOfTestRunner", DefaultValue = "", IsRequired = false)]
		public string WorkingDirectoryOfTestRunner { get { return (string)base[ConfigProperty.WorkingDirectoryOfTestRunner.ToString()]; } }

		[ConfigurationProperty("ExecutionLocation", DefaultValue = ".", IsRequired = false)]
		public string ExecutionLocation { get { return (string)base[ConfigProperty.ExecutionLocation.ToString()]; } }

		[ConfigurationProperty("TestingProjectLocation", DefaultValue = ".", IsRequired = false)]
		public string TestingProjectLocation { get { return (string)base[ConfigProperty.TestingProjectLocation.ToString()]; } }

		[ConfigurationProperty("LoadTestingProjectBinariesFromItsOwnLocationOnly", DefaultValue = "false", IsRequired = false)]
		public bool LoadTestingProjectBinariesFromItsOwnLocationOnly
		{
			get { return (bool)base[ConfigProperty.LoadTestingProjectBinariesFromItsOwnLocationOnly.ToString()]; }
		}

		[ConfigurationProperty("TestCasesExtractor", DefaultValue = "", IsRequired = false)]
		public string TestCasesExtractor { get { return (string)base[ConfigProperty.TestCasesExtractor.ToString()]; } }

		[ConfigurationProperty("TestCategories", DefaultValue = "", IsRequired = false)]
		public string TestCategories { get { return (string)base[ConfigProperty.TestCategories.ToString()]; } }

		[ConfigurationProperty("TestClasses", DefaultValue = "", IsRequired = false)]
		public string TestClasses { get { return (string)base[ConfigProperty.TestClasses.ToString()]; } }

		[ConfigurationProperty("SemicolonSeparatedTestCases", DefaultValue = "", IsRequired = false)]
		public string SemicolonSeparatedTestCases { get { return (string)base[ConfigProperty.SemicolonSeparatedTestCases.ToString()]; } }

		[ConfigurationProperty("SemicolonSeparatedTestCasesToBeSkipped", DefaultValue = "", IsRequired = false)]
		public string SemicolonSeparatedTestCasesToBeSkipped { get { return (string)base[ConfigProperty.SemicolonSeparatedTestCasesToBeSkipped.ToString()]; } }

		[ConfigurationProperty("BeforeRunConfigEditor", DefaultValue = "", IsRequired = false)]
		public string BeforeRunConfigEditor { get { return (string)base[ConfigProperty.BeforeRunConfigEditor.ToString()]; } }

		[ConfigurationProperty("TimesToRerunFailedTestCases", DefaultValue = "0", IsRequired = false)]
		public int TimesToRerunFailedTestCases { get { return (int)base[ConfigProperty.TimesToRerunFailedTestCases.ToString()]; } }

		[ConfigurationProperty("BeforeRerunConfigEditor", DefaultValue = "", IsRequired = false)]
		public string BeforeRerunConfigEditor { get { return (string)base[ConfigProperty.BeforeRerunConfigEditor.ToString()]; } }

		[ConfigurationProperty("ThreadCount", DefaultValue = -1, IsRequired = false)]
		public int ThreadCount { get { return (int)base[ConfigProperty.ThreadCount.ToString()]; } }

		[ConfigurationProperty("MinBucketSize", DefaultValue = 3, IsRequired = false)]
		public int MinBucketSize { get { return (int)base[ConfigProperty.MinBucketSize.ToString()]; } }

		[ConfigurationProperty("MaxBucketSize", DefaultValue = 50, IsRequired = false)]
		public int MaxBucketSize { get { return (int)base[ConfigProperty.MaxBucketSize.ToString()]; } }

		[ConfigurationProperty("ConcurrentUnit", DefaultValue = 2, IsRequired = false)]
		public int ConcurrentUnit { get { return (int)base[ConfigProperty.ConcurrentUnit.ToString()]; } }

		[ConfigurationProperty("ReportProcessor", DefaultValue = "", IsRequired = false)]
		public string ReportProcessor { get { return (string)base[ConfigProperty.ReportProcessor.ToString()]; } }

		[ConfigurationProperty("CleanAfterCompletion", DefaultValue = "true", IsRequired = false)]
		public bool CleanAfterCompletion { get { return (bool)base[ConfigProperty.CleanAfterCompletion.ToString()]; } }
	}

	class TestingConfigurationsSection : ConfigurationSection
	{
		[ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
		[ConfigurationCollection(typeof(TestConfigSection), AddItemName = "Config")]
		public GenericConfigElementCollection<TestConfigSection> TestConfigSectionCollection
		{ get { return (GenericConfigElementCollection<TestConfigSection>)this[""]; } }
	}

	class GenericConfigElementCollection<T> : ConfigurationElementCollection, IEnumerable<T> where T : ConfigurationElement, new()
	{
		private List<T> _lstElements = new List<T>();

		public new IEnumerator<T> GetEnumerator()
		{
			return _lstElements.GetEnumerator();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			var element = new T();
			_lstElements.Add(element);
			return element;
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return _lstElements.Find(item => item.Equals(element));
		}
	}

	class TestConfigSection : ConfigurationSection
	{
		[ConfigurationProperty("ID", DefaultValue = "", IsRequired = true)]
		public string ID { get { return (string)base[ConfigProperty.ID.ToString()]; } }

		[ConfigurationProperty("SemicolonSeparatedConfigResultsToBeMergedInto", DefaultValue = "", IsRequired = false)]
		public string SemicolonSeparatedConfigResultsToBeMergedInto { get { return (string)base[ConfigProperty.SemicolonSeparatedConfigResultsToBeMergedInto.ToString()]; } }

		[ConfigurationProperty("SemicolonSeparatedFilesHavingTestCases", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string SemicolonSeparatedFilesHavingTestCases { get { return (string)base[ConfigProperty.SemicolonSeparatedFilesHavingTestCases.ToString()]; } }

		[ConfigurationProperty("IsEnabled", DefaultValue = "true", IsRequired = false)]
		public bool IsEnabled { get { return (bool)base[ConfigProperty.IsEnabled.ToString()]; } }

		[ConfigurationProperty("TestingFramework", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string TestingFramework { get { return (string)base[ConfigProperty.TestingFramework.ToString()]; } }

		[ConfigurationProperty("TestRunner", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string TestRunner { get { return (string)base[ConfigProperty.TestRunner.ToString()]; } }

		[ConfigurationProperty("MakeTestRunnerAsChildProcessOfPTR", IsRequired = false)]
		public bool? MakeTestRunnerAsChildProcessOfPTR { get { return (bool?)base[ConfigProperty.MakeTestRunnerAsChildProcessOfPTR.ToString()]; } }

		[ConfigurationProperty("WorkingDirectoryOfTestRunner", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string WorkingDirectoryOfTestRunner { get { return (string)base[ConfigProperty.WorkingDirectoryOfTestRunner.ToString()]; } }

		[ConfigurationProperty("ExecutionLocation", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string ExecutionLocation { get { return (string)base[ConfigProperty.ExecutionLocation.ToString()]; } }

		[ConfigurationProperty("ReportingLocation", DefaultValue = "", IsRequired = false)]
		public string ReportingLocation { get { return (string)base[ConfigProperty.ReportingLocation.ToString()]; } }

		[ConfigurationProperty("TestingProjectLocation", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string TestingProjectLocation { get { return (string)base[ConfigProperty.TestingProjectLocation.ToString()]; } }

		[ConfigurationProperty("LoadTestingProjectBinariesFromItsOwnLocationOnly", IsRequired = false)]
		public bool? LoadTestingProjectBinariesFromItsOwnLocationOnly
		{
			get { return (bool?)base[ConfigProperty.LoadTestingProjectBinariesFromItsOwnLocationOnly.ToString()]; }
		}

		[ConfigurationProperty("TestCasesExtractor", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string TestCasesExtractor { get { return (string)base[ConfigProperty.TestCasesExtractor.ToString()]; } }

		[ConfigurationProperty("TestCategories", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string TestCategories { get { return (string)base[ConfigProperty.TestCategories.ToString()]; } }

		[ConfigurationProperty("TestClasses", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string TestClasses { get { return (string)base[ConfigProperty.TestClasses.ToString()]; } }

		[ConfigurationProperty("SemicolonSeparatedTestCases", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string SemicolonSeparatedTestCases { get { return (string)base[ConfigProperty.SemicolonSeparatedTestCases.ToString()]; } }

		[ConfigurationProperty("SemicolonSeparatedTestCasesToBeSkipped", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string SemicolonSeparatedTestCasesToBeSkipped { get { return (string)base[ConfigProperty.SemicolonSeparatedTestCasesToBeSkipped.ToString()]; } }

		[ConfigurationProperty("BeforeRunConfigEditor", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string BeforeRunConfigEditor { get { return (string)base[ConfigProperty.BeforeRunConfigEditor.ToString()]; } }

		[ConfigurationProperty("TimesToRerunFailedTestCases", DefaultValue = CONSTANTS.NoIntValueProvided, IsRequired = false)]
		public int TimesToRerunFailedTestCases { get { return (int)base[ConfigProperty.TimesToRerunFailedTestCases.ToString()]; } }

		[ConfigurationProperty("BeforeRerunConfigEditor", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string BeforeRerunConfigEditor { get { return (string)base[ConfigProperty.BeforeRerunConfigEditor.ToString()]; } }

		[ConfigurationProperty("ThreadCount", DefaultValue = CONSTANTS.NoIntValueProvided, IsRequired = false)]
		public int ThreadCount { get { return (int)base[ConfigProperty.ThreadCount.ToString()]; } }

		[ConfigurationProperty("MinBucketSize", DefaultValue = CONSTANTS.NoIntValueProvided, IsRequired = false)]
		public int MinBucketSize { get { return (int)base[ConfigProperty.MinBucketSize.ToString()]; } }

		[ConfigurationProperty("MaxBucketSize", DefaultValue = CONSTANTS.NoIntValueProvided, IsRequired = false)]
		public int MaxBucketSize { get { return (int)base[ConfigProperty.MaxBucketSize.ToString()]; } }

		[ConfigurationProperty("ConcurrentUnit", DefaultValue = CONSTANTS.NoIntValueProvided, IsRequired = false)]
		public int ConcurrentUnit { get { return (int)base[ConfigProperty.ConcurrentUnit.ToString()]; } }

		[ConfigurationProperty("ReportProcessor", DefaultValue = CONSTANTS.NoStringValueProvided, IsRequired = false)]
		public string ReportProcessor { get { return (string)base[ConfigProperty.ReportProcessor.ToString()]; } }

		[ConfigurationProperty("CleanAfterCompletion", IsRequired = false)]
		public bool? CleanAfterCompletion { get { return (bool?)base[ConfigProperty.CleanAfterCompletion.ToString()]; } }
	}

	class SharedResourcesSection : ConfigurationSection
	{
		[ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
		[ConfigurationCollection(typeof(ResourceSection))]
		public ResourceEntries ResourcekEntries
		{
			get { return (ResourceEntries)this[""]; }
			set { this[""] = value; }
		}
	}

	class ResourceEntries : ConfigurationElementCollection, IEnumerable<ResourceSection>
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new ResourceSection();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ResourceSection)element).ID;
		}

		IEnumerator<ResourceSection> IEnumerable<ResourceSection>.GetEnumerator()
		{
			foreach (var key in this.BaseGetAllKeys()) { yield return (ResourceSection)BaseGet(key); }
		}
	}

	class ResourceSection : ConfigurationSection
	{
		[ConfigurationProperty("ID", IsRequired = true)]
		public string ID { get { return (string)base["ID"]; } }

		[ConfigurationProperty("SemicolonSeparatedResources", IsRequired = true)]
		public string SemicolonSeparatedResources { get { return (string)base["SemicolonSeparatedResources"]; } }
	}
}
