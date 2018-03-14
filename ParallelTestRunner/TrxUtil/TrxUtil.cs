using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Collections;

namespace TrxUtil
{
	class TrxUtil : IReportUtil
	{
		List<TestResultOutcomeAttributeValues> _lstOutcomePrioritiesForTestCasesWithMultipleEntries;
		public TrxUtil()
		{
			_lstOutcomePrioritiesForTestCasesWithMultipleEntries = null;
		}

		public TrxUtil(List<TestResultOutcomeAttributeValues> lstOutcomePrioritiesForTestCasesWithMultipleEntries)
		{
			_lstOutcomePrioritiesForTestCasesWithMultipleEntries = lstOutcomePrioritiesForTestCasesWithMultipleEntries;
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

			ConsolidateTimesNode(xmlDocConsolidatedReport, xmlDocReportToBeMerged);

			ConsolidateResultSummary(xmlDocConsolidatedReport, xmlDocReportToBeMerged);

			ConsolidateCounters(xmlDocConsolidatedReport, xmlDocReportToBeMerged);

			ConsolidateNodes(xmlDocConsolidatedReport, xmlDocReportToBeMerged, NodeNames.UnitTest);

			ConsolidateNodes(xmlDocConsolidatedReport, xmlDocReportToBeMerged, NodeNames.UnitTestResult);

			ConsolidateNodes(xmlDocConsolidatedReport, xmlDocReportToBeMerged, NodeNames.TestEntry);

			RemoveDuplicateTestCases(xmlDocConsolidatedReport);

			xmlDocConsolidatedReport.Save(strConsolidatedReport);
		}

		private void ConsolidateTimesNode(XmlDocument xmlDocConsolidatedReport, XmlDocument xmlDocReportToBeMerged)
		{
			string strTimesNodeName = NodeNames.Times.ToString();
			var TimesOfConsolidatedReport = xmlDocConsolidatedReport.GetElementsByTagName(strTimesNodeName)[0];
			var TimesOfReportToBeMerged = xmlDocReportToBeMerged.GetElementsByTagName(strTimesNodeName)[0];

			foreach (TimeAttributes time in Enum.GetValues(typeof(TimeAttributes)))
			{
				string strTime = time.ToString();
				TimesOfConsolidatedReport.SetAttributeValue(strTime,
																			GetTimeByItsOccurrence(TimesOfConsolidatedReport,
																											TimesOfReportToBeMerged,
																											strTime,
																											time != TimeAttributes.finish)
																			);
			}
		}

		private string GetTimeByItsOccurrence(XmlNode xmlNode1, XmlNode xmlNode2, string strTimeAttributeName, bool bGetFormerTime = true)
		{
			string strTime1 = xmlNode1.GetAttributeValue(strTimeAttributeName);
			string strTime2 = xmlNode2.GetAttributeValue(strTimeAttributeName);

			if ((string.IsNullOrEmpty(strTime1) && !string.IsNullOrEmpty(strTime2)) ||
					(string.IsNullOrEmpty(strTime2) && !string.IsNullOrEmpty(strTime1)))
			{
				return string.IsNullOrEmpty(strTime1) ? strTime2 : strTime1;
			}

			if (DateTime.Parse(strTime1).CompareTo(DateTime.Parse(strTime2)) <= 0)
			{
				return bGetFormerTime ? strTime1 : strTime2;
			}

			string strTimeByOccurrence = bGetFormerTime ? strTime2 : strTime1;

			return string.IsNullOrEmpty(strTimeByOccurrence) ? "" : strTimeByOccurrence;
		}

		private void ConsolidateResultSummary(XmlDocument xmlDocConsolidatedReport, XmlDocument xmlDocReportToBeMerged)
		{
			string strResultSummaryNodeName = NodeNames.ResultSummary.ToString();
			var ResultSummaryOfConsolidatedReport = xmlDocConsolidatedReport.GetElementsByTagName(strResultSummaryNodeName)[0];
			var ResultSummaryOfReportToBeMerged = xmlDocReportToBeMerged.GetElementsByTagName(strResultSummaryNodeName)[0];

			string strOutcomeAttribute = ResultSummaryAttributes.outcome.ToString();
			string strOutcomeAttributeValue = ResultSummaryOfReportToBeMerged.GetAttributeValue(strOutcomeAttribute);
			if (!string.IsNullOrEmpty(strOutcomeAttributeValue) && strOutcomeAttributeValue == TestResultOutcomeAttributeValues.Failed.ToString())
			{
				ResultSummaryOfConsolidatedReport.SetAttributeValue(strOutcomeAttribute, strOutcomeAttributeValue);
			}
		}

