using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBookingTests.PageObjectModel.Pages
{
    interface IHotelBookingPage
    {
        bool IsVisible { get; }

        void EnterFirstName(string name);
        void EnterSurname(string surname);
        void EnterPrice(int price);
        void SetDeposit(bool deposit);
        void EnterCheckInDate(string date);
        void EnterCheckOutDate(string date);
        void ClickSaveRecord();

        List<string> GetAllNames();
        void DeleteRecordByName(string name);
    }

    class HotelBookingPage: IHotelBookingPage
    {
        private HotelBookingPageUiMap UiMap = new HotelBookingPageUiMap();

        IWebDriver Driver { get; set; }
        public HotelBookingPage(IWebDriver driver)
        {
            Driver = driver;
        }

        public bool IsVisible
        {
            get
            {

                try
                {
                    var element = Driver.FindElement(By.CssSelector(UiMap.HeadingCss));
                    return element.Text == "Hotel booking form";
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            }
        }

        public void EnterFirstName(string name)
        {
            Driver.FindElement(By.CssSelector(UiMap.FirstnameTextboxCss)).SendKeys(name);
        }

        public void EnterSurname(string surname)
        {
            Driver.FindElement(By.CssSelector(UiMap.SurnameTextboxCss)).SendKeys(surname);
        }

        public void EnterPrice(int price)
        {
            Driver.FindElement(By.CssSelector(UiMap.PriceTextboxCss)).SendKeys(price.ToString());
        }

        public void SetDeposit(bool deposit)
        {
            var selectElement = new SelectElement(Driver.FindElement(By.CssSelector(UiMap.DepositSelectControlCss)));
            selectElement.SelectByText(deposit.ToString().ToLower());
        }

        public void EnterCheckInDate(string date)
        {
            Driver.FindElement(By.CssSelector(UiMap.CheckInDateTextboxCss)).SendKeys(date);
        }

        public void EnterCheckOutDate(string date)
        {
            Driver.FindElement(By.CssSelector(UiMap.CheckOutDateTextboxCss)).SendKeys(date);
        }

        public void ClickSaveRecord()
        {
            Driver.FindElement(By.CssSelector(UiMap.SaveButtonCss)).Click();
        }

        public List<string> GetAllNames()
        {
            var nameElements = Driver.FindElements(By.CssSelector(UiMap.FirstnameColumnCss));

            return nameElements.Select(x => x.Text).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        public void DeleteRecordByName(string name)
        {
            var rowIndex = GetAllNames().IndexOf(name);
            Driver.FindElement(By.CssSelector(UiMap.GetDeleteButtonCssByRowIndex(rowIndex))).Click();
        }
    }

    class HotelBookingPageUiMap
    {
        public string HeadingCss => ".jumbotron h1";

        public string FirstnameTextboxCss => "#firstname";
        public string SurnameTextboxCss => "#lastname";
        public string PriceTextboxCss => "#totalprice";
        public string DepositSelectControlCss => "#depositpaid";
        public string CheckInDateTextboxCss => "#checkin";
        public string CheckOutDateTextboxCss => "#checkout";
        public string SaveButtonCss => "input[onclick=\"createBooking()\"]";

        public string FirstnameColumnCss => $"#bookings .row div:nth-child(1) p";
        public string GetDeleteButtonCssByRowIndex(int rowIndex)
        {
            return $"#bookings .row:nth-child({rowIndex + 2}) input[onclick*=\"deleteBooking\"]";
        }
    }
}
