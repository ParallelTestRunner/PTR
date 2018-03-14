using Cafetownsend.Automation.POM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace Cafetownsend.Automation.Tests
{
    [Binding]
    public sealed class Hooks
    {
        [BeforeTestRun]
        public static void BeforeAutomationRun()
        {
            WebManager.Initialize();
        }

        [AfterTestRun]
        public static void AfterAutomationRun()
        {
            WebManager.Cleanup();
        }

        [BeforeScenario]
        public static void BeforeScenarioRun()
        {
            WebManager.OpenWebpage("about:blank");
        }

        [AfterScenario]
        public static void AfterScenarioRun()
        {
            if (ScenarioContext.Current.TestError != null)
            {
                Console.WriteLine();
                Console.WriteLine($"Test case failed on page: {WebManager.URL}");
            }
            System.Threading.Thread.Sleep(1000);
        }
    }
}
