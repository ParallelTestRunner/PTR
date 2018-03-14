using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrxUtil
{
	enum NodeNames
	{
		Times,
		ResultSummary,
		Counters,
		TestDefinitions,
		UnitTest,
		Results,
		UnitTestResult,
		TestEntries,
		TestEntry,
		Execution,
		TestMethod
	}

	enum UnitTestAttributes
	{
		id
	}

	enum UnitTestResultAttributes
	{
		executionId,
		testId,
		outcome
	}

	enum TestEntryAttributes
	{
		testId,
		executionId
	}

	enum ExecutionAttributes
	{
		id
	}

	enum TestMethodAttributes
	{
		className,
		name
	}

	static class NodeNamesExtensions
	{
		public static NodeNames GetParentNodeName(this NodeNames nodeName)
		{
			switch (nodeName)
			{
				case NodeNames.UnitTest:
					return NodeNames.TestDefinitions;

				case NodeNames.UnitTestResult:
					return NodeNames.Results;

				case NodeNames.TestEntry:
					return NodeNames.TestEntries;

				case NodeNames.Execution:
					return NodeNames.UnitTest;
			}

			throw new InvalidOperationException(string.Format("Parent node of {0} is not specified", nodeName.ToString()));
		}
	}

	enum ResultSummaryAttributes
	{
		outcome
	}

	enum TestResultOutcomeAttributeValues
	{
		Pending,
		Failed,
		Passed
	}

	static class ResultSummaryOutcomeAttributeValuesExtensions
	{
		public static TestResultOutcomeAttributeValues ToResultSummaryOutcome(this string strOutcome)
		{
			foreach (TestResultOutcomeAttributeValues outcome in Enum.GetValues(typeof(TestResultOutcomeAttributeValues)))
			{
				if (outcome.ToString().ToLower() == strOutcome.ToLower())
				{
					return outcome;
				}
			}

			//throw new ArgumentException(string.Format("{0} is not a valid argument", strOutcome));
			return TestResultOutcomeAttributeValues.Pending;
		}
	}

	enum TimeAttributes
	{
		creation,
		queuing,
		start,
		finish
	}

	enum CounterAttributes
	{
		total,
		executed,
		passed,
		error,
		failed,
		timeout,
		aborted,
		inconclusive,
		passedButRunAborted,
		notRunnable,
		notExecuted,
		disconnected,
		warning,
		completed,
		inProgress,
		pending
	}
}
