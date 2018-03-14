using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	enum ConcurentUnitType
	{
		Undefined = 0,
		ClassLevel,
		TestCaseLevel
	}

	static class ConcurentUnitTypeExtensions
	{
		public static ConcurentUnitType ConcurentUnitType(this int iConcurentUnit)
		{
			switch (iConcurentUnit)
			{
				case 1:
					return PTR.ConcurentUnitType.ClassLevel;

				case 2:
					return PTR.ConcurentUnitType.TestCaseLevel;
			}

			return PTR.ConcurentUnitType.Undefined;
		}
	}
}
