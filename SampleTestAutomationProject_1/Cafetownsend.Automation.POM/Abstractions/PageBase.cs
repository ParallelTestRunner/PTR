using Cafetownsend.Automation.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.POM
{
    abstract class PageBase
    {
        public ITestDriver Driver { get; set; }
        protected UIMap _uiMap;
        protected PageBase(ITestDriver driver, UIMap uiMap)
        {
            Driver = driver;
            _uiMap = uiMap;
        }

        public bool WaitUntil(Func<bool> condition, int maxWaitTimeInMilliseconds = -1)
        {
            return Driver.WaitUntil(condition, maxWaitTimeInMilliseconds);
        }
    }

    class UIMap
    {
    }
}
