using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PTR
{
	class TestingEnvironment : ITestingEnvironment
	{
		public ProcessWideConfig ProcessWideConfig { get; private set; }
		public List<TestConfig> TestingConfigurations { get; private set; }

		IObjectFactory Agent;

		public TestingEnvironment(ProcessWideConfigSection processWideConfigSection,
                                    TestingConfigurationsSection testingConfigurationsSection,
									IObjectFactory Factory)
		{
			this.Agent = Factory;
			Factory.LogUtil.LogMessage("Creating testing environment...");

			Factory.LogUtil.LogMessage("Creating process wide configurations");
			this.ProcessWideConfig = new ProcessWideConfig(processWideConfigSection);

			Factory.LogUtil.LogMessage("Creating testing configurations...");
			var tempTestConfigCollection = testingConfigurationsSection.TestConfigSectionCollection.Select((x) => new TestConfig(x)).ToList();

            Factory.CommandLineProcessor.ApplyCommandLineArguments(this.ProcessWideConfig, tempTestConfigCollection);

			this.TestingConfigurations = new List<TestConfig>();
			foreach (var tempTestConfig in tempTestConfigCollection)
			{
				if (tempTestConfig.IsEnabled)
				{
					this.ApplyProcessWideConfigurationsToTestConfigIfRequired(tempTestConfig);
                    this.CreateSeparateTestConfigurationForEachFileHavingTestCases(tempTestConfig);
				}
				else
				{
					Factory.LogUtil.LogWarning(string.Format("Testing configuration {0} is disabled, so it is not being processed", tempTestConfig.Identifier));
				}
			}

			this.Update_SemicolonSeparatedConfigResultsToBeMergedInto_FieldInAllParallelConfigurations();

			this.UpdateProcessWideConfig();

			this.ValidateAllConfigurations();

			Factory.LogUtil.LogMessage("Copying all required components for all enabled testing configurations at their corresponding execution location...");
			this.CopyRequiredComponentsForAllEnabledConfigurationsAtTheirCorrespondingExecutionLocation();

			Factory.LogUtil.LogMessage("Testing environment is created successfully.");
		}

		private void ApplyProcessWideConfigurationsToTestConfigIfRequired(TestConfig testConfig)
		{
			Agent.LogUtil.LogMessage("Applying process wide configurations to testing configurations if required...");

			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.SemicolonSeparatedFilesHavingTestCases.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.TestingFramework.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.TestRunner.ToString());
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.WorkingDirectoryOfTestRunner.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.ExecutionLocation.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.TestingProjectLocation.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.TestCasesExtractor.ToString());
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.TestCategories.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.TestClasses.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.SemicolonSeparatedTestCases.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.SemicolonSeparatedTestCasesToBeSkipped.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.BeforeRunConfigEditor.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.BeforeRerunConfigEditor.ToString(), false);
			this.ApplyProcessWideConfigurationStringTypeProperty(testConfig, ConfigProperty.ReportProcessor.ToString());

			this.ApplyProcessWideConfigurationIntTypeProperty(testConfig, ConfigProperty.TimesToRerunFailedTestCases.ToString());
			this.ApplyProcessWideConfigurationIntTypeProperty(testConfig, ConfigProperty.ThreadCount.ToString());
			this.ApplyProcessWideConfigurationIntTypeProperty(testConfig, ConfigProperty.MinBucketSize.ToString());
			this.ApplyProcessWideConfigurationIntTypeProperty(testConfig, ConfigProperty.MaxBucketSize.ToString());
			this.ApplyProcessWideConfigurationIntTypeProperty(testConfig, ConfigProperty.ConcurrentUnit.ToString());

			this.ApplyProcessWideConfigurationBoolTypeProperty(testConfig, ConfigProperty.LoadTestingProjectBinariesFromItsOwnLocationOnly.ToString());
			this.ApplyProcessWideConfigurationBoolTypeProperty(testConfig, ConfigProperty.MakeTestRunnerAsChildProcessOfPTR.ToString());
			this.ApplyProcessWideConfigurationBoolTypeProperty(testConfig, ConfigProperty.CleanAfterCompletion.ToString());
		}

		private void ApplyProcessWideConfigurationStringTypeProperty(TestConfig testConfig, string strProperty, bool bRequired = true)
		{
            if (testConfig[strProperty].ToString() == CONSTANTS.NoStringValueProvided)
			{
                testConfig[strProperty] = this.ProcessWideConfig[strProperty];
            }

			if (bRequired && string.IsNullOrEmpty(testConfig[strProperty].ToString()))
			{
				throw new MissingFieldException(string.Format("{0}.{1} field is empty", testConfig.Identifier, strProperty));
			}
		}

		private void ApplyProcessWideConfigurationIntTypeProperty(TestConfig testConfig, string strProperty)
		{
			int iValue = int.Parse(testConfig[strProperty].ToString());
			if (iValue == CONSTANTS.NoIntValueProvided)
			{
				testConfig[strProperty] = this.ProcessWideConfig[strProperty];
			}
		}

		private void ApplyProcessWideConfigurationBoolTypeProperty(TestConfig testConfig, string strProperty)
		{
			object objValue = testConfig[strProperty];
			if (objValue == null)
			{
				testConfig[strProperty] = this.ProcessWideConfig[strProperty];
			}
		}

		private Dictionary<string, string> _dic_SemicolonSeparatedConfigResultsToBeMergedInto_Fields_Replacements = new Dictionary<string, string>();
		private void CreateSeparateTestConfigurationForEachFileHavingTestCases(TestConfig tempTestConfig)
		{
			Agent.LogUtil.LogMessage("Creating separate test configuration for each file having test cases...");

			TestConfig firstTestConfigForAFileHavingTestCases = null;
			var filesHavingTestCases = tempTestConfig.SemicolonSeparatedFilesHavingTestCases.Split(';')
                                        .Where(i => !string.IsNullOrEmpty(i)).Select(j => j.Trim('\\')).Distinct();

			if (filesHavingTestCases.Count() < 1)
			{
				filesHavingTestCases = GetSemicolonSeparatedAssembliesHavingTestCases(tempTestConfig);
			}

			bool bReportingLocationIsToBeReset = false;
			foreach (var file in filesHavingTestCases)
			{
				TestConfig testConfigForAFileHavingTestCases = GetNewTestConfigFromAnExistingTestConfigForAFileHavingTestCases(tempTestConfig, file);
				if (bReportingLocationIsToBeReset)
				{
					testConfigForAFileHavingTestCases[ConfigProperty.ReportingLocation.ToString()] = string.Empty;
				}
				bReportingLocationIsToBeReset = true;
				this.TestingConfigurations.Add(testConfigForAFileHavingTestCases);

				if (firstTestConfigForAFileHavingTestCases == null)
				{
					firstTestConfigForAFileHavingTestCases = testConfigForAFileHavingTestCases;
					firstTestConfigForAFileHavingTestCases[ConfigProperty.SemicolonSeparatedConfigResultsToBeMergedInto.ToString()] += ";";
					_dic_SemicolonSeparatedConfigResultsToBeMergedInto_Fields_Replacements.Add(firstTestConfigForAFileHavingTestCases.OriginalID + ";",
                                                                                                firstTestConfigForAFileHavingTestCases.ID + ";");
				}
				else
				{
					firstTestConfigForAFileHavingTestCases[ConfigProperty.SemicolonSeparatedConfigResultsToBeMergedInto.ToString()] += (testConfigForAFileHavingTestCases.ID + ";");
					testConfigForAFileHavingTestCases[ConfigProperty.SemicolonSeparatedConfigResultsToBeMergedInto.ToString()] = "";
					testConfigForAFileHavingTestCases["IsFirstConfig"] = false;
				}
			}
		}

		private IEnumerable<string> GetSemicolonSeparatedAssembliesHavingTestCases(TestConfig tempTestConfig)
		{
            string strXmlHavingPathsOfAssemblies;
            XmlDocument xmlDoc = new XmlDocument();
            List<string> lstAssembliesHavingTestCases = new List<string>();
            string strTestingProjectLocation = tempTestConfig.TestingProjectLocation.GetFullPath(Agent.InputOutputUtil);

            bool bLogMessage = false;
            if (File.Exists(strTestingProjectLocation))
            {
                strXmlHavingPathsOfAssemblies = strTestingProjectLocation;
                xmlDoc.Load(strXmlHavingPathsOfAssemblies);
            }
            else if (Directory.Exists(strTestingProjectLocation))
            {
                bLogMessage = true;
                Agent.LogUtil.LogMessage(string.Format("Fetching all the files having test cases for the testing configuration {0}...",
                                            tempTestConfig.Identifier));

                strXmlHavingPathsOfAssemblies = Path.Combine(strTestingProjectLocation, CONSTANTS.FileHavingTestAssemblies);

                DeleteXmlFileHavingPathsOfAssembliesIfItContainsInvalidOrNoPath(strXmlHavingPathsOfAssemblies);
                
                if (!File.Exists(strXmlHavingPathsOfAssemblies))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    List<ResourceSection> lstResourcesAllocated;
                    var arrPathWithParameters = tempTestConfig.GetExecutableAlongWithCommandLineParametersOfAGivenProperty(ConfigProperty.TestCasesExtractor,
                                                                                                                            tempTestConfig.ExecutionLocation,
                                                                                                                            this.Agent,
                                                                                                                            out lstResourcesAllocated);
                    psi.FileName = arrPathWithParameters[0];
                    psi.Arguments = "\"" + strXmlHavingPathsOfAssemblies + "\" ";

                    psi.RedirectStandardInput = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;

                    var process = Process.Start(psi);
                    ChildProcessTracker.AddProcess(process);
                    process.WaitForExit();
                }
                xmlDoc.Load(strXmlHavingPathsOfAssemblies);
                //File.Delete(strXmlHavingPathsOfAssemblies);
            }
            else
            {
                throw new InvalidConfigurationException(string.Format("{0}.{1} is not a valid path",
                                                                        tempTestConfig.Identifier,
                                                                        ConfigProperty.TestingProjectLocation));
            }

            tempTestConfig[ConfigProperty.SemicolonSeparatedFilesHavingTestCases.ToString()] = "";
            try
            {
                foreach (XmlNode testAssemblyNode in xmlDoc.GetElementsByTagName("TestAssembly"))
                {
                    string strAssemblyHavingTestCases = testAssemblyNode.Attributes["Path"].Value;
                    if (!File.Exists(strAssemblyHavingTestCases))
                    {
                        strAssemblyHavingTestCases = strAssemblyHavingTestCases.GetFullPath(Agent.InputOutputUtil);
                    }

                    if (!File.Exists(strAssemblyHavingTestCases))
                    {
                        throw new InvalidConfigurationException(string.Format("The path \"{0}\" specified in \"{1}\" (the value of {2}.{3}) is not a valid path",
                                                                strAssemblyHavingTestCases,
                                                                strXmlHavingPathsOfAssemblies,
                                                                tempTestConfig.Identifier,
                                                                ConfigProperty.TestingProjectLocation));
                    }
                    else
                    {
                        strAssemblyHavingTestCases = Path.GetFullPath(strAssemblyHavingTestCases);
                        if (bLogMessage)
                        {
                            Agent.LogUtil.LogStatus("Found a file having test cases: " + strAssemblyHavingTestCases);
                        }
                    }

                    strAssemblyHavingTestCases = strAssemblyHavingTestCases.Replace(strTestingProjectLocation, "").Trim('\\');
                    lstAssembliesHavingTestCases.Add(strAssemblyHavingTestCases);
                    tempTestConfig[ConfigProperty.SemicolonSeparatedFilesHavingTestCases.ToString()] =
                    tempTestConfig[ConfigProperty.SemicolonSeparatedFilesHavingTestCases.ToString()] + strAssemblyHavingTestCases + ";";
                }
            }
            catch (Exception exp)
            {
                Agent.LogUtil.LogError($"Either no test assembly is specified in \"{strXmlHavingPathsOfAssemblies}\" or the format of the file is not correct");
                throw exp;
            }

            return lstAssembliesHavingTestCases;
		}

        private void DeleteXmlFileHavingPathsOfAssembliesIfItContainsInvalidOrNoPath(string strXmlHavingPathsOfAssemblies)
        {
            if (File.Exists(strXmlHavingPathsOfAssemblies))
            {
                XmlDocument xmlDocTemp = new XmlDocument();
                xmlDocTemp.Load(strXmlHavingPathsOfAssemblies);
                bool bSomeAssemblyWasNotFound = true;
                foreach (XmlNode testAssemblyNode in xmlDocTemp.GetElementsByTagName("TestAssembly"))
                {
                    bSomeAssemblyWasNotFound = false;
                    string strAssemblyHavingTestCases = testAssemblyNode.Attributes["Path"].Value;
                    if (!File.Exists(strAssemblyHavingTestCases))
                    {
                        strAssemblyHavingTestCases = strAssemblyHavingTestCases.GetFullPath(Agent.InputOutputUtil);
                    }

                    if (!File.Exists(strAssemblyHavingTestCases))
                    {
                        bSomeAssemblyWasNotFound = true;
                        break;
                    }
                }
                if (bSomeAssemblyWasNotFound)
                {
                    File.Delete(strXmlHavingPathsOfAssemblies);
                }
            }
        }

        private TestConfig GetNewTestConfigFromAnExistingTestConfigForAFileHavingTestCases(TestConfig testConfig, string strFileHavingTestCases)
        {
            TestConfig newTestConfig = testConfig.Clone();
            string strNewIDPostFix = newTestConfig.ID + "-" + strFileHavingTestCases;
            string strTestingProjectLocation = newTestConfig[ConfigProperty.TestingProjectLocation.ToString()].ToString().Trim('/').Trim('\\');
            if (!string.IsNullOrEmpty(strTestingProjectLocation))
            {
                int iIndex = strNewIDPostFix.ToLower().IndexOf(strTestingProjectLocation.ToLower());
                if (iIndex != -1)
                {
                    strNewIDPostFix = strNewIDPostFix.Remove(iIndex, strTestingProjectLocation.Length);
                }
            }
            strNewIDPostFix = strNewIDPostFix.Replace(" ", "-").Replace(":", "-").Replace(" ", "-").Replace("\\", "-")
                                .Replace("/", "-").Replace(".dll", "").Replace(".jar", "").Replace(".class", "")
                                .Replace("--", "-").Replace("--", "-");

            newTestConfig[ConfigProperty.ID.ToString()] = strNewIDPostFix;

            newTestConfig[ConfigProperty.TestingProjectLocation.ToString()] = newTestConfig.TestingProjectLocation.GetFullPath(Agent.InputOutputUtil);
            if (File.Exists(strFileHavingTestCases) && !strFileHavingTestCases.Trim('\\').ToLower().
                Contains(newTestConfig[ConfigProperty.TestingProjectLocation.ToString()].ToString().Trim('\\').ToLower()))
            {
                newTestConfig[ConfigProperty.TestingProjectLocation.ToString()] = Path.GetDirectoryName(strFileHavingTestCases);
                newTestConfig[ConfigProperty.ID.ToString()] = testConfig.ID + "-" + Path.GetFileNameWithoutExtension(strFileHavingTestCases);
            }
            newTestConfig[ConfigProperty.SemicolonSeparatedFilesHavingTestCases.ToString()] = strFileHavingTestCases;

            return newTestConfig;
        }

		private void Update_SemicolonSeparatedConfigResultsToBeMergedInto_FieldInAllParallelConfigurations()
		{
			Agent.LogUtil.LogMessage(string.Format("Updating \"{0}\" field in all testing configurations...",
                                                    ConfigProperty.SemicolonSeparatedConfigResultsToBeMergedInto));

			foreach (var testConfig in this.TestingConfigurations)
			{
				if (!string.IsNullOrEmpty(testConfig.SemicolonSeparatedConfigResultsToBeMergedInto))
				{
					foreach (var keyValPair in _dic_SemicolonSeparatedConfigResultsToBeMergedInto_Fields_Replacements)
					{
						testConfig[ConfigProperty.SemicolonSeparatedConfigResultsToBeMergedInto.ToString()] =
							 testConfig.SemicolonSeparatedConfigResultsToBeMergedInto.Replace(keyValPair.Key, keyValPair.Value);
					}

					testConfig[ConfigProperty.SemicolonSeparatedConfigResultsToBeMergedInto.ToString()] =
						testConfig.SemicolonSeparatedConfigResultsToBeMergedInto.Replace(";;", ";").Trim(';');
				}
			}

			// Making it null because it is not required any more
			_dic_SemicolonSeparatedConfigResultsToBeMergedInto_Fields_Replacements = null;
		}

		private void UpdateProcessWideConfig()
		{
			int iTotalThreadCount = 0;
			this.TestingConfigurations.ForEach(testConfig => iTotalThreadCount += testConfig.ThreadCount);
			
			this.ProcessWideConfig[ConfigProperty.MaxThreads.ToString()] = Math.Min(this.ProcessWideConfig.MaxThreads, iTotalThreadCount);
		}

		private void ValidateAllConfigurations()
		{
			StringBuilder sbConfigRelatedErrors = new StringBuilder();
			if (this.ProcessWideConfig.MaxThreads < 1)
			{
				sbConfigRelatedErrors.AppendLine();
				sbConfigRelatedErrors.AppendLine(string.Format("{0}.{1} should be greater than equal to 1",
                                                                this.ProcessWideConfig.Identifier,
                                                                ConfigProperty.MaxThreads));
			}

			this.CheckPathConfiguration(this.ProcessWideConfig, ConfigProperty.BeforeTestExecution, false, sbConfigRelatedErrors);

			this.CheckPathConfiguration(this.ProcessWideConfig, ConfigProperty.AfterTestExecution, false, sbConfigRelatedErrors);

			List<string> lstID = new List<string>();
			foreach (var testConfig in this.TestingConfigurations)
			{
                //if (string.IsNullOrEmpty(testConfig.TestingFramework))
                //{
                //	sbConfiguration.AppendLine();
                //	sbConfiguration.AppendLine(string.Format("{0}.{1} is not specified. Make sure you specify the correct testing-framework",
                //												testConfig.Identifier, ConfigProperty.TestingFramework));
                //}

                if (testConfig.ThreadCount < 1)
				{
					sbConfigRelatedErrors.AppendLine();
					sbConfigRelatedErrors.AppendLine(string.Format("{0}.{1} should be greater than equal to 1",
                                                        testConfig.Identifier,
                                                        ConfigProperty.ThreadCount));
				}

                string strFileHavingTestCases = testConfig.SemicolonSeparatedFilesHavingTestCases.FirstFile();
                if (!File.Exists(strFileHavingTestCases) && !File.Exists(Path.Combine(testConfig.TestingProjectLocation, strFileHavingTestCases)))
                {
                    sbConfigRelatedErrors.AppendLine();
                    sbConfigRelatedErrors.AppendLine(string.Format("Path of at least one assembly in {0}.{1}" +
                                                                    " or the path of {0}.{2} is not correct",
                                                                    testConfig.Identifier,
                                                                    ConfigProperty.SemicolonSeparatedFilesHavingTestCases,
                                                                    ConfigProperty.TestingProjectLocation));
                }

				this.CheckPathConfiguration(testConfig, ConfigProperty.WorkingDirectoryOfTestRunner, false, sbConfigRelatedErrors);

				this.CheckPathConfiguration(testConfig, ConfigProperty.TestingProjectLocation, false, sbConfigRelatedErrors);

				if (!string.IsNullOrEmpty(testConfig.TestCategories) && !string.IsNullOrEmpty(testConfig.SemicolonSeparatedTestCases))
				{
					sbConfigRelatedErrors.AppendLine();
					sbConfigRelatedErrors.AppendLine(string.Format("Testing configuration {0} contains both {1} and {2}. " +
                                                                    "It can contain either {1} or {2} but not both",
																	testConfig.Identifier,
                                                                    ConfigProperty.TestCategories,
                                                                    ConfigProperty.SemicolonSeparatedTestCases));
				}

				if (!string.IsNullOrEmpty(testConfig.TestClasses) && !string.IsNullOrEmpty(testConfig.SemicolonSeparatedTestCases))
				{
					sbConfigRelatedErrors.AppendLine();
					sbConfigRelatedErrors.AppendLine(string.Format("Testing configuration {0} contains both {1} and {2}. " +
                                                                    "It can contain either {1} or {2} but not both",
                                                                    testConfig.Identifier,
                                                                    ConfigProperty.TestClasses,
                                                                    ConfigProperty.SemicolonSeparatedTestCases));
				}

				if (testConfig.TestCategories.Contains("|") && testConfig.TestCategories.Contains("^"))
				{
					sbConfigRelatedErrors.AppendLine();
					sbConfigRelatedErrors.AppendLine(string.Format("{0}.{1} contains both ^ (AND operator) and | (OR operator). " +
                                                                    "It can contain either ^ or | but not both",
                                                                    testConfig.Identifier, ConfigProperty.TestCategories));
				}

				if (lstID.Contains(testConfig.ID))
				{
					sbConfigRelatedErrors.AppendLine();
					sbConfigRelatedErrors.AppendLine(string.Format(("Testing configuration {0} is generating duplicate testing configurations. " +
                                                                    "Make sure {1}s (configuration.TestingConfigurations.Config.ID) of all " +
                                                                    "testing configurations must be unique. Also no assembly should be repeated in " +
                                                                    "configuration.TestingConfigurations.Config.{2} " +
                                                                    " in any test configuration"),
                                                                    testConfig.Identifier,
                                                                    ConfigProperty.ID,
                                                                    ConfigProperty.SemicolonSeparatedFilesHavingTestCases));
				}
				lstID.Add(testConfig.ID);

				if (testConfig.ConcurrentUnit.ConcurentUnitType() == ConcurentUnitType.Undefined)
				{
					sbConfigRelatedErrors.AppendLine();
					sbConfigRelatedErrors.AppendLine(string.Format(("Invalid {0}.{1}." +
                                                                    "{1} type can either be 1 (for executing Test-Classes in parallel) " +
                                                                    "or 2 (for executing Test Cases in parallel)"),
                                                                    testConfig.Identifier,
                                                                    ConfigProperty.ConcurrentUnit));
				}
			}

			if (!string.IsNullOrEmpty(sbConfigRelatedErrors.ToString()))
			{
				throw new InvalidConfigurationException(sbConfigRelatedErrors.ToString());
			}
		}

		private void CheckPathConfiguration(BaseConfig baseConfig, ConfigProperty configProperty, bool bRequired, StringBuilder sbErrorMessage)
		{
			string strProperty = configProperty.ToString();
			string strPath = baseConfig[strProperty].ToString();

			if ((bRequired && string.IsNullOrEmpty(strPath)) ||
				(!string.IsNullOrEmpty(strPath) && !File.Exists(strPath) && !Directory.Exists(strPath)))
			{
				sbErrorMessage.AppendLine(string.Format("{0}.{1} is not a valid path", baseConfig.Identifier, strProperty));
			}
		}

		private string _testExecutionFolderName;
		public string TestExecutionFolderName
		{
			get
			{
				if (string.IsNullOrEmpty(_testExecutionFolderName))
				{
					_testExecutionFolderName = "PTR_" + CONSTANTS.TimeStampWhenPTRWasLaunched;
				}
				return _testExecutionFolderName;
			}
		}

		private void CopyRequiredComponentsForAllEnabledConfigurationsAtTheirCorrespondingExecutionLocation()
		{
			foreach (var testConfig in this.TestingConfigurations)
			{
				string strExecutionLocation = testConfig.ExecutionLocation.GetFullPath(this.Agent.InputOutputUtil);
				this.Agent.InputOutputUtil.CreateDirectoryIfDoesNotExist(strExecutionLocation);

				strExecutionLocation = Path.Combine(strExecutionLocation, TestExecutionFolderName);
				this.Agent.InputOutputUtil.CreateDirectoryIfDoesNotExist(strExecutionLocation);
				string strTestExecutionFolderPath = strExecutionLocation;
                strExecutionLocation = Path.Combine(strExecutionLocation, testConfig.ID);
				this.Agent.InputOutputUtil.CreateDirectoryIfDoesNotExist(strExecutionLocation);
				testConfig[ConfigProperty.ExecutionLocation.ToString()] = strExecutionLocation;

				// To run from execution location rather than actual location of testing project
				if (!testConfig.LoadTestingProjectBinariesFromItsOwnLocationOnly.ToBool())
				{
					this.Agent.InputOutputUtil.CopyDirectoryWithAllItsContent(testConfig.TestingProjectLocation,
																				strExecutionLocation,
																				new List<string>() { strTestExecutionFolderPath });
				}
			}
		}
	}
}