		private void ConsolidateCounters(XmlDocument xmlDocConsolidatedReport, XmlDocument xmlDocReportToBeMerged)
		{
			string strCountersNodeName = NodeNames.Counters.ToString();
			var CountersOfConsolidatedReport = xmlDocConsolidatedReport.GetElementsByTagName(strCountersNodeName)[0];
			var CountersOfReportToBeMerged = xmlDocReportToBeMerged.GetElementsByTagName(strCountersNodeName)[0];

			foreach (CounterAttributes counter in Enum.GetValues(typeof(CounterAttributes)))
			{
				string strCounter = counter.ToString();
				CountersOfConsolidatedReport.SetAttributeValue(strCounter,
																				GetAddedAttributeValues(CountersOfConsolidatedReport,
																												CountersOfReportToBeMerged,
																												strCounter));
			}
		}

		private string GetAddedAttributeValues(XmlNode xmlNode1, XmlNode xmlNode2, string strAttributeName)
		{
			string strValue1 = xmlNode1.GetAttributeValue(strAttributeName);
			string strValue2 = xmlNode2.GetAttributeValue(strAttributeName);

			if ((string.IsNullOrEmpty(strValue1) && !string.IsNullOrEmpty(strValue2)) ||
					(string.IsNullOrEmpty(strValue2) && !string.IsNullOrEmpty(strValue1)))
			{
				return string.IsNullOrEmpty(strValue1) ? strValue2 : strValue1;
			}

			Int32 iValue1 = 0;
			Int32 iValue2 = 0;
			if (Int32.TryParse(strValue1, out iValue1) && Int32.TryParse(strValue2, out iValue2))
			{
				return (iValue1 + iValue2).ToString();
			}

			return strValue1 + strValue2;
		}

		private void ConsolidateNodes(XmlDocument xmlDocConsolidatedReport, XmlDocument xmlDocReportToBeMerged, NodeNames nodeName)
		{
			string strNodeName = nodeName.ToString();
			string strParentNodeName = nodeName.GetParentNodeName().ToString();

			var nodesOfReportToBeMerged = Shim<XmlNode>(xmlDocReportToBeMerged.GetElementsByTagName(strNodeName))
																		.Where(x => x.ParentNode.Name == strParentNodeName);

			var parentNodeOfGivenNodeInConsolidatedReport = xmlDocConsolidatedReport.GetElementsByTagName(strParentNodeName)[0];
			foreach (XmlNode xmlNode in nodesOfReportToBeMerged)
			{
				parentNodeOfGivenNodeInConsolidatedReport.AppendChild(xmlDocConsolidatedReport.ImportNode(xmlNode, true));
			}
		}

		private void RemoveDuplicateTestCases(XmlDocument xmlDocConsolidatedReport)
		{
			var lstRemovedUnitTestResult = RemoveDuplicateNodes(xmlDocConsolidatedReport,
																					NodeNames.UnitTestResult,
																					UnitTestResultAttributes.testId.ToString(),
																					SortNodesByGivenOutcome);

			RemoveAssociatedUnitTestNodes(xmlDocConsolidatedReport, lstRemovedUnitTestResult);

			RemoveAssociatedTestEntryNodes(xmlDocConsolidatedReport, lstRemovedUnitTestResult);

			UpdateCounters(xmlDocConsolidatedReport, lstRemovedUnitTestResult);
		}

		private IEnumerable<XmlNode> SortNodesByGivenOutcome(IEnumerable<XmlNode> nodes)
		{
			if (_lstOutcomePrioritiesForTestCasesWithMultipleEntries == null)
			{
				return nodes;
			}
			return nodes.OrderBy(x => _lstOutcomePrioritiesForTestCasesWithMultipleEntries.IndexOf(GetTestCaseOutcome(x)));
		}

		private TestResultOutcomeAttributeValues GetTestCaseOutcome(XmlNode unitTestResultNode)
		{
			return unitTestResultNode.GetAttributeValue(ResultSummaryAttributes.outcome.ToString()).ToResultSummaryOutcome();
		}

