using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class ObjectFactory : IObjectFactory
	{
		private IPTRManager _ptrManager;
		public IPTRManager PTRManager { get { if (_ptrManager == null) { _ptrManager = new PTRManager(this); } return _ptrManager; } }

		private ILogUtil _logUtil;
		public ILogUtil LogUtil { get { if (_logUtil == null) { _logUtil = new LogUtil(this); } return _logUtil; } }

		private ITestingEnvironment _testingEnvironment;
		public ITestingEnvironment TestingEnvironment
		{
			get
			{
				if (_testingEnvironment == null)
				{
					_testingEnvironment = new TestingEnvironment(AppConfigReader.ProcessWideConfigSection, AppConfigReader.TestingConfigurationsSection, this);
				}
				return _testingEnvironment;
			}
		}

		private ICommandLineProcessor _commandLineProcessor;
		public ICommandLineProcessor CommandLineProcessor
		{
			get { if (_commandLineProcessor == null) { _commandLineProcessor = new CommandLineProcessor(this); } return _commandLineProcessor; }
		}

		private IInputOutputUtil _inputOutputUtil;
		public IInputOutputUtil InputOutputUtil
		{
			get { if (_inputOutputUtil == null) { _inputOutputUtil = new InputOutputUtil(); } return _inputOutputUtil; }
		}

		private Dictionary<TestConfig, ITestConfigManager> _dicConfigManagerByTestConfig;
		public ITestConfigManager GetConfigManagerByTestConfig(TestConfig TestConfig)
		{
			if (_dicConfigManagerByTestConfig == null)
			{
				_dicConfigManagerByTestConfig = new Dictionary<TestConfig, ITestConfigManager>();
			}

			if (_dicConfigManagerByTestConfig.ContainsKey(TestConfig))
			{
				return _dicConfigManagerByTestConfig[TestConfig];
			}

			ITestConfigManager configRunner = null;
			if (TestConfig.ConcurrentUnit.ConcurentUnitType() == ConcurentUnitType.ClassLevel)
			{
				configRunner = new TestConfigManagerWithTestClassAsConcurentUnit(TestConfig, this);
			}
			else if (TestConfig.ConcurrentUnit.ConcurentUnitType() == ConcurentUnitType.TestCaseLevel)
			{
				configRunner = new TestConfigManagerWithTestCaseAsConcurentUnit(TestConfig, this);
			}

			_dicConfigManagerByTestConfig.Add(TestConfig, configRunner);
			return configRunner;
		}

		private Dictionary<string, ITestCaseRepository> _dicTestCaseRepositoriesByFilesHavingTestCases;
		public ITestCaseRepository GetTestCaseRepository(TestConfig TestConfig)
		{
			if (_dicTestCaseRepositoriesByFilesHavingTestCases == null)
			{
				_dicTestCaseRepositoriesByFilesHavingTestCases = new Dictionary<string, ITestCaseRepository>();
			}

			string strFileHavingTestCasesNormalisedPath = Path.Combine(TestConfig.ExecutionLocation,
																							TestConfig.SemicolonSeparatedFilesHavingTestCases.FirstFile().NormalisedPath());

			if (_dicTestCaseRepositoriesByFilesHavingTestCases.ContainsKey(strFileHavingTestCasesNormalisedPath))
			{
				return _dicTestCaseRepositoriesByFilesHavingTestCases[strFileHavingTestCasesNormalisedPath];
			}

			ITestCaseRepository testCaseRepository = new TestCaseRepository(TestConfig, this);
			_dicTestCaseRepositoriesByFilesHavingTestCases.Add(strFileHavingTestCasesNormalisedPath, testCaseRepository);
			return testCaseRepository;
		}

		private Dictionary<TestConfig, ITestRunner> _dicTestRunnerByTestConfig;
		public ITestRunner GetTestRunnerByTestConfig(TestConfig TestConfig)
		{
			ITestRunner testRunner = null;
			lock (this)
			{
				if (_dicTestRunnerByTestConfig == null)
				{
					_dicTestRunnerByTestConfig = new Dictionary<TestConfig, ITestRunner>();
				}

				if (_dicTestRunnerByTestConfig.ContainsKey(TestConfig))
				{
					testRunner = _dicTestRunnerByTestConfig[TestConfig];
				}

				if (testRunner == null)
				{
					testRunner = new TestRunner(this);
					_dicTestRunnerByTestConfig.Add(TestConfig, testRunner);
				}
			}
			return testRunner;
		}

		private Dictionary<TestConfig, IReportProcessor> _dicReportManagerByTestConfig;
		public IReportProcessor GetReportManagerByTestConfig(TestConfig TestConfig)
		{
			IReportProcessor reportProcessor = null;
			lock (this)
			{
				if (_dicReportManagerByTestConfig == null)
				{
					_dicReportManagerByTestConfig = new Dictionary<TestConfig, IReportProcessor>();
				}

				if (_dicReportManagerByTestConfig.ContainsKey(TestConfig))
				{
					reportProcessor = _dicReportManagerByTestConfig[TestConfig];
				}

				if (reportProcessor == null)
				{
					reportProcessor = new ReportProcessor(TestConfig, this);
					_dicReportManagerByTestConfig.Add(TestConfig, reportProcessor);
				}
			}
			return reportProcessor;
		}

		private ILicenseChecker _threadController;
		public ILicenseChecker ThreadController
		{
			get { if (_threadController == null) { _threadController = new LicenseChecker(); } return _threadController; }
		}
	}
}
