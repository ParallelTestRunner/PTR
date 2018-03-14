using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitReportUtilityNunit2
{
	enum NodeNames
	{
		testResults,
		testSuite,
		results,
		testCase,
		failure,
		message,
		stackTrace
	}

	static class NodeAndAttributeNamesExtensions
	{
		private static string strTestResults = NodeNames.testResults.ToString();
		private static string strTestSuite = NodeNames.testSuite.ToString();
		private static string strNotRun = TestResultsAttributes.notRun.ToString();
		private static string strTestProject = TestSuiteTypes.TestProject.ToString();
		private static string strTestCase = NodeNames.testCase.ToString();
		private static string strStackTrace = NodeNames.stackTrace.ToString();

		public static string String(this object obj)
		{
			string strObj = obj.ToString();
			if (strTestResults == strObj)
				return "test-results";

			if (strTestProject == strObj)
				return "Test Project";

			if (strTestSuite == strObj)
				return "test-suite";

			if (strNotRun == strObj)
				return "not-run";

			if (strTestCase == strObj)
				return "test-case";

			if (strStackTrace == strObj)
				return "stack-trace";

			return strObj.ToString();
		}
	}

	enum TestResultsAttributes
	{
		total,
		errors,
		failures,
		notRun,
		inconclusive,
		ignored,
		skipped,
		invalid,
		date,
		time
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
		name,
		executed,
		result,
		success,
		time,
		asserts
	}

	enum ResultAttributeValues
	{
		Inconclusive,
		Ignored,
		Failure,
		Success
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