		private List<XmlNode> RemoveDuplicateNodes(XmlDocument xmlDoc,
																	NodeNames nodeName,
																	string strAttributeToBeCheckedToMarkAsDuplicate,
																	Func<IEnumerable<XmlNode>, IEnumerable<XmlNode>>
																	processorBeforeRemoving = null)
		{
			var nodes = Shim<XmlNode>(xmlDoc.GetElementsByTagName(nodeName.ToString()))
							.Where(x => x.ParentNode.Name == nodeName.GetParentNodeName().ToString());

			var parentNode = xmlDoc.GetElementsByTagName(nodeName.GetParentNodeName().ToString())[0];

			Dictionary<string, List<XmlNode>> dicNodesByAttributeValue = new Dictionary<string, List<XmlNode>>();
			foreach (XmlNode xmlNode in nodes)
			{
				string strAttributeValue = xmlNode.GetAttributeValue(strAttributeToBeCheckedToMarkAsDuplicate);
				if (!dicNodesByAttributeValue.ContainsKey(strAttributeValue))
				{
					dicNodesByAttributeValue.Add(strAttributeValue, new List<XmlNode>());
				}

				dicNodesByAttributeValue[strAttributeValue].Add(xmlNode);
			}

			List<XmlNode> lstRemovedNodes = new List<XmlNode>();

			foreach (var keyValPair in dicNodesByAttributeValue)
			{
				IEnumerable<XmlNode> nodesToBeRemoved = keyValPair.Value;
				if (processorBeforeRemoving != null)
				{
					nodesToBeRemoved = processorBeforeRemoving(nodesToBeRemoved).ToList();
				}

				for (int i = 1; i < nodesToBeRemoved.Count(); i++)
				{
					lstRemovedNodes.Add(nodesToBeRemoved.ElementAt(i));
					parentNode.RemoveChild(nodesToBeRemoved.ElementAt(i));
				}
			}

			return lstRemovedNodes;
		}

		private void RemoveAssociatedUnitTestNodes(XmlDocument xmlDoc, List<XmlNode> lstRemovedUnitTestResult)
		{
			var lstUnitTestResultIdentifier = lstRemovedUnitTestResult.Select(x => GetUnitTestResultNodeIdentifier(x)).ToList();

			NodeNames nodeNameExecution = NodeNames.Execution;
			string strNodeNameExecutionParent = nodeNameExecution.GetParentNodeName().ToString();
			var nodes = xmlDoc.GetElementsByTagName(nodeNameExecution.ToString());
			var parentNode = xmlDoc.GetElementsByTagName(NodeNames.UnitTest.GetParentNodeName().ToString())[0];

			var lstNodes = new List<XmlNode>(Shim<XmlNode>(nodes));
			foreach (XmlNode xmlNodeExecution in lstNodes)
			{
				if (xmlNodeExecution.ParentNode.Name == strNodeNameExecutionParent)
				{
					string strId = GetExecutionNodeIdentifier(xmlNodeExecution);
					int index = lstUnitTestResultIdentifier.IndexOf(strId);
					if (index >= 0)
					{
						lstUnitTestResultIdentifier.RemoveAt(index);
						parentNode.RemoveChild(xmlNodeExecution.ParentNode);
					}
				}
			}
		}

		private string GetUnitTestResultNodeIdentifier(XmlNode xmlNodeUnitTestResult)
		{
			return xmlNodeUnitTestResult.GetAttributeValue(UnitTestResultAttributes.executionId.ToString()) +
						xmlNodeUnitTestResult.GetAttributeValue(UnitTestResultAttributes.testId.ToString());
		}

		private string GetExecutionNodeIdentifier(XmlNode xmlNodeExecution)
		{
			return xmlNodeExecution.GetAttributeValue(ExecutionAttributes.id.ToString()) +
						xmlNodeExecution.ParentNode.GetAttributeValue(UnitTestAttributes.id.ToString());
		}

		private void RemoveAssociatedTestEntryNodes(XmlDocument xmlDoc, List<XmlNode> lstRemovedUnitTestResult)
		{
			var lstUnitTestResultIdentifier = lstRemovedUnitTestResult.Select(x => GetUnitTestResultNodeIdentifier(x)).ToList();

			NodeNames nodeNameTestEntry = NodeNames.TestEntry;
			var nodes = xmlDoc.GetElementsByTagName(nodeNameTestEntry.ToString());
			var parentNode = xmlDoc.GetElementsByTagName(NodeNames.TestEntry.GetParentNodeName().ToString())[0];

			var lstNodes = new List<XmlNode>(Shim<XmlNode>(nodes));
			foreach (XmlNode xmlNodeTestEntry in lstNodes)
			{
				string strId = GetTestEntryNodeIdentifier(xmlNodeTestEntry);
				int index = lstUnitTestResultIdentifier.IndexOf(strId);
				if (index >= 0)
				{
					lstUnitTestResultIdentifier.RemoveAt(index);
					parentNode.RemoveChild(xmlNodeTestEntry);
				}
			}
		}

