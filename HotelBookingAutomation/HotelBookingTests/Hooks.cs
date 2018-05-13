using HotelBookingTests.PageObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace HotelBookingTests
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
            System.Threading.Thread.Sleep(1500);
            WebManager.Cleanup();
        }

        [BeforeScenario]
        public static void BeforeScenarioRun()
        {
            WebManager.OpenWebpage("about:blank");
            System.Threading.Thread.Sleep(1500);
        }

        [AfterScenario]
        public static void AfterScenarioRun()
        {
            if (ScenarioContext.Current.TestError != null)
            {
                Console.WriteLine();
                Console.WriteLine($"Test case failed on page: {WebManager.URL}");
            }
        }
    }
}
