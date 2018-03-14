using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface ILogUtil
	{
		void LogMessage(string strMessage);
		void LogStatus(string strStatus);
		void LogWarning(string strWarning);
		void LogError(string strError);
	}
}