		private string GetTestEntryNodeIdentifier(XmlNode xmlNodeTestEntry)
		{
			return xmlNodeTestEntry.GetAttributeValue(TestEntryAttributes.executionId.ToString()) +
						xmlNodeTestEntry.GetAttributeValue(TestEntryAttributes.testId.ToString());
		}

		private IEnumerable<T> Shim<T>(IEnumerable enumerable)
		{
			foreach (object current in enumerable)
			{
				yield return (T)current;
			}
		}

		private void UpdateCounters(XmlDocument xmlDoc, List<XmlNode> lstRemovedUnitTestResult)
		{
			int TotalReduction = 0;
			int ExecutedReduction = 0;
			int PassedReduction = 0;
			int FailedReduction = 0;
			int PendingReduction = 0;
			foreach (XmlNode xmlNodeUnitTestResult in lstRemovedUnitTestResult)
			{
				TotalReduction++;
				var outcome = xmlNodeUnitTestResult.GetAttributeValue(UnitTestResultAttributes.outcome.ToString());
				if (outcome == TestResultOutcomeAttributeValues.Failed.ToString())
				{
					FailedReduction++;
					ExecutedReduction++;
				}
				else if (outcome == TestResultOutcomeAttributeValues.Passed.ToString())
				{
					PassedReduction++;
					ExecutedReduction++;
				}
				else if (outcome == TestResultOutcomeAttributeValues.Pending.ToString())
				{
					PendingReduction++;
				}
			}

			var CountersOfConsolidatedReport = xmlDoc.GetElementsByTagName(NodeNames.Counters.ToString())[0];
			UpdateCounterAttribute(CountersOfConsolidatedReport, CounterAttributes.total, TotalReduction);

			UpdateCounterAttribute(CountersOfConsolidatedReport, CounterAttributes.executed, ExecutedReduction);

			UpdateCounterAttribute(CountersOfConsolidatedReport, CounterAttributes.passed, PassedReduction);

			UpdateCounterAttribute(CountersOfConsolidatedReport, CounterAttributes.failed, FailedReduction);

			UpdateCounterAttribute(CountersOfConsolidatedReport, CounterAttributes.pending, PendingReduction);
		}

		private void UpdateCounterAttribute(XmlNode xmlNodeCounters, CounterAttributes counterAttribute, int iValueToBeReduced)
		{
			Int32 iValue = 0;
			Int32.TryParse(xmlNodeCounters.GetAttributeValue(counterAttribute.ToString()), out iValue);
			iValue -= iValueToBeReduced;
			xmlNodeCounters.SetAttributeValue(counterAttribute.ToString(), iValue.ToString());
		}

		public void CreateXmlOfTestCasesWithGivenOutcomes(string strResultFile,
																	string strXmlToContainTestCasesWithGivenOutcomes,
																	IEnumerable<string> outcomes)
		{
			StringBuilder sbTestCasesXml = new StringBuilder("<?xml version='1.0' encoding='utf-8' ?>");
			sbTestCasesXml.AppendLine("<TestCases>");

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(strResultFile);

			NodeNames nodeNameExecution = NodeNames.Execution;
			var executionNodes = xmlDoc.GetElementsByTagName(nodeNameExecution.ToString());
			string strNodeNameExecutionParent = nodeNameExecution.GetParentNodeName().ToString();

			List<string> lstTestCases = new List<string>();
			var unitTestResultNodes = Shim<XmlNode>(xmlDoc.GetElementsByTagName(NodeNames.UnitTestResult.ToString()));
			Parallel.ForEach(unitTestResultNodes, (unitTestResultNode) =>
			{
				if (outcomes.Contains(GetTestCaseOutcome(unitTestResultNode).ToString().ToLower()))
				{
					string strUnitTestResultNodeId = GetUnitTestResultNodeIdentifier(unitTestResultNode);
					foreach (XmlNode xmlNodeExecution in executionNodes)
					{
						if (xmlNodeExecution.ParentNode.Name == strNodeNameExecutionParent)
						{
							string strExecutionNodeId = GetExecutionNodeIdentifier(xmlNodeExecution);
							if (strUnitTestResultNodeId == strExecutionNodeId)
							{
								var testMethodNode = Shim<XmlNode>(xmlNodeExecution.ParentNode.ChildNodes)
															.Where(x => x.Name == NodeNames.TestMethod.ToString()).First();

								lock (lstTestCases)
								{
									lstTestCases.Add(testMethodNode.GetAttributeValue(TestMethodAttributes.className.ToString()).Split(',').First() + "." +
															testMethodNode.GetAttributeValue(TestMethodAttributes.name.ToString()));
								}
								break;
							}
						}
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
