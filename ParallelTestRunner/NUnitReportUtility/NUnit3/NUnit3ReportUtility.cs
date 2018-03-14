using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using NUnitReportUtility;

namespace NUnitReportUtilityNunit3
{
	class NUnit3ReportUtility : IReportUtil
	{
		List<ResultAttributeValues> _lstOutcomePrioritiesForTestCasesWithMultipleEntries;

		public NUnit3ReportUtility()
		{
			_lstOutcomePrioritiesForTestCasesWithMultipleEntries = null;
		}

		public NUnit3ReportUtility(List<ResultAttributeValues> lstOutcomePriority)
		{
			_lstOutcomePrioritiesForTestCasesWithMultipleEntries = lstOutcomePriority;
		}

		public void Consolidate(string strConsolidatedReport, string strReportToBeMerged)
		{
			if (!File.Exists(strConsolidatedReport))
			{
				File.Copy(strReportToBeMerged, strConsolidatedReport);
				return;
			}

			XmlDocument xmlDocConsolidatedReport = new XmlDocument();
			xmlDocConsolidatedReport.Load(strConsolidatedReport);

			XmlDocument xmlDocReportToBeMerged = new XmlDocument();
			xmlDocReportToBeMerged.Load(strReportToBeMerged);

			ConsolidateTestRunNode(xmlDocConsolidatedReport, xmlDocReportToBeMerged);

			CreateTestSuiteOfProjectTypeIfNotExists(xmlDocConsolidatedReport);

			CreateTestSuiteOfProjectTypeIfNotExists(xmlDocReportToBeMerged);

			ConsolidateTestSuitesOfAllTypes(xmlDocConsolidatedReport, xmlDocReportToBeMerged);

			UpdateTestRunNode(xmlDocConsolidatedReport);

			RemoveTestSuiteOfType_TestProject(xmlDocConsolidatedReport);

			xmlDocConsolidatedReport.Save(strConsolidatedReport);
		}

