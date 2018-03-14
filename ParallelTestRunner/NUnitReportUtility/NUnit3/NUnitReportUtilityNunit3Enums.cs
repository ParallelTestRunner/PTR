using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitReportUtilityNunit3
{
	enum NodeNames
	{
		testRun,
		commandLine,
		filter,
		testSuite,
		testCase,
		failure,
		message,
		stackTrace
	}

	static class NodeAndAttributeNamesExtensions
	{
		private static string strTestResults = NodeNames.testRun.ToString();
		private static string strTestSuite = NodeNames.testSuite.ToString();
		private static string strTestProject = TestSuiteTypes.TestProject.ToString();
		private static string strTestCase = NodeNames.testCase.ToString();
		private static string strStackTrace = NodeNames.stackTrace.ToString();
		private static string strStartTime = TestRunAttributes.startTime.ToString();
		private static string strEndTime = TestRunAttributes.endTime.ToString();
		private static string strCommandLine = NodeNames.commandLine.ToString();

		public static string String(this object obj)
		{
			string strObj = obj.ToString();
			if (strTestResults == strObj)
				return "test-run";

			if (strCommandLine == strObj)
				return "command-line";

			if (strTestProject == strObj)
				return "Test Project";

			if (strTestSuite == strObj)
				return "test-suite";

			if (strTestCase == strObj)
				return "test-case";

			if (strStackTrace == strObj)
				return "stack-trace";

			if (strStartTime == strObj)
				return "start-time";

			if (strEndTime == strObj)
				return "end-time";

			return strObj.ToString();
		}
	}

	enum TestRunAttributes
	{
		testcasecount,
		result,
		total,
		passed,
		failed,
		inconclusive,
		skipped,
		asserts,
		startTime,
		endTime,
		duration
	}

	enum TestSuiteTypes
	{
		TestProject,
		Assembly,
		Namespace,
		TestFixture
	}

	enum TestSuiteAttributes
	{
		type,
		id,
		runstate,
		fullname,
		testcasecount,
		result,
		total,
		passed,
		failed,
		inconclusive,
		skipped,
		asserts,
		duration,
		site,
		startTime,
		endTime
	}

	enum RunStateAttributes
	{
		Runnable,
		Ignored
	}

	enum SiteAttributes
	{
		Child
	}

	enum TestCaseAttributes
	{
		id,
		result,
		runstate,
		asserts,
		duration
	}

	enum ResultAttributeValues
	{
		Inconclusive,
		Skipped,
		Failed,
		Passed
	}

	static class ResultAttributeValuesExtensions
	{
		public static ResultAttributeValues ToResultAttributeValue(this string strResult)
		{
			foreach (ResultAttributeValues outcome in Enum.GetValues(typeof(ResultAttributeValues)))
			{
				if (outcome.ToString().ToLower() == strResult.ToLower())
				{
					return outcome;
				}
			}

			//throw new ArgumentException(string.Format("{0} is not a valid argument", strResult));
			return ResultAttributeValues.Inconclusive;
		}
	}
}