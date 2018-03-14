using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.Driver
{
    public class DriverFactory
    {
        public static ITestDriver GetTestDriver(string browserType,
                                                int implicitWaitTimeOutInMilliseconds,
                                                int defaultExplilicitWaitTimeOutInMilliseconds,
                                                bool openInPrivateMode,
                                                string driverLocation)
        {
            return new TestDriver(GetWebdriver(browserType, implicitWaitTimeOutInMilliseconds, openInPrivateMode, driverLocation),
                                    defaultExplilicitWaitTimeOutInMilliseconds);
        }

        private static IWebDriver GetWebdriver(string browserType,
                                                int implicitWaitTimeOutInMilliseconds,
                                                bool openInPrivateMode,
                                                string driverLocation="")
        {
            string driverDirectoryPath = driverLocation;
            if (string.IsNullOrEmpty(driverDirectoryPath))
            {
                driverDirectoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }

            int timeoutInMinutes = 30;

            IWebDriver iWebdriver = null;
            switch (browserType)
            {
                case "chrome":
                    {
                        ChromeOptions chromeOptions = new ChromeOptions();
                        chromeOptions.AddArgument("no-sandbox");
                        chromeOptions.AddArguments("disable-infobars");
                        if (openInPrivateMode)
                        {
                            chromeOptions.AddArguments("incognito");
                        }

                        iWebdriver = new ChromeDriver(driverDirectoryPath, chromeOptions, TimeSpan.FromMinutes(timeoutInMinutes));

                        iWebdriver.Manage().Window.Maximize();
                    }
                    break;

                default:
                    throw new Exception($"Testing on \"{browserType}\" browser is not yet supported.");
            }

            iWebdriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(implicitWaitTimeOutInMilliseconds);
            return iWebdriver;
        }
    }
}
