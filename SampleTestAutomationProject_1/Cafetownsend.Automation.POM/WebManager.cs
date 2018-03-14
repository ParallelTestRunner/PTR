using Cafetownsend.Automation.Configurations;
using Cafetownsend.Automation.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.POM
{
    public class WebManager
    {
        internal static ITestDriver Driver { get; private set; }

        public static void Initialize()
        {
            if (Driver == null)
            {
                Driver = DriverFactory.GetTestDriver(Config.BrowserType,
                                                        Config.ImplicitWaitTimeOutInMilliseconds,
                                                        Config.DefaultExplilicitWaitTimeOutInMilliseconds,
                                                        false,
                                                        Config.DriverLocation);
                CreatePages();
            }
        }

        public static void Cleanup()
        {
            Driver.Quit();
        }

        public static void OpenWebpage(string strURL)
        {
            Driver.OpenURL(strURL);
        }

        public static string URL { get { return Driver.URL; } }

        private static void CreatePages()
        {
            PageLogin = PageLoginFactory.GetPageLogin(Driver);
            PageUserRecords = PageUserRecordsFactory.GetPageUserRecords(Driver);
        }

        public static IPageLogin PageLogin { get; private set; }
        public static IPageUserRecords PageUserRecords { get; private set; }
    }
}
