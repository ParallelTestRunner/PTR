using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PTR
{
	interface IPTRManager
	{
		void EnqueueTestCaseExecutor(ITestConfigManager testConfigManager, Action testCaseExecutor);
		void Run();
	}
}
