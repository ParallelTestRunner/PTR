using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrxUtil
{
	interface IReportUtil
	{
		void Consolidate(string strConsolidatedReport, string strReportToBeMerged);

		void CreateXmlOfTestCasesWithGivenOutcomes(string strResultFile,
																	string strXmlToContainTestCasesWithGivenOutcomes,
																	IEnumerable<string> outcomes);
	}
}
