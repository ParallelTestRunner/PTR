using Cafetownsend.Automation.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.POM
{
    public interface IPageUserRecords : IWebComponent
    {
        int RecordCount { get; }

        List<string> AllNames { get; }

        void CreateANewRecord(string name, string lastName, string startDate, string email);

        void UpdateRecordByFullName(string fullName, string newName);

        void DeleteRecordByFullName(string fullName);
    }

    class PageUserRecords : PageBase, IPageUserRecords
    {
        private PageRecordsUIMap UIMap => (PageRecordsUIMap)_uiMap;

        public bool IsVisible => Driver.IsVisible(UIMap.LogoutButtonCss);

        public int RecordCount => Driver.Count(UIMap.RecordCss);

        public List<string> AllNames => Driver.AllValues(UIMap.RecordCss);

        public PageUserRecords(ITestDriver driver) : base(driver, new PageRecordsUIMap())
        { }

        public void CreateANewRecord(string name, string lastName, string startDate, string email)
        {
            Driver.Click(UIMap.AddButtonCss);
            Driver.SendText(UIMap.NameCss, name);
            Driver.SendText(UIMap.LastNameCss, lastName);
            Driver.SendText(UIMap.StartDateCss, startDate);
            Driver.SendText(UIMap.EmailCss, email);
            Driver.ClickButtonByText(UIMap.SubmitButtonCss, "Add");
        }

        public void UpdateRecordByFullName(string fullName, string newName)
        {
            var zeroBaseIndex = this.AllNames.IndexOf(fullName);
            Driver.Click(string.Format(UIMap.NthRecordCss, zeroBaseIndex + 1));
            Driver.Click(UIMap.EditButtonCss);

            Driver.SendText(UIMap.NameCss, newName);
            Driver.ClickButtonByText(UIMap.SubmitButtonCss, "Update");
        }

        public void DeleteRecordByFullName(string fullName)
        {
            var zeroBaseIndex = this.AllNames.IndexOf(fullName);
            Driver.Click(string.Format(UIMap.NthRecordCss, zeroBaseIndex + 1));
            Driver.Click(UIMap.DeleteButtonCss);
            Driver.ClickOkButtonOnConfirmDialog();
        }
    }

    class PageRecordsUIMap : UIMap
    {
        public string LogoutButtonCss => ".main-button";

        public string AddButtonCss => "#bAdd";
        public string NameCss => "input[ng-model*=\"firstName\"]";
        public string LastNameCss => "input[ng-model*=\"lastName\"]";
        public string StartDateCss => "input[ng-model*=\"startDate\"]";
        public string EmailCss => "input[type=\"email\"][ng-model*=\"email\"]";
        public string SubmitButtonCss => "button[type=\"submit\"]";

        public string EditButtonCss => "#bEdit";

        public string DeleteButtonCss => "#bDelete";

        public string RecordCss => "#employee-list li";

        public string NthRecordCss => "#employee-list li:nth-child({0})";
    }

    public class PageUserRecordsFactory
    {
        public static IPageUserRecords GetPageUserRecords(ITestDriver driver) => new PageUserRecords(driver);
    }
}
