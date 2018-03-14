using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnitReportUtility;

namespace NUnitReportUtilityNunit2
{
	class ReportProcessorForNUnit2
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
																							ResultAttributeValues.Failure,
																							ResultAttributeValues.Inconclusive,
																							ResultAttributeValues.Ignored,
																							ResultAttributeValues.Success
																						};

				IReportUtil nunitReportUtility = new NUnit2ReportUtility(lstOutcomePriority);
				nunitReportUtility.Consolidate(args[1], args[2]);
			}
			else if (args[0] == "/2")
			{
				List<ResultAttributeValues> lstOutcomePriority = new List<ResultAttributeValues>()
																						{
																							ResultAttributeValues.Success,
																							ResultAttributeValues.Failure,
																							ResultAttributeValues.Inconclusive,
																							ResultAttributeValues.Ignored
																						};

				IReportUtil nunitReportUtility = new NUnit2ReportUtility(lstOutcomePriority);
				nunitReportUtility.Consolidate(args[1], args[2]);
			}
			else if (args[0] == "/3")
			{
				List<string> lstOutcome = new List<string>();
				for (int i = 3; i < args.Length; i++)
				{
					lstOutcome.Add(ConvertToAppropriateResultAttributeValue(args[i].Trim().ToLower()));
				}

				IReportUtil nunitReportUtility = new NUnit2ReportUtility();
				nunitReportUtility.CreateXmlOfTestCasesWithGivenOutcomes(args[1], args[2], lstOutcome.Distinct());
			}
		}

		private static string ConvertToAppropriateResultAttributeValue(string strResultType)
		{
			switch (strResultType)
			{
				case "passed":
					return ResultAttributeValues.Success.ToString();

				case "failed":
					return ResultAttributeValues.Failure.ToString();

				case "pending":
					return ResultAttributeValues.Inconclusive.ToString();

				default:
					return ResultAttributeValues.Inconclusive.ToString();
			}
		}
	}
}
