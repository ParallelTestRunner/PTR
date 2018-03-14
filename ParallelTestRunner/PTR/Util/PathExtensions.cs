using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	static class PathExtensions
	{
		public static string NormalisedPath(this string strPath)
		{
			return strPath.Trim(new char[] { ' ', '/', '\\' }).ToLower();
		}

		public static string FirstFile(this string strPath)
		{
			return strPath.Split(';')[0];
		}
	}
}
