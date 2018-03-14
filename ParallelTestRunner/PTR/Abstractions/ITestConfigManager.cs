using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface ITestConfigManager
	{
		TestConfig TestConfig { get; }
		List<Action> TestCaseExecutors { get; }
		void ActionCompleted(Action action);
		string ConsolidatedResultsFile { get; }
		void MergeOtherConfigManagersResultsWhereeverRequired(IEnumerable<ITestConfigManager> otherTestConfigManagers);
		void CleanUp();
	}
}
