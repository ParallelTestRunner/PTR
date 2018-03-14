using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface IReportProcessor
	{
		void ConsolidateResults(string strConsolidatedFilePath,
										string strFileWhoseResultsAreToBeAdded,
										bool bDeleteFileWhoseResultsAreToBeAdded_AfterConsolidation = true);

		void Merge_TestCasePassedInAnyOneFileWillBeConsideredAsPassedOnly(string strConsolidatedFilePath,
																								string strFileWhoseResultsAreToBeAdded,
																								bool bDeleteFileWhoseResultsAreToBeAdded_AfterConsolidation = true);

		List<string> GetListOfTestCasesWithGivenOutomes(string strResultFilePath, List<TestResultOutcomes> lstOutcomes);
	}
}
