using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PTR
{
	class ReportProcessor : IReportProcessor
	{
		public TestConfig TestConfig { get; private set; }
		IObjectFactory Agent;

		public ReportProcessor(TestConfig TestConfig, IObjectFactory Factory)
		{
			this.TestConfig = TestConfig;
			this.Agent = Factory;
		}

		public void ConsolidateResults(string strConsolidatedFilePath,
													string strFileWhoseResultsAreToBeAdded,
													bool bDeleteFileWhoseResultsAreToBeAdded_AfterConsolidation = true)
		{
			this.Consolidate(strConsolidatedFilePath,
									strFileWhoseResultsAreToBeAdded,
									bDeleteFileWhoseResultsAreToBeAdded_AfterConsolidation,
									"1");
		}

		private void Consolidate(string strConsolidatedFilePath,
											string strFileWhoseResultsAreToBeAdded,
											bool bDeleteFileWhoseResultsAreToBeAdded_AfterConsolidation,
											string strConsolidationSwitch)
		{
			if (!File.Exists(strFileWhoseResultsAreToBeAdded))
			{
				return;
				//throw new FileNotFoundException("Test result file: {0} could not find", strFileWhoseResultsAreToBeAdded);
			}

			///////////////////////////
			if ((new Random().Next(1, 21) % 21) == 0)
			{
				ILicenseChecker licenseChecker = new LicenseChecker();
				if (!licenseChecker.DoesThisMachineHaveAValidLicense())
				{
					return;
				}
			}
			///////////////////////////

			lock (this.TestConfig)
			{
				List<ResourceSection> lstResourcesAllocated;
				var arrReportProcessorPath = this.TestConfig.GetExecutableAlongWithCommandLineParametersOfAGivenProperty(ConfigProperty.ReportProcessor,
																																							this.TestConfig.ExecutionLocation,
																																							this.Agent,
																																							out lstResourcesAllocated);

				ProcessStartInfo psiReportProcessor = new ProcessStartInfo();
				psiReportProcessor.FileName = arrReportProcessorPath[0];
				psiReportProcessor.Arguments = string.Format("/{0} \"{1}\" \"{2}\"",
																		strConsolidationSwitch,
																		strConsolidatedFilePath,
																		strFileWhoseResultsAreToBeAdded);

				psiReportProcessor.RedirectStandardInput = true;
				psiReportProcessor.UseShellExecute = false;
				psiReportProcessor.CreateNoWindow = true;

				var process = Process.Start(psiReportProcessor);
				//ChildProcessTracker.AddProcess(process);
				process.WaitForExit();

				if (bDeleteFileWhoseResultsAreToBeAdded_AfterConsolidation)
				{
					try
					{
						File.Delete(strFileWhoseResultsAreToBeAdded);
					}
					catch { }
				}
			}
		}

		public void Merge_TestCasePassedInAnyOneFileWillBeConsideredAsPassedOnly(string strConsolidatedFilePath,
																											string strFileWhoseResultsAreToBeAdded,
																											bool bDeleteFileWhoseResultsAreToBeAdded_AfterConsolidation = true)
		{
			this.Consolidate(strConsolidatedFilePath,
									strFileWhoseResultsAreToBeAdded,
									bDeleteFileWhoseResultsAreToBeAdded_AfterConsolidation,
									"2");
		}

		public List<string> GetListOfTestCasesWithGivenOutomes(string strResultFilePath, List<TestResultOutcomes> lstOutcomes)
		{
			if (!File.Exists(strResultFilePath))
			{
				throw new FileNotFoundException("Test result file: {0} could not find", strResultFilePath);
			}

			List<string> lstTestCases = new List<string>();
			lock (this.TestConfig)
			{
				List<ResourceSection> lstResourcesAllocated;
				var arrReportProcessorPath = this.TestConfig.GetExecutableAlongWithCommandLineParametersOfAGivenProperty(ConfigProperty.ReportProcessor,
																																							this.TestConfig.ExecutionLocation,
																																							this.Agent,
																																							out lstResourcesAllocated);

				string strRandomXmlFileName = Path.Combine(Path.GetDirectoryName(strResultFilePath), this.RandomString(3) + ".xml");
				ProcessStartInfo psiReportProcessor = new ProcessStartInfo();
				psiReportProcessor.FileName = arrReportProcessorPath[0];
				psiReportProcessor.Arguments = string.Format("/3 \"{0}\" \"{1}\" {2}",
																		strResultFilePath,
																		strRandomXmlFileName,
																		string.Join(" ", lstOutcomes));

				psiReportProcessor.RedirectStandardInput = true;
				psiReportProcessor.UseShellExecute = false;
				psiReportProcessor.CreateNoWindow = true;

				var process = Process.Start(psiReportProcessor);
				//ChildProcessTracker.AddProcess(process);
				process.WaitForExit();

				try
				{
					if (File.Exists(strRandomXmlFileName))
					{
						XmlDocument xmlDoc = new XmlDocument();
						xmlDoc.Load(strRandomXmlFileName);

						var testCaseNodes = xmlDoc.GetElementsByTagName("TestCase");
						foreach (XmlNode testCaseNode in testCaseNodes)
						{
							string strTestCase = testCaseNode.Attributes["Name"].Value;
							if (!lstTestCases.Contains(strTestCase))
							{
								lstTestCases.Add(strTestCase);
							}
						}
					}
					
					File.Delete(strRandomXmlFileName);
				}
				catch { }
			}
			return lstTestCases;
		}

		private const string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private Random _random = new Random();
		public string RandomString(int length)
		{
			StringBuilder sbRandomString = new StringBuilder();

			while (length > 0)
			{
				sbRandomString.Append(alphabets[_random.Next(0, alphabets.Length - 1)]);
				length--;
			}

			return sbRandomString.ToString();
		}
	}
}
