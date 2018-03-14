using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitReportUtility
{
	static class EnumerableExtensions
	{
		public static IEnumerable<T> Shim<T>(this IEnumerable enumerable)
		{
			foreach (object current in enumerable)
			{
				yield return (T)current;
			}
		}
	}
}
