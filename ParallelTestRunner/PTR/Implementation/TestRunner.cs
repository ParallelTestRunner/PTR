using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class TestRunner : ITestRunner
	{
		IObjectFactory Agent;
		public TestRunner(IObjectFactory Factory)
		{
			this.Agent = Factory;
		}

		Dictionary<string, ProcessStartInfo> _processStartInfoOfConfigEditorByCallerId = new Dictionary<string, ProcessStartInfo>();
		Dictionary<string, List<ResourceSection>> _resourcesAllocatedByCallerId = new Dictionary<string, List<ResourceSection>>();

		List<string> _lstFilesToBeRestored = new List<string>();
		
		public void Run(string strCallerId,
						TestConfig testConfig,
						IEnumerable<string> testCaseGroup,
						string strExecutionLocation,
						string strResultFile,
						bool bRunningFailedTestCase = false,
						int iNumberOfTimesRunningFailedTestCases = -1)
		{
			if (testCaseGroup == null || testCaseGroup.Count() <= 0)
			{
				this.Agent.LogUtil.LogWarning("Empty test case list found in test runner...");
				return;
			}

			///////////////////////////
			if ((new Random().Next(1, 21) % 21) == 0)
			{
				if (!(new LicenseChecker()).DoesThisMachineHaveAValidLicense())
				{
					return;
				}
			}
			///////////////////////////

			List<ResourceSection> lstResourcesAllocated;
			var arrTestRunnerPath = testConfig.GetExecutableAlongWithCommandLineParametersOfAGivenProperty(ConfigProperty.TestRunner,
																											testConfig.ExecutionLocation,
																											this.Agent,
																											out lstResourcesAllocated);
			ProcessStartInfo psiTestRunner = new ProcessStartInfo();
			psiTestRunner.FileName = "cmd.exe";

			if (testConfig.MakeTestRunnerAsChildProcessOfPTR.ToBool())
			{
				psiTestRunner.RedirectStandardInput = true;
				psiTestRunner.UseShellExecute = false;
				psiTestRunner.CreateNoWindow = true;
			}

			Process processTestRunner = null;
			object objLocker = testConfig;
			if (testConfig.LoadTestingProjectBinariesFromItsOwnLocationOnly.ToBool())
			{
				objLocker = this.Agent;
			}

			lock (objLocker)
			{
				string strTitle = testConfig.GetTitleForCurrentThread(strExecutionLocation, bRunningFailedTestCase, iNumberOfTimesRunningFailedTestCases);
				string strTestContainer = Path.Combine(testConfig.TestingProjectLocation, testConfig.SemicolonSeparatedFilesHavingTestCases.FirstFile());
				psiTestRunner.Arguments = GetCommandLineParameterStringForTestCases(arrTestRunnerPath[0],
                                                                                    strTitle,
																					strTestContainer,
																					strResultFile,
																					testCaseGroup);
				
				if (!string.IsNullOrEmpty(testConfig.WorkingDirectoryOfTestRunner))
				{
					psiTestRunner.WorkingDirectory = testConfig.WorkingDirectoryOfTestRunner;
				}

				ConfigProperty configProperty = bRunningFailedTestCase ? ConfigProperty.BeforeRerunConfigEditor : ConfigProperty.BeforeRunConfigEditor;

				if (!_processStartInfoOfConfigEditorByCallerId.ContainsKey(strCallerId))
				{
					CacheProcessStartInfoOfConfigEditorForCurrentCaller(strCallerId, testConfig, configProperty);
				}

				var psi = _processStartInfoOfConfigEditorByCallerId[strCallerId];
				if (psi != null)
				{
					//this.Agent.LogUtil.LogMessage("Testing project's configuration is being changed...");
					foreach (string strFileToBeRestored in _lstFilesToBeRestored)
					{
						string strBackupOfFileToEdit = GenerateBackupFileName(strFileToBeRestored);
						if (!File.Exists(strBackupOfFileToEdit))
						{
							File.Copy(strFileToBeRestored, strBackupOfFileToEdit, true);
						}
						else
						{
							File.Copy(strBackupOfFileToEdit, strFileToBeRestored, true);
						}
					}

					//this.Agent.LogUtil.LogMessage("Modifying testing project's configurations...");
					var process = Process.Start(psi);
					//ChildProcessTracker.AddProcess(process);
					process.WaitForExit();
				}

				this.Agent.InputOutputUtil.CreateDirectoryIfDoesNotExist(strExecutionLocation);

                try
                {
                    this.Agent.LogUtil.LogStatus("Test cases are being executed by following command:");
                    StringBuilder sbStatus = new StringBuilder(psiTestRunner.Arguments);
                    sbStatus.AppendLine();
                    this.Agent.LogUtil.LogMessage(sbStatus.ToString());
                }
                catch
                { }

                psiTestRunner.ErrorDialog = true;
				processTestRunner = Process.Start(psiTestRunner);
				if (testConfig.MakeTestRunnerAsChildProcessOfPTR.ToBool())
				{
					ChildProcessTracker.AddProcess(processTestRunner);
				}

				bool bWaitForSometimeBeforeRestoringAFileButDoThisOnlyOnce = true;
				foreach (string strFileToBeRestored in _lstFilesToBeRestored)
				{
					string strBackupOfFileToEdit = GenerateBackupFileName(strFileToBeRestored);
					if (File.Exists(strBackupOfFileToEdit))
					{
						if (bWaitForSometimeBeforeRestoringAFileButDoThisOnlyOnce)
						{
							bWaitForSometimeBeforeRestoringAFileButDoThisOnlyOnce = false;
							//this.Agent.LogUtil.LogMessage("Testing project's configuration is being reset...");
							System.Threading.Thread.Sleep(5000);
						}
						File.Copy(strBackupOfFileToEdit, strFileToBeRestored, true);
						File.Delete(strBackupOfFileToEdit);
					}
				}
			}

			processTestRunner.WaitForExit();
			//this.Agent.LogUtil.LogWarning("Exit Code: " + processTestRunner.ExitCode);
		}

		private string GetCommandLineParameterStringForTestCases(string strTestRunnerBatFile,
                                                                    string strTitle,
                                                                    string strTestContainer,
                                                                    string strResultFile,
                                                                    IEnumerable<string> testCaseGroup)
		{
			StringBuilder sbTestRunnerCommand;
			if (string.IsNullOrEmpty(_strTestRunnerCommandPattern))
			{
				sbTestRunnerCommand = new StringBuilder(File.ReadAllLines(strTestRunnerBatFile)[0]);

				_strTestCasePattern = sbTestRunnerCommand.ToString();
				_strTestCasePattern = _strTestCasePattern.Substring(_strTestCasePattern.IndexOf("%test-case-switch%"));
				sbTestRunnerCommand = sbTestRunnerCommand.Replace(_strTestCasePattern, "");

				_strTestCasePattern = _strTestCasePattern.Replace("%test-case-switch%", "");

				_strTestRunnerCommandPattern = sbTestRunnerCommand.ToString();
			}
			else
			{
				sbTestRunnerCommand = new StringBuilder(_strTestRunnerCommandPattern);
			}

			sbTestRunnerCommand = sbTestRunnerCommand.Replace("%title%", strTitle);
			sbTestRunnerCommand = sbTestRunnerCommand.Replace("%test-container%", "\"" + strTestContainer + "\"");
			sbTestRunnerCommand = sbTestRunnerCommand.Replace("%result-file%", "\"" + strResultFile + "\"");
			foreach (string strTestCase in testCaseGroup)
			{
				sbTestRunnerCommand.Append(_strTestCasePattern.Replace("%test-case%", GetStringAfterProcessingDoubleQuotsAndSingleQuotes(strTestCase)) + " ");
			}

            sbTestRunnerCommand.Append("& exit");
            return sbTestRunnerCommand.ToString().Trim().Trim(',');
		}

		private string GetStringAfterProcessingDoubleQuotsAndSingleQuotes(string str)
		{
			return str.Replace("\\\"", "\\\"\"").Replace("\"", "\\\"").Replace("\\\'", "\\\'\'").Replace("\'", "\\\'");
		}

		private string _strTestRunnerCommandPattern;
		private string _strTestCasePattern;

		private static string GenerateBackupFileName(string strFileToBeRestored)
		{
			return strFileToBeRestored.Replace(Path.GetFileName(strFileToBeRestored), Path.GetFileNameWithoutExtension(strFileToBeRestored) + "_BK");
		}

		private void CacheProcessStartInfoOfConfigEditorForCurrentCaller(string strCallerId, TestConfig testConfig, ConfigProperty configProperty)
		{
			List<ResourceSection> lstResourcesAllocated;
			var arrPathWithParameters = testConfig.GetExecutableAlongWithCommandLineParametersOfAGivenProperty(configProperty,
                                                                                                                testConfig.ExecutionLocation,
																												this.Agent,
																												out lstResourcesAllocated);
			if (lstResourcesAllocated.Count > 0)
			{
                lstResourcesAllocated.ForEach(x => this.Agent.LogUtil.LogMessage($"Shared resource allocated to Thread {strCallerId}: {x.ID}"));
                _resourcesAllocatedByCallerId.Add(strCallerId, lstResourcesAllocated);
			}

			ProcessStartInfo psi = null;
			if (arrPathWithParameters != null)
			{
				psi = new ProcessStartInfo();
				psi.RedirectStandardInput = true;
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;
				psi.FileName = arrPathWithParameters[0];
				for (int i = 1; i < arrPathWithParameters.Length; i++)
				{
					if (string.IsNullOrEmpty(psi.Arguments))
					{
						psi.Arguments = "\"" + arrPathWithParameters[i] + "\"";
					}
					else
					{
						psi.Arguments += " \"" + arrPathWithParameters[i] + "\"";
					}
				}

				if (testConfig.LoadTestingProjectBinariesFromItsOwnLocationOnly.ToBool() && !_lstFilesToBeRestored.Contains(arrPathWithParameters[2]))
				{
					_lstFilesToBeRestored.Add(arrPathWithParameters[2]);
				}
			}

			_processStartInfoOfConfigEditorByCallerId.Add(strCallerId, psi);
		}

		public void AllTestCasesInAThreadAreExecuted(string strCallerId)
		{
			if (_resourcesAllocatedByCallerId.ContainsKey(strCallerId))
			{
				foreach(var resource in _resourcesAllocatedByCallerId[strCallerId])
				{
                    this.Agent.LogUtil.LogMessage($"Shared resource deallocated to Thread {strCallerId}: {resource.ID}");
                    SharedResourcesUtil.ResourceUsageCompleted(resource);
				}
			}
		}
	}
}
