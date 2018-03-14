using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUtil
{
	interface ILicenseChecker
	{
		bool DoesThisMachineHaveAValidLicense();
	}
}
