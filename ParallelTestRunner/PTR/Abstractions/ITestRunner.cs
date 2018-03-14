using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface ITestRunner
	{
		void Run(string strCallerId,
					TestConfig testConfig,
					IEnumerable<string> testCaseGroup,
					string strExecutionLocation,
					string strResultFile,
					bool bRunningFailedTestCase = false,
					int iNumberOfTimesRunningFailedTestCases = -1);

		void AllTestCasesInAThreadAreExecuted(string strCallerId);
	}
}
