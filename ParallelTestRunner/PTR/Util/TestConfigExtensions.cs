using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	static class TestConfigExtensions
	{
		public static string GetTitleForCurrentThread(this TestConfig testConfig,
																		string strExecutionLocation,
																		bool bRunningFailedTestCase = false,
																		int iNumberOfTimesRunningFailedTestCases = -1)
		{
			var directoryInfo = new DirectoryInfo(strExecutionLocation);
			string strTitle = directoryInfo.Name;
			if (testConfig.ConcurrentUnit.ConcurentUnitType() == ConcurentUnitType.TestCaseLevel)
			{
				strTitle = directoryInfo.Parent.Name + ": " + directoryInfo.Name;
			}

			if (!bRunningFailedTestCase)
			{
				return strTitle;
			}

			return "Running failed test cases - " + strTitle + " - " + iNumberOfTimesRunningFailedTestCases.ToOrdinal() + " times";
		}
	}
}
