using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.POM
{
    public interface IWebComponent
    {
        bool IsVisible { get; }
        bool WaitUntil(Func<bool> condition, int maxWaitTimeInMilliseconds = -1);
    }
}