		private void CreateTestSuiteOfProjectTypeIfNotExists(XmlDocument xmlDoc)
		{
			XmlNode xmlNodeOfTestSuiteOfTypeProjectType = null;
			string xpathOfTestSuiteOfTypeProjectType = GetXPathOfTestSuiteOfType_TestProject();
			var rootTestSuiteNode = xmlDoc.DocumentElement.SelectNodes(xpathOfTestSuiteOfTypeProjectType);
			if (rootTestSuiteNode.Count <= 0)
			{
				xmlNodeOfTestSuiteOfTypeProjectType = xmlDoc.CreateNode(XmlNodeType.Element, NodeNames.testSuite.String(), "");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.type.String(), TestSuiteTypes.TestProject.String());
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.id.String(), "temp");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.fullname.String(), "temp");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.testcasecount.String(), "0");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.result.String(), ResultAttributeValues.Passed.String());
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.total.String(), "0");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.passed.String(), "0");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.failed.String(), "0");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.inconclusive.String(), "0");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.skipped.String(), "0");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.asserts.String(), "0");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.duration.String(), "0.0");

				var testRun = xmlDoc.GetElementsByTagName(NodeNames.testRun.String())[0];
				testRun.AppendChild(xmlNodeOfTestSuiteOfTypeProjectType);
				var testSuiteNodes = testRun.ChildNodes.Shim<XmlNode>().ToList();
				foreach (var node in testSuiteNodes)
				{
					if (node == xmlNodeOfTestSuiteOfTypeProjectType || node.Name != NodeNames.testSuite.String())
					{
						continue;
					}

					node.ParentNode.RemoveChild(node);
					xmlNodeOfTestSuiteOfTypeProjectType.AppendChild(node);
				}
			}
		}

		private void ConsolidateTestRunNode(XmlDocument xmlDocConsolidatedReport, XmlDocument xmlDocReportToBeMerged)
		{
			string strTestRunNode = NodeNames.testRun.String();
			var TestRunOfConsolidatedReport = xmlDocConsolidatedReport.GetElementsByTagName(strTestRunNode)[0];
			var TestRunOfReportToBeMerged = xmlDocReportToBeMerged.GetElementsByTagName(strTestRunNode)[0];
			string strNewValue;
			foreach (TestRunAttributes testRunAttribute in Enum.GetValues(typeof(TestRunAttributes)))
			{
				if (testRunAttribute == TestRunAttributes.result)
				{
					continue;
				}

				string strTestRunAttribute = testRunAttribute.String();
				if (testRunAttribute == TestRunAttributes.startTime || testRunAttribute == TestRunAttributes.endTime)
				{
					strNewValue = GetStartOrEndTimeByItsOccurenceFromNodesBeingConsolidated(TestRunOfConsolidatedReport,
																													TestRunOfReportToBeMerged,
																													testRunAttribute.String());
				}
				else
				{
					strNewValue = GetAddedAttributeValues(TestRunOfConsolidatedReport,
																		TestRunOfReportToBeMerged,
																		strTestRunAttribute,
																		testRunAttribute == TestRunAttributes.duration);
				}

				TestRunOfConsolidatedReport.SetAttributeValue(strTestRunAttribute, strNewValue);
			}
		}

		private string GetStartOrEndTimeByItsOccurenceFromNodesBeingConsolidated(XmlNode NodeBeingConsolidated,
																											XmlNode NodeBeingMerged,
																											string strAttributeNameContainingStartOrEndTime)
		{
			string strDateTimeOfNodeBeingConsolidated = NodeBeingConsolidated.GetAttributeValue(strAttributeNameContainingStartOrEndTime);
			string strDateTimeOfNodeBeingMerged = NodeBeingMerged.GetAttributeValue(strAttributeNameContainingStartOrEndTime);

			return GetTimeByItsOccurrence(strDateTimeOfNodeBeingConsolidated,
																strDateTimeOfNodeBeingMerged,
																strAttributeNameContainingStartOrEndTime != TestRunAttributes.endTime.String());
		}

		private string GetTimeByItsOccurrence(string strDateTime1, string strDateTime2, bool bGetFormerTime = true)
		{
			if (string.IsNullOrEmpty(strDateTime1) && string.IsNullOrEmpty(strDateTime2))
			{
				return string.Empty;
			}

			if ((string.IsNullOrEmpty(strDateTime1) && !string.IsNullOrEmpty(strDateTime2)) ||
					(string.IsNullOrEmpty(strDateTime2) && !string.IsNullOrEmpty(strDateTime1)))
			{
				return string.IsNullOrEmpty(strDateTime1) ? strDateTime2 : strDateTime1;
			}

			if (DateTime.Parse(strDateTime1).CompareTo(DateTime.Parse(strDateTime2)) <= 0)
			{
				return bGetFormerTime ? strDateTime1 : strDateTime2;
			}

			return bGetFormerTime ? strDateTime2 : strDateTime1;
		}

		private string GetAddedAttributeValues(XmlNode xmlNode1, XmlNode xmlNode2, string strAttributeName, bool bAttributeValueIsOfDecimalType = false)
		{
			string strValue1 = xmlNode1.GetAttributeValue(strAttributeName);
			string strValue2 = xmlNode2.GetAttributeValue(strAttributeName);

			if ((string.IsNullOrEmpty(strValue1) && !string.IsNullOrEmpty(strValue2)) ||
					(string.IsNullOrEmpty(strValue2) && !string.IsNullOrEmpty(strValue1)))
			{
				return string.IsNullOrEmpty(strValue1) ? strValue2 : strValue1;
			}

			if (bAttributeValueIsOfDecimalType)
			{
				Decimal dValue1 = 0;
				Decimal dValue2 = 0;
				if (Decimal.TryParse(strValue1, out dValue1) && Decimal.TryParse(strValue2, out dValue2))
				{
					return (dValue1 + dValue2).ToString();
				}
			}
			else
			{
				Int32 iValue1 = 0;
				Int32 iValue2 = 0;
				if (Int32.TryParse(strValue1, out iValue1) && Int32.TryParse(strValue2, out iValue2))
				{
					return (iValue1 + iValue2).ToString();
				}
			}

			return strValue1 + strValue2;
		}

		private void ConsolidateTestSuitesOfAllTypes(XmlDocument xmlDocConsolidatedReport, XmlDocument xmlDocReportToBeMerged)
		{
			string xpathOfTestSuiteOfType_TestProject = GetXPathOfTestSuiteOfType_TestProject();
			var testSuiteNodeOfConsolidatedReport = xmlDocConsolidatedReport.DocumentElement.SelectNodes(xpathOfTestSuiteOfType_TestProject).Item(0);
			var testSuiteNodeOfReportToBeMerged = xmlDocReportToBeMerged.DocumentElement.SelectNodes(xpathOfTestSuiteOfType_TestProject).Item(0);

			ConsolidateChildTestSuite(testSuiteNodeOfConsolidatedReport, testSuiteNodeOfReportToBeMerged);

			UpdateAllAttributesOfTestObject(testSuiteNodeOfConsolidatedReport);
		}

		private string GetXPathOfTestSuiteOfType_TestProject()
		{
			string strXPath = "//" + NodeNames.testRun.String() + "/" +
									NodeNames.testSuite.String() +
									"[@" + TestSuiteAttributes.type.String() + "='{0}']";

			return string.Format(strXPath, TestSuiteTypes.TestProject.String());
		}

		Dictionary<XmlNode, bool> _dicProcessedTestSuitesOfConsolidatedReport = new Dictionary<XmlNode, bool>();
		private void ConsolidateChildTestSuite(XmlNode testSuiteParentInConsolidatedReport, XmlNode testSuiteParentInReportToBeMerged)
		{
			var nodesOfReportToBeMerged = testSuiteParentInReportToBeMerged.ChildNodes.Shim<XmlNode>().ToList()
														.Where(x => x.Name == NodeNames.testSuite.String() || x.Name == NodeNames.testCase.String());

			var nodesOfConsolidatedReport = testSuiteParentInConsolidatedReport.ChildNodes.Shim<XmlNode>().ToList()
															.Where(x => x.Name == NodeNames.testSuite.String() || x.Name == NodeNames.testCase.String());

			foreach (XmlNode nodeOfReportToBeMerged in nodesOfReportToBeMerged)
			{
				XmlNode correspondingNodeOfConsolidatedReport = null;
				foreach (XmlNode nodeOfConsolidatedReport in nodesOfConsolidatedReport)
				{
					if (!_dicProcessedTestSuitesOfConsolidatedReport.ContainsKey(nodeOfConsolidatedReport) &&
						 AreTheySimilarTestObject(nodeOfReportToBeMerged, nodeOfConsolidatedReport))
					{
						correspondingNodeOfConsolidatedReport = nodeOfConsolidatedReport;
						break;
					}
				}

				if (correspondingNodeOfConsolidatedReport == null)
				{
					testSuiteParentInConsolidatedReport.AppendChild(testSuiteParentInConsolidatedReport.OwnerDocument.ImportNode(nodeOfReportToBeMerged, true));
				}
				else
				{
					ConsolidateTestObjects(correspondingNodeOfConsolidatedReport, nodeOfReportToBeMerged);
					_dicProcessedTestSuitesOfConsolidatedReport.Add(correspondingNodeOfConsolidatedReport, true);
				}
			}
		}

		private void ConsolidateTestObjects(XmlNode nodeOfConsolidatedReport, XmlNode nodeOfReportToBeMerged)
		{
			if (nodeOfConsolidatedReport.Name == NodeNames.testCase.String())
			{
				ConsolidateTestCaseResults(nodeOfConsolidatedReport, nodeOfReportToBeMerged);
			}
			else
			{
				string strTime = GetStartOrEndTimeByItsOccurenceFromNodesBeingConsolidated(nodeOfConsolidatedReport,
																													nodeOfReportToBeMerged,
																													TestSuiteAttributes.startTime.String());

				if (!string.IsNullOrEmpty(strTime))
				{
					nodeOfConsolidatedReport.SetAttributeValue(TestSuiteAttributes.startTime.String(), strTime);
				}

				strTime = GetStartOrEndTimeByItsOccurenceFromNodesBeingConsolidated(nodeOfConsolidatedReport,
																											nodeOfReportToBeMerged,
																											TestSuiteAttributes.endTime.String());

				if (!string.IsNullOrEmpty(strTime))
				{
					nodeOfConsolidatedReport.SetAttributeValue(TestSuiteAttributes.endTime.String(), strTime);
				}

				ConsolidateChildTestSuite(nodeOfConsolidatedReport, nodeOfReportToBeMerged);
			}
		}

		List<ResultAttributeValues> resultsAsPerTheirPriorityLevel = new List<ResultAttributeValues>()
																								{
																									ResultAttributeValues.Failed,
																									ResultAttributeValues.Passed,
																									ResultAttributeValues.Inconclusive,
																									ResultAttributeValues.Skipped
																								};
		private int _iTestCaseCount = 0;
		private int _iTotal = 0;
		private int _iTotalPassed = 0;
		private int _iTotalFailed = 0;
		private int _iTotalInconclusive = 0;
		private int _iTotalSkipped = 0;
		private int _iTotalAsserts = 0;
		private double _dTotalDuration = 0.0;
		private void UpdateAllAttributesOfTestObject(XmlNode testObject)
		{
			if (testObject.Name == NodeNames.testCase.String())
			{
				ResultAttributeValues result = GetTestCaseOutcome(testObject);

				_iTestCaseCount++;
				_iTotal++;

				if (result == ResultAttributeValues.Passed)
				{
					_iTotalPassed++;
				}
				else if (result == ResultAttributeValues.Failed)
				{
					_iTotalFailed++;
				}
				else if (result == ResultAttributeValues.Inconclusive)
				{
					_iTotalInconclusive++;
				}
				else if (result == ResultAttributeValues.Skipped)
				{
					_iTotalSkipped++;
				}
				else
				{
					Debug.Assert(true, "Test case result type: " + result.String() + " is not handled");
				}

				int iTempAsserts = 0;
				if (int.TryParse(testObject.GetAttributeValue(TestSuiteAttributes.asserts.String()), out iTempAsserts))
				{
					_iTotalAsserts += iTempAsserts;
				}

				return;
			}

			var childTestObjects = testObject.ChildNodes.Shim<XmlNode>()
														.Where(x => x.Name == NodeNames.testSuite.String() || x.Name == NodeNames.testCase.String());

			bool bUpdateTestSuiteAttributes = false;
			string strRunState = "Ignored";
			string strResult = string.Empty;
			int testCaseCount = 0;
			int total = 0;
			int passed = 0;
			int failed = 0;
			int inconclusive = 0;
			int skipped = 0;
			int asserts = 0;
			double duration = 0.0;
			//bool bHasChildTestSuites = false;

			foreach (XmlNode childTestObject in childTestObjects)
			{
				bUpdateTestSuiteAttributes = true;

				UpdateAllAttributesOfTestObject(childTestObject);

				if (childTestObject.GetAttributeValue(TestSuiteAttributes.runstate.String()) == RunStateAttributes.Runnable.String())
				{
					strRunState = RunStateAttributes.Runnable.String();
				}

				ResultAttributeValues result = GetTestCaseOutcome(childTestObject);
				if (string.IsNullOrEmpty(strResult) ||
						resultsAsPerTheirPriorityLevel.IndexOf(result) <
						resultsAsPerTheirPriorityLevel.IndexOf((ResultAttributeValues)Enum.Parse(typeof(ResultAttributeValues), strResult)))
				{
					strResult = result.String();
				}

				int iTemp = 0;
				if (int.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.testcasecount.String()), out iTemp))
				{
					testCaseCount += iTemp;
				}

				if (int.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.total.String()), out iTemp))
				{
					total += iTemp;
				}

				if (result != ResultAttributeValues.Skipped)
				{
					double dTempDuration = 0.0;
					if (double.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.duration.String()), out dTempDuration))
					{
						duration += dTempDuration;
						if (childTestObject.Name == NodeNames.testCase.String())
						{
							_dTotalDuration += dTempDuration;
						}
					}
				}
				else
				{
					if (int.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.skipped.String()), out iTemp))
					{
						skipped += iTemp;
					}
				}

				if (int.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.passed.String()), out iTemp))
				{
					passed += iTemp;
				}

				if (int.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.failed.String()), out iTemp))
				{
					failed += iTemp;
				}

				if (int.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.inconclusive.String()), out iTemp))
				{
					inconclusive += iTemp;
				}

				if (int.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.asserts.String()), out iTemp))
				{
					asserts += iTemp;
				}

				//if (!bHasChildTestSuites && childTestObject.Name == NodeNames.testSuite.String())
				//{
				//	bHasChildTestSuites = true;
				//}

				if (childTestObject.Name == NodeNames.testCase.String())
				{
					testCaseCount++;
					total++;
					if (result == ResultAttributeValues.Passed)
					{
						passed++;
					}
					else if (result == ResultAttributeValues.Failed)
					{
						failed++;
					}
					else if (result == ResultAttributeValues.Skipped)
					{
						skipped++;
					}
					else if (result == ResultAttributeValues.Inconclusive)
					{
						inconclusive++;
					}
					else
					{
						Debug.Assert(true, "Test case result type: " + result.String() + " is not handled");
					}
				}
			}

			if (bUpdateTestSuiteAttributes)
			{
				if (string.IsNullOrEmpty(strResult))
				{
					strResult = ResultAttributeValues.Inconclusive.String();
				}

				testObject.SetAttributeValue(TestSuiteAttributes.runstate.String(), strRunState);
				testObject.SetAttributeValue(TestSuiteAttributes.result.String(), strResult);
				testObject.SetAttributeValue(TestSuiteAttributes.testcasecount.String(), testCaseCount.ToString());
				testObject.SetAttributeValue(TestSuiteAttributes.total.String(), total.ToString());
				testObject.SetAttributeValue(TestSuiteAttributes.passed.String(), passed.ToString());
				testObject.SetAttributeValue(TestSuiteAttributes.failed.String(), failed.ToString());
				testObject.SetAttributeValue(TestSuiteAttributes.inconclusive.String(), inconclusive.ToString());
				testObject.SetAttributeValue(TestSuiteAttributes.skipped.String(), skipped.ToString());
				testObject.SetAttributeValue(TestSuiteAttributes.asserts.String(), asserts.ToString());
				testObject.SetAttributeValue(TestSuiteAttributes.duration.String(), duration.ToString());

				if (testObject.GetAttributeValue(TestSuiteAttributes.result.String()) != ResultAttributeValues.Failed.String())
				{
					if (testObject.HasChildNodes)
					{
						var failureNodes = testObject.ChildNodes.Shim<XmlNode>().Where(x => x.Name == NodeNames.failure.String());
						if (failureNodes.Count() > 0)
						{
							failureNodes = failureNodes.ToList();
							foreach (var failureNode in failureNodes)
							{
								failureNode.ParentNode.RemoveChild(failureNode);
							}
						}
					}
				}
				else if (testObject.HasChildNodes)
				{
					var failureNodes = testObject.ChildNodes.Shim<XmlNode>().Where(x => x.Name == NodeNames.failure.String());
					if (failureNodes.Count() <= 0)
					{
						testObject.PrependChild(CreateANewFailureNode(testObject.OwnerDocument));
					}
				}

				//if (bHasChildTestSuites)
				//{
				//	testObject.SetAttributeValue(TestSuiteAttributes.site.String(), SiteAttributes.Child.String());
				//}
			}
		}

		private bool AreTheySimilarTestObject(XmlNode testObject1, XmlNode testObject2)
		{
			if (testObject1.Name != testObject2.Name)
			{
				return false;
			}

			if (testObject1.GetAttributeValue(TestSuiteAttributes.type.String()) !=
					testObject2.GetAttributeValue(TestSuiteAttributes.type.String()))
			{
				return false;
			}

			if (testObject1.GetAttributeValue(TestSuiteAttributes.id.String()) !=
					testObject2.GetAttributeValue(TestSuiteAttributes.id.String()))
			{
				return false;
			}

			if (testObject1.GetAttributeValue(TestSuiteAttributes.fullname.String()) !=
					testObject2.GetAttributeValue(TestSuiteAttributes.fullname.String()))
			{
				return false;
			}

			return true;
		}

		private void ConsolidateTestCaseResults(XmlNode testCaseNodeOfConsolidatedReport, XmlNode testCaseNodeOfReportToBeMerged)
		{
			if (testCaseNodeOfConsolidatedReport.ParentNode == null)
			{
				return;
			}

			List<XmlNode> lstTestCases = new List<XmlNode>() { testCaseNodeOfConsolidatedReport, testCaseNodeOfReportToBeMerged };

			if (testCaseNodeOfReportToBeMerged ==
				lstTestCases.OrderBy(x => _lstOutcomePrioritiesForTestCasesWithMultipleEntries.IndexOf(GetTestCaseOutcome(x))).First())
			{
				XmlNode parentNode = testCaseNodeOfConsolidatedReport.ParentNode;
				parentNode.ReplaceChild(parentNode.OwnerDocument.ImportNode(testCaseNodeOfReportToBeMerged, true), testCaseNodeOfConsolidatedReport);
			}
		}

		private ResultAttributeValues GetTestCaseOutcome(XmlNode testCaseNode)
		{
			return testCaseNode.GetAttributeValue(TestSuiteAttributes.result.String()).ToResultAttributeValue();
		}

		private void UpdateTestRunNode(XmlDocument xmlDoc)
		{
			var testRunNode = xmlDoc.GetElementsByTagName(NodeNames.testRun.String())[0];

			testRunNode.SetAttributeValue(TestRunAttributes.testcasecount.String(), _iTestCaseCount.ToString());
			testRunNode.SetAttributeValue(TestRunAttributes.total.String(), _iTotal.ToString());
			testRunNode.SetAttributeValue(TestRunAttributes.passed.String(), _iTotalPassed.ToString());
			testRunNode.SetAttributeValue(TestRunAttributes.failed.String(), _iTotalFailed.ToString());
			testRunNode.SetAttributeValue(TestRunAttributes.inconclusive.String(), _iTotalInconclusive.ToString());
			testRunNode.SetAttributeValue(TestRunAttributes.skipped.String(), _iTotalSkipped.ToString());
			testRunNode.SetAttributeValue(TestRunAttributes.asserts.String(), _iTotalAsserts.ToString());
			testRunNode.SetAttributeValue(TestRunAttributes.duration.String(), _dTotalDuration.ToString());

			var commandLineNodes = testRunNode.ChildNodes.Shim<XmlNode>().Where(x => x.Name == NodeNames.commandLine.String());
			if (commandLineNodes.Count() > 0)
			{
				testRunNode.ReplaceChild(CreateANewCommandLineNode(xmlDoc), commandLineNodes.First());
			}

			var filterNodes = testRunNode.ChildNodes.Shim<XmlNode>().Where(x => x.Name == NodeNames.filter.String());
			if (filterNodes.Count() > 0)
			{
				testRunNode.RemoveChild(filterNodes.First());
			}
		}

		private XmlNode CreateANewFailureNode(XmlDocument xmlDoc)
		{
			XmlNode failureNode = xmlDoc.CreateNode(XmlNodeType.Element, NodeNames.failure.String(), "");

			XmlNode messageNode = xmlDoc.CreateNode(XmlNodeType.Element, NodeNames.message.String(), "");
			XmlCDataSection cdata = xmlDoc.CreateCDataSection("One or more child tests had errors");
			messageNode.AppendChild(cdata);

			XmlNode stackTraceNode = xmlDoc.CreateNode(XmlNodeType.Element, NodeNames.stackTrace.String(), "");

			failureNode.AppendChild(messageNode);
			failureNode.AppendChild(stackTraceNode);

			return failureNode;
		}

		private void RemoveTestSuiteOfType_TestProject(XmlDocument xmlDoc)
		{
			var testSuitesOfType_TestProject = xmlDoc.DocumentElement.SelectNodes(GetXPathOfTestSuiteOfType_TestProject()).Item(0);
			var testSuitesOfType_Assembly = testSuitesOfType_TestProject.ChildNodes.Shim<XmlNode>()
														.Where(x => x.Name == NodeNames.testSuite.String()).ToList();

			foreach (var testSuite in testSuitesOfType_Assembly)
			{
				testSuite.ParentNode.RemoveChild(testSuite);
				testSuitesOfType_TestProject.ParentNode.AppendChild(testSuite);
			}

			testSuitesOfType_TestProject.ParentNode.RemoveChild(testSuitesOfType_TestProject);
		}

		private XmlNode CreateANewCommandLineNode(XmlDocument xmlDoc)
		{
			XmlNode commandLine = xmlDoc.CreateNode(XmlNodeType.Element, NodeNames.commandLine.String(), "");

			XmlCDataSection cdata = xmlDoc.CreateCDataSection("Please check the intermediate test-result files");
			commandLine.AppendChild(cdata);

			return commandLine;
		}

		public void CreateXmlOfTestCasesWithGivenOutcomes(string strResultFile, string strXmlToContainTestCasesWithGivenOutcomes, IEnumerable<string> outcomes)
		{
			StringBuilder sbTestCasesXml = new StringBuilder("<?xml version='1.0' encoding='utf-8' ?>");
			sbTestCasesXml.AppendLine("<TestCases>");

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(strResultFile);

			CreateTestSuiteOfProjectTypeIfNotExists(xmlDoc);

			var testCaseNodes = xmlDoc.GetElementsByTagName(NodeNames.testCase.String()).Shim<XmlNode>();
			List<string> lstTestCases = new List<string>();
			Parallel.ForEach(testCaseNodes, (testCaseNode) =>
			{
				if (outcomes.Contains(GetTestCaseOutcome(testCaseNode).ToString()))
				{
					lock (lstTestCases)
					{
						lstTestCases.Add(testCaseNode.GetAttributeValue(TestSuiteAttributes.fullname.String()));
					}
				}
			});

			foreach (string strTestCase in lstTestCases)
			{
				sbTestCasesXml.AppendLine(string.Format("<TestCase Name='{0}'></TestCase>", strTestCase));
			}

			sbTestCasesXml.AppendLine("</TestCases>");
			XmlDocument xmlDocToContainTestCasesWithGivenOutcomes = new XmlDocument();
			xmlDocToContainTestCasesWithGivenOutcomes.LoadXml(sbTestCasesXml.ToString());
			xmlDocToContainTestCasesWithGivenOutcomes.Save(strXmlToContainTestCasesWithGivenOutcomes);
		}
	}
}
