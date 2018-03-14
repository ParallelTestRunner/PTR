using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NUnitReportUtility
{
	class Program
	{
		static void Main(string[] args)
		{
            //args = new string[5];
            //args[0] = "/1";
            //args[1] = @"C:\Users\aseem\Downloads\OnePlanner\AssembliesHavingTestCasesForBamboo\ReportDifference\Bamboo Results Onep Main.xml";
            //args[2] = @"C:\Users\aseem\Downloads\OnePlanner\AssembliesHavingTestCasesForBamboo\ReportDifference\TestResults_1.xml";
            //args[3] = "Failed";
            //args[4] = "Pending";

            //args = new string[5];
            //args[0] = "/3";
            //args[1] = @"C:\CanBeDel\TestExecutionLocation\testResults_1.xml";
            //args[2] = @"C:\CanBeDel\TestExecutionLocation\Failed1";
            //args[3] = "Failed";
            //args[4] = "Pending";

            try
			{
				if (IsNUnit3OrAboveReportsProvided(args))
				{
					NUnitReportUtilityNunit3.ReportProcessorForNUnit3.Process(args);
				}
				else
				{
					NUnitReportUtilityNunit2.ReportProcessorForNUnit2.Process(args);
				}
			}
			catch
			{ }
		}

		private static bool IsNUnit3OrAboveReportsProvided(string[] args)
		{
			string strReportFile = args.Where(x => File.Exists(x)).First();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(strReportFile);
			if (xmlDoc.ChildNodes.Shim<XmlNode>().Where(x => x.Name == "test-run").Count() > 0)
			{
				return true;
			}

			return false;
		}
	}
}
