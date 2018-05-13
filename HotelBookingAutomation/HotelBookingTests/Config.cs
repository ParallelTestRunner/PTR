using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBookingTests
{
    public class Config
    {
        public static string WebsiteURL;

        public static int ImplicitWaitTimeOutInMilliseconds;
        public static string BrowserType;
        public static string DriverLocation;

        static Config()
        {
            WebsiteURL = ConfigurationManager.AppSettings["WebsiteURL"];

            ImplicitWaitTimeOutInMilliseconds = Convert.ToInt32(ConfigurationManager.AppSettings["ImplicitWaitTimeOutInMilliseconds"]);
            BrowserType = ConfigurationManager.AppSettings["BrowserType"];
            DriverLocation = ConfigurationManager.AppSettings["DriverLocation"];
        }
    }
}
