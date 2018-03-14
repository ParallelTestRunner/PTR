using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface ICommandLineProcessor
	{
		void ApplyCommandLineArguments(ProcessWideConfig processWideConfig, IEnumerable<TestConfig> testConfigCollection);
	}
}
