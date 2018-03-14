using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface ITestingEnvironment
	{
		ProcessWideConfig ProcessWideConfig { get; }
		List<TestConfig> TestingConfigurations { get; }
		string TestExecutionFolderName { get; }
	}
}
