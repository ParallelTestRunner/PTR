using HotelBookingTests.PageObjectModel.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBookingTests.PageObjectModel
{
    class WebManager
    {
        private static IWebDriver Driver { get; set; }

        public static void Initialize()
        {
            if (Driver == null)
            {
                Driver = GetWebdriver(Config.BrowserType, Config.ImplicitWaitTimeOutInMilliseconds, false, Config.DriverLocation);
                CreatePages();
            }
        }

        private static IWebDriver GetWebdriver(string browserType,
                                                int implicitWaitTimeOutInMilliseconds,
                                                bool openInPrivateMode,
                                                string driverLocation = "")
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

        public static void Cleanup()
        {
            Driver.Quit();
        }

        public static void OpenWebpage(string strURL)
        {
            Driver.Navigate().GoToUrl(strURL);
        }

        public static string URL { get { return Driver.Url; } }

        private static void CreatePages()
        {
            HotelBookingPage = new HotelBookingPage(Driver);
        }

        public static IHotelBookingPage HotelBookingPage { get; private set; }
    }
}
