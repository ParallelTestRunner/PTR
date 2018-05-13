using HotelBookingTests.PageObjectModel;
using NUnit.Framework;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace HotelBookingTests.Steps
{
    [Binding]
    public sealed class HotelBookingSteps
    {
        [Given(@"I open the Hotel Booking website")]
        public void IOpenTheHotelBookingWebsite()
        {
            WebManager.OpenWebpage(Config.WebsiteURL);
        }

        [When(@"I enter (.*) as the Firstname")]
        public void IEnterTheFirstname(string firstName)
        {
            ScenarioContext.Current.Add("FirstName", firstName);
            WebManager.HotelBookingPage.EnterFirstName(firstName);
        }

        [When(@"I enter (.*) as the Surname")]
        public void IEnterAsTheSurname(string surname)
        {
            WebManager.HotelBookingPage.EnterSurname(surname);
        }

        [When(@"I enter (.*) as the Price")]
        public void IEnterThePrice(int price)
        {
            WebManager.HotelBookingPage.EnterPrice(price);
        }

        [When(@"I set (.*) as the Deposit")]
        public void ISetDeposit(bool deposit)
        {
            WebManager.HotelBookingPage.SetDeposit(deposit);
        }

        [When(@"I enter (.*) as the Check-in date")]
        public void IEnterTheCheckInDate(string checkInDate)
        {
            WebManager.HotelBookingPage.EnterCheckInDate(checkInDate);
        }

        [When(@"I enter (.*) as the Check-out date")]
        public void IEnterTheCheckOutDate(string checkOutDate)
        {
            WebManager.HotelBookingPage.EnterCheckOutDate(checkOutDate);
        }

        [When(@"I click the Save button")]
        public void IClickTheSaveButton()
        {
            var allNames = WebManager.HotelBookingPage.GetAllNames();

            ScenarioContext.Current.Add("RecordCount", allNames.Count);

            WebManager.HotelBookingPage.ClickSaveRecord();
        }

        [Then(@"The record should have get saved")]
        public void TheRecordShouldHaveGetSaved()
        {
            var recordCountBeforeSavingCurrentBooking = ScenarioContext.Current.Get<int>("RecordCount");
            WaitUntil(() => WebManager.HotelBookingPage.GetAllNames().Count > recordCountBeforeSavingCurrentBooking);

            var allNames = WebManager.HotelBookingPage.GetAllNames();
            var firstName = ScenarioContext.Current.Get<string>("FirstName");

            Assert.IsTrue(allNames.Contains(firstName), $"First name \"{firstName}\" is not found in the booking table");
        }

        [When(@"I find out a record with (.*) as Firstname")]
        public void IFindOutARecordWithAsFirstname(string firstName)
        {
            var names = WebManager.HotelBookingPage.GetAllNames();
            var indexOfNameToBeDeleted = names.IndexOf(firstName);
            Assert.IsTrue(indexOfNameToBeDeleted != -1, $"No record is found with first name: \"{firstName}\"");

            ScenarioContext.Current.Add("FirstName", firstName);
        }

        [When(@"I click the Delete button corresponding to that record")]
        public void IClickTheDeleteButtonCorrespondingToThatRecord()
        {
            var allNames = WebManager.HotelBookingPage.GetAllNames();
            ScenarioContext.Current.Add("RecordCount", allNames.Count);

            var firstName = ScenarioContext.Current.Get<string>("FirstName");
            WebManager.HotelBookingPage.DeleteRecordByName(firstName);
        }

        [Then(@"That record should get deleted")]
        public void ThatRecordShouldGetDeleted()
        {
            var recordCountBeforeDeletingTheRecord = ScenarioContext.Current.Get<int>("RecordCount");
            WaitUntil(() => WebManager.HotelBookingPage.GetAllNames().Count < recordCountBeforeDeletingTheRecord);

            var allNames = WebManager.HotelBookingPage.GetAllNames();
            Assert.IsTrue(allNames.Count == recordCountBeforeDeletingTheRecord -1, $"Records count is same before and after deletion");
        }

        private bool WaitUntil(Func<bool> condition, int maxWaitTimeInSeconds = 10, int pollingInterval = 300)
        {
            long maxWaitTimeInMilliseconds = maxWaitTimeInSeconds * 1000;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bool conditionIsCheckedAtLeastOnce = false;
            do
            {
                if (conditionIsCheckedAtLeastOnce)
                {
                    // This sleep is to avoid checking the condition too often as condition may need some time to get true and
                    // so would be efficient to have a short interval between any two successive checks
                    System.Threading.Thread.Sleep(pollingInterval);
                }

                if (condition())
                {
                    return true;
                }

                conditionIsCheckedAtLeastOnce = true;
            }
            while (stopWatch.ElapsedMilliseconds < maxWaitTimeInMilliseconds);

            return false;
        }
    }
}
