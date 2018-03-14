using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface ITestCaseRepository
	{
		List<TestCase> AllTestCases { get; }
		Dictionary<string, List<TestCase>> TestCasesByClass { get; }
	}
}
