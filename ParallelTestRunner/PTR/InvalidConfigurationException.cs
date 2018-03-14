using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class InvalidConfigurationException : Exception
	{
		public InvalidConfigurationException(string strExceptionMessage)
			: base(strExceptionMessage)
		{

		}
	}
}
