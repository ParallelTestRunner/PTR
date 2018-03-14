using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	static class MathExtensions
	{
		public static string ToOrdinal(this int iNumber)
		{
			string strNumber = iNumber.ToString();

			if (strNumber.EndsWith("11")) return strNumber + "th";
			if (strNumber.EndsWith("12")) return strNumber + "th";
			if (strNumber.EndsWith("13")) return strNumber + "th";
			if (strNumber.EndsWith("1")) return strNumber + "st";
			if (strNumber.EndsWith("2")) return strNumber + "nd";
			if (strNumber.EndsWith("3")) return strNumber + "rd";

			return strNumber + "th";
		}
	}
}
