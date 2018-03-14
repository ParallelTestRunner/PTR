using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnitReportUtility;

namespace NUnitReportUtilityNunit3
{
	class ReportProcessorForNUnit3
	{
		public static void Process(string[] args)
		{
			NUtil.ILicenseChecker licenseChecker = new NUtil.LicenseChecker();
			if ((new Random().Next(1, 11) % 11) == 0)
			{
				if (!licenseChecker.DoesThisMachineHaveAValidLicense())
				{
					return;
				}
			}

			if (args[0] == "/1")
			{
				List<ResultAttributeValues> lstOutcomePriority = new List<ResultAttributeValues>()
																						{
																							ResultAttributeValues.Failed,
																							ResultAttributeValues.Inconclusive,
																							ResultAttributeValues.Skipped,
																							ResultAttributeValues.Passed
																						};

				IReportUtil nunitReportUtility = new NUnit3ReportUtility(lstOutcomePriority);
				nunitReportUtility.Consolidate(args[1], args[2]);
			}
			else if (args[0] == "/2")
			{
				List<ResultAttributeValues> lstOutcomePriority = new List<ResultAttributeValues>()
																						{
																							ResultAttributeValues.Passed,
																							ResultAttributeValues.Failed,
																							ResultAttributeValues.Inconclusive,
																							ResultAttributeValues.Skipped
																						};

				IReportUtil nunitReportUtility = new NUnit3ReportUtility(lstOutcomePriority);
				nunitReportUtility.Consolidate(args[1], args[2]);
			}
			else if (args[0] == "/3")
			{
				List<string> lstOutcome = new List<string>();
				for (int i = 3; i < args.Length; i++)
				{
					lstOutcome.Add(ConvertToAppropriateResultAttributeValue(args[i].Trim().ToLower()));
				}

				IReportUtil nunitReportUtility = new NUnit3ReportUtility();
				nunitReportUtility.CreateXmlOfTestCasesWithGivenOutcomes(args[1], args[2], lstOutcome.Distinct());
			}
		}

		private static string ConvertToAppropriateResultAttributeValue(string strResultType)
		{
			switch (strResultType)
			{
				case "passed":
					return ResultAttributeValues.Passed.ToString();

				case "failed":
					return ResultAttributeValues.Failed.ToString();

				case "pending":
					return ResultAttributeValues.Inconclusive.ToString();

				default:
					return ResultAttributeValues.Inconclusive.ToString();
			}
		}
	}
}
