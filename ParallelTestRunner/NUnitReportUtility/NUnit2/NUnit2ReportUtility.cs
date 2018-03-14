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

namespace NUnitReportUtilityNunit2
{
	class NUnit2ReportUtility : IReportUtil
	{
		List<ResultAttributeValues> _lstOutcomePrioritiesForTestCasesWithMultipleEntries;

		public NUnit2ReportUtility()
		{
			_lstOutcomePrioritiesForTestCasesWithMultipleEntries = null;
		}

		public NUnit2ReportUtility(List<ResultAttributeValues> lstOutcomePriority)
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

			ConsolidateTestResultsNode(xmlDocConsolidatedReport, xmlDocReportToBeMerged);

			CreateTestSuiteOfProjectTypeIfNotExists(xmlDocConsolidatedReport);

			CreateTestSuiteOfProjectTypeIfNotExists(xmlDocReportToBeMerged);

			ConsolidateTestSuitesOfAllTypes(xmlDocConsolidatedReport, xmlDocReportToBeMerged);

			UpdateTestResultsNode(xmlDocConsolidatedReport);

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
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.name.String(), "");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.executed.String(), true.ToString());
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.result.String(), ResultAttributeValues.Success.String());
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.success.String(), true.ToString());
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.time.String(), "0.0");
				xmlNodeOfTestSuiteOfTypeProjectType.SetAttributeValue(TestSuiteAttributes.asserts.String(), "0");
				var resultsNodeOfTestSuiteOfTypeProjectType = xmlDoc.CreateNode(XmlNodeType.Element, NodeNames.results.String(), "");
				xmlNodeOfTestSuiteOfTypeProjectType.AppendChild(resultsNodeOfTestSuiteOfTypeProjectType);

				var testResultsNode = xmlDoc.GetElementsByTagName(NodeNames.testResults.String())[0];
				testResultsNode.AppendChild(xmlNodeOfTestSuiteOfTypeProjectType);

				var testSuiteNodes = testResultsNode.ChildNodes.Shim<XmlNode>().ToList();

				foreach (var node in testSuiteNodes)
				{
					if (node == xmlNodeOfTestSuiteOfTypeProjectType || node.Name != NodeNames.testSuite.String())
					{
						continue;
					}

					node.ParentNode.RemoveChild(node);
					resultsNodeOfTestSuiteOfTypeProjectType.AppendChild(node);
				}
			}
		}

		private void ConsolidateTestResultsNode(XmlDocument xmlDocConsolidatedReport, XmlDocument xmlDocReportToBeMerged)
		{
			string strTestResultsNode = NodeNames.testResults.String();
			var TestResultsOfConsolidatedReport = xmlDocConsolidatedReport.GetElementsByTagName(strTestResultsNode)[0];
			var TestResultsOfReportToBeMerged = xmlDocReportToBeMerged.GetElementsByTagName(strTestResultsNode)[0];
			string strNewValue;
			foreach (TestResultsAttributes testResultAttribute in Enum.GetValues(typeof(TestResultsAttributes)))
			{
				string strTestResultAttribute = testResultAttribute.String();
				if (testResultAttribute == TestResultsAttributes.date || testResultAttribute == TestResultsAttributes.time)
				{
					continue;
				}

				strNewValue = GetAddedAttributeValues(TestResultsOfConsolidatedReport, TestResultsOfReportToBeMerged, strTestResultAttribute);

				TestResultsOfConsolidatedReport.SetAttributeValue(strTestResultAttribute, strNewValue);
			}

			string strDateAttribute = TestResultsAttributes.date.String();
			string strTimeAttribute = TestResultsAttributes.time.String();

			string strDateTimeOfConsolidatedReport = GetDateAndTimeAsASingleString(TestResultsOfConsolidatedReport, strDateAttribute, strTimeAttribute);
			string strDateTimeOfReportToBeMerged = GetDateAndTimeAsASingleString(TestResultsOfReportToBeMerged, strDateAttribute, strTimeAttribute);

			if (strDateTimeOfReportToBeMerged == GetTimeByItsOccurrence(strDateTimeOfConsolidatedReport, strDateTimeOfReportToBeMerged))
			{
				TestResultsOfConsolidatedReport.SetAttributeValue(strDateAttribute, TestResultsOfReportToBeMerged.GetAttributeValue(strDateAttribute));
				TestResultsOfConsolidatedReport.SetAttributeValue(strTimeAttribute, TestResultsOfReportToBeMerged.GetAttributeValue(strTimeAttribute));
			}
		}

		private string GetTimeByItsOccurrence(string strDateTime1, string strDateTime2, bool bGetFormerTime = true)
		{
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

		private string GetDateAndTimeAsASingleString(XmlNode xmlNode, string strDateAttribute, string strTimeAttribute)
		{
			return xmlNode.GetAttributeValue(strDateAttribute) + " " + xmlNode.GetAttributeValue(strTimeAttribute);
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

			ConsolidateChildTestSuite(GetChildResultsNode(testSuiteNodeOfConsolidatedReport), GetChildResultsNode(testSuiteNodeOfReportToBeMerged));

			UpdateAllAttributesOfTestObject(testSuiteNodeOfConsolidatedReport);
		}

		private string GetXPathOfTestSuiteOfType_TestProject()
		{
			string strXPath = "//" + NodeNames.testResults.String() + "/" +
									NodeNames.testSuite.String() +
									"[@" + TestSuiteAttributes.type.String() + "='{0}']";

			return string.Format(strXPath, TestSuiteTypes.TestProject.String());
		}

		private XmlNode GetChildResultsNode(XmlNode xmlNode)
		{
			return xmlNode.ChildNodes.Shim<XmlNode>()
						.Where(x => x.NodeType == XmlNodeType.Element && x.Name == NodeNames.results.String()).First();
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
				ConsolidateChildTestSuite(GetChildResultsNode(nodeOfConsolidatedReport), GetChildResultsNode(nodeOfReportToBeMerged));
			}
		}

		List<ResultAttributeValues> resultsAsPerTheirPriorityLevel = new List<ResultAttributeValues>()
																								{
																									ResultAttributeValues.Failure,
																									ResultAttributeValues.Success,
																									ResultAttributeValues.Inconclusive,
																									ResultAttributeValues.Ignored
																								};
		private int _iTotal = 0;
		private int _iTotalFailures = 0;
		private int _iTotalNotRun = 0;
		private int _iTotalInconclusive = 0;
		private int _iTotalIgnored = 0;
		private void UpdateAllAttributesOfTestObject(XmlNode testObject)
		{
			if (testObject.Name == NodeNames.testCase.String())
			{
				ResultAttributeValues result = GetTestCaseOutcome(testObject);

				_iTotal++;
				if (result == ResultAttributeValues.Failure)
				{
					_iTotalFailures++;
				}
				else if (result == ResultAttributeValues.Inconclusive)
				{
					_iTotalInconclusive++;
				}
				else if (result == ResultAttributeValues.Ignored)
				{
					_iTotalNotRun++;
					_iTotalIgnored++;
				}
				else
				{
					Debug.Assert(true);
				}

				return;
			}

			var childResultNode = GetChildResultsNode(testObject);
			var childTestObjects = childResultNode.ChildNodes.Shim<XmlNode>()
														.Where(x => x.Name == NodeNames.testSuite.String() || x.Name == NodeNames.testCase.String());

			bool bUpdateTestSuiteAttributes = false;
			string strExecuted = "False";
			string strResult = string.Empty;
			string strSuccess = "True";
			double dTime = 0.0;
			int iAssert = 0;

			foreach (XmlNode childTestObject in childTestObjects)
			{
				UpdateAllAttributesOfTestObject(childTestObject);

				bUpdateTestSuiteAttributes = true;
				if (childTestObject.GetAttributeValue(TestSuiteAttributes.executed.String()) == "True")
				{
					strExecuted = "True";
				}

				ResultAttributeValues result = GetTestCaseOutcome(childTestObject);
				if (string.IsNullOrEmpty(strResult) ||
						resultsAsPerTheirPriorityLevel.IndexOf(result) <
						resultsAsPerTheirPriorityLevel.IndexOf((ResultAttributeValues)Enum.Parse(typeof(ResultAttributeValues), strResult)))
				{
					strResult = result.String();
				}

				if (result != ResultAttributeValues.Ignored)
				{
					if (childTestObject.GetAttributeValue(TestSuiteAttributes.success.String()) == "False")
					{
						strSuccess = "False";
					}

					double dTempTime = 0.0;
					if (double.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.time.String()), out dTempTime))
					{
						dTime += dTempTime;
					}

					int iTempAsserts = 0;
					if (int.TryParse(childTestObject.GetAttributeValue(TestSuiteAttributes.asserts.String()), out iTempAsserts))
					{
						iAssert += iTempAsserts;
					}
				}
			}

			if (bUpdateTestSuiteAttributes)
			{
				if (string.IsNullOrEmpty(strResult))
				{
					strResult = ResultAttributeValues.Inconclusive.String();
				}

				if (strResult != ResultAttributeValues.Success.String())
				{
					strSuccess = "False";
				}

				testObject.SetAttributeValue(TestSuiteAttributes.executed.String(), strExecuted);
				testObject.SetAttributeValue(TestSuiteAttributes.result.String(), strResult);
				if (strResult != ResultAttributeValues.Ignored.String())
				{
					testObject.SetAttributeValue(TestSuiteAttributes.success.String(), strSuccess);
					testObject.SetAttributeValue(TestSuiteAttributes.time.String(), dTime.ToString());
					testObject.SetAttributeValue(TestSuiteAttributes.asserts.String(), iAssert.ToString());
				}

				if (testObject.GetAttributeValue(TestSuiteAttributes.result.String()) != ResultAttributeValues.Failure.String())
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

			if (testObject1.GetAttributeValue(TestSuiteAttributes.name.String()) !=
					testObject2.GetAttributeValue(TestSuiteAttributes.name.String()))
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

		private void UpdateTestResultsNode(XmlDocument xmlDoc)
		{
			var testResultsNode = xmlDoc.GetElementsByTagName(NodeNames.testResults.String())[0];

			testResultsNode.SetAttributeValue(TestResultsAttributes.total.String(), _iTotal.ToString());
			testResultsNode.SetAttributeValue(TestResultsAttributes.failures.String(), _iTotalFailures.ToString());
			testResultsNode.SetAttributeValue(TestResultsAttributes.inconclusive.String(), _iTotalInconclusive.ToString());
			testResultsNode.SetAttributeValue(TestResultsAttributes.notRun.String(), _iTotalNotRun.ToString());
			testResultsNode.SetAttributeValue(TestResultsAttributes.ignored.String(), _iTotalIgnored.ToString());
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
			var testSuitesOfType_Assembly = GetChildResultsNode(testSuitesOfType_TestProject).ChildNodes.Shim<XmlNode>()
														.Where(x => x.Name == NodeNames.testSuite.String()).ToList();

			foreach(var testSuite in testSuitesOfType_Assembly)
			{
				testSuite.ParentNode.RemoveChild(testSuite);
				testSuitesOfType_TestProject.ParentNode.AppendChild(testSuite);
			}

			testSuitesOfType_TestProject.ParentNode.RemoveChild(testSuitesOfType_TestProject);
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
						lstTestCases.Add(testCaseNode.GetAttributeValue(TestSuiteAttributes.name.String()));
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
