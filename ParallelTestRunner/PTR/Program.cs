using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class Program
	{
		static void Main(string[] args)
		{
			IObjectFactory objectFactory = new ObjectFactory();
			objectFactory.PTRManager.Run();
		}
	}
}
