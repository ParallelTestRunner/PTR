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
	class TestCaseRepository : ITestCaseRepository
	{
		private List<TestCase> _allTestCases = null;
		public List<TestCase> AllTestCases
		{
			get
			{
				if (_allTestCases == null)
				{
					this.Agent.LogUtil.LogMessage(string.Format("Fetching test cases for TestConfig:{0}...", TestConfig.ID));
					_allTestCases = new List<TestCase>();

                    List<ResourceSection> lstResourcesAllocated;
                    var arrPathWithParameters = this.TestConfig.GetExecutableAlongWithCommandLineParametersOfAGivenProperty(ConfigProperty.TestCasesExtractor,
                                                                                                                            this.TestConfig.ExecutionLocation,
                                                                                                                            this.Agent,
                                                                                                                            out lstResourcesAllocated);

                    ProcessStartInfo psi = new ProcessStartInfo();
					psi.FileName = arrPathWithParameters[0];
					psi.Arguments = "\"" +
									Path.Combine(this.TestConfig.LocationFromWhereTestingProjectBinariesAreToBeLoaded,
                                                    this.TestConfig.SemicolonSeparatedFilesHavingTestCases.FirstFile()) +
									"\" " +
									"\"" +
									_strXmlFileHavingTestCases +
									"\" ";
                    if (arrPathWithParameters.Length > 1)
                    {
                        psi.Arguments += " \"" + arrPathWithParameters[1] + "\"";
                    }

                    psi.RedirectStandardInput = true;
					psi.UseShellExecute = false;
					psi.CreateNoWindow = true;

					var process = Process.Start(psi);
					//ChildProcessTracker.AddProcess(process);
					process.WaitForExit();

					bool bFileExists = File.Exists(_strXmlFileHavingTestCases);

					///////////////////////////
					if ((new Random().Next(1, 5) % 5) == 0)
					{
						bFileExists = bFileExists && (new LicenseChecker()).DoesThisMachineHaveAValidLicense();
					}
					///////////////////////////

					if (!bFileExists)
					{
						this.Agent.LogUtil.LogWarning("Test cases could not be extracted from " + this.TestConfig.SemicolonSeparatedFilesHavingTestCases.FirstFile());
						if (File.Exists(_strXmlFileHavingTestCases))
						{
							File.Delete(_strXmlFileHavingTestCases);
						}
						return _allTestCases;
					}

					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.Load(_strXmlFileHavingTestCases);

					foreach (XmlNode testCaseNode in xmlDoc.GetElementsByTagName("TestCase"))
					{
						if (!Convert.ToBoolean(testCaseNode.Attributes["Ignore"].Value))
						{
							_allTestCases.Add(new TestCase(testCaseNode.Attributes["TestCaseName"].Value,
																		testCaseNode.Attributes["TestClassFullName"].Value,
																		testCaseNode.Attributes["TestCategories"].Value));
						}
					}

					//File.Delete(_strXmlFileHavingTestCases);

					_allTestCases = _allTestCases.Shuffle().ToList();
				}
				return _allTestCases;
			}
		}

		private Dictionary<string, List<TestCase>> _testCasesByClass = null;
		public Dictionary<string, List<TestCase>> TestCasesByClass
		{
			get
			{
				if (_testCasesByClass == null)
				{
					_testCasesByClass = new Dictionary<string, List<TestCase>>();
					foreach (var testCase in this.AllTestCases)
					{
						List<TestCase> lstTestCases;
						if (_testCasesByClass.ContainsKey(testCase.Class))
						{
							lstTestCases = _testCasesByClass[testCase.Class];
						}
						else
						{
							lstTestCases = new List<TestCase>();
							_testCasesByClass.Add(testCase.Class, lstTestCases);
						}
						lstTestCases.Add(testCase);
					}
				}
				return _testCasesByClass;
			}
		}

		private TestConfig TestConfig { get; set; }
		private string _strXmlFileHavingTestCases;
		IObjectFactory Agent;

		public TestCaseRepository(TestConfig TestConfig, IObjectFactory Factory)
		{
			this.TestConfig = TestConfig;
			this.Agent = Factory;

			string strFileHavingTestCases = this.TestConfig.SemicolonSeparatedFilesHavingTestCases.FirstFile();
			_strXmlFileHavingTestCases = strFileHavingTestCases.Replace(Path.GetFileName(strFileHavingTestCases),
																							Path.GetFileNameWithoutExtension(strFileHavingTestCases) +
																							CONSTANTS.FileHavingTestCases);

			_strXmlFileHavingTestCases = Path.Combine(this.TestConfig.LocationFromWhereTestingProjectBinariesAreToBeLoaded, _strXmlFileHavingTestCases);
		}
	}
}
