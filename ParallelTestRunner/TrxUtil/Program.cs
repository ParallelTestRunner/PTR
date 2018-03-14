using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrxUtil
{
	class Program
	{
		static void Main(string[] args)
		{
			TrxExten.ILicenseChecker licenseChecker = new TrxExten.LicenseChecker();
			if ((new Random().Next(1, 11) % 11) == 0)
			{
				if (!licenseChecker.DoesThisMachineHaveAValidLicense())
				{
					return;
				}
			}

			try
			{
				if (args[0] == "/1")
				{
					List<TestResultOutcomeAttributeValues> lstOutcomePriority = new List<TestResultOutcomeAttributeValues>()
																								{
																									TestResultOutcomeAttributeValues.Failed,
																									TestResultOutcomeAttributeValues.Pending,
																									TestResultOutcomeAttributeValues.Passed
																								};
					IReportUtil trxUtil = new TrxUtil(lstOutcomePriority);
					trxUtil.Consolidate(args[1], args[2]);
				}
				else if (args[0] == "/2")
				{
					List<TestResultOutcomeAttributeValues> lstOutcomePriority = new List<TestResultOutcomeAttributeValues>()
																								{
																									TestResultOutcomeAttributeValues.Passed,
																									TestResultOutcomeAttributeValues.Failed,
																									TestResultOutcomeAttributeValues.Pending
																								};
					IReportUtil trxUtil = new TrxUtil(lstOutcomePriority);
					trxUtil.Consolidate(args[1], args[2]);
				}
				else if (args[0] == "/3")
				{
					List<string> lstOutcome = new List<string>();
					for (int i = 3; i < args.Length; i++)
					{
						lstOutcome.Add(args[i]);
					}

					IReportUtil trxUtil = new TrxUtil();
					trxUtil.CreateXmlOfTestCasesWithGivenOutcomes(args[1], args[2], lstOutcome
																								.Select(x => x.Trim())
																								.Where(x => !string.IsNullOrEmpty(x))
																								.Select(x => x.ToLower())
																								.Distinct());
				}
			}
			catch
			{ }
		}
	}
}
