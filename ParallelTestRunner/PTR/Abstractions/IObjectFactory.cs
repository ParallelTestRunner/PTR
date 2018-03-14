using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface IObjectFactory
	{
		IPTRManager PTRManager { get; }
		ILogUtil LogUtil { get; }
		ITestingEnvironment TestingEnvironment { get; }
		ICommandLineProcessor CommandLineProcessor { get; }
		IInputOutputUtil InputOutputUtil { get; }

		ITestConfigManager GetConfigManagerByTestConfig(TestConfig TestConfig);
		ITestCaseRepository GetTestCaseRepository(TestConfig TestConfig);
		ITestRunner GetTestRunnerByTestConfig(TestConfig TestConfig);
		IReportProcessor GetReportManagerByTestConfig(TestConfig TestConfig);

		ILicenseChecker ThreadController { get; }
	}
}
