using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	static class BoolExtensions
	{
		public static bool ToBool(this bool? bValue)
		{
			return bValue == null ? false : (bool)bValue;
		}
	}
}
