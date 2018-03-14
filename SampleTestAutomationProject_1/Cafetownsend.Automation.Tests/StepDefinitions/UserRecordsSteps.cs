using Cafetownsend.Automation.POM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace Cafetownsend.Automation.Tests.StepDefinitions
{
    [Binding]
    public sealed class UserRecordsSteps
    {
        ScenarioContext _scenarioContext;
        public UserRecordsSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Then(@"I am on the record page")]
        public void IAmOnTheRecordPage()
        {
            Assert.IsTrue(WebManager.PageUserRecords.WaitUntil(() => WebManager.PageUserRecords.IsVisible), "Current webpage is not the user records one");
        }

        string _lastName = "NoLastName";
        string _startDate = "2005-06-05";
        string _email = "xyz@gmail.com";

        [Then(@"I can create a new record with a random name")]
        public void ICanCreateANewRecordWithARandomName()
        {
            var name = Utility.GetRandomString();
            _scenarioContext.Add("Name", name);
            Console.WriteLine($"Randomly chosen name: '{name}'");
            WebManager.PageUserRecords.CreateANewRecord(name, _lastName, _startDate, _email);
            string fullName = name + " " + _lastName;
            Assert.IsTrue(WebManager.PageUserRecords.AllNames.Contains(fullName), $"Newly created record with name '{fullName}' is not found in the list.");
        }

        [Then(@"I can also update that record")]
        public void ICanAlsoUpdateThatRecord()
        {
            string fullName = _scenarioContext.Get<string>("Name") + " " + _lastName;

            var newName = Utility.GetRandomString();
            _scenarioContext["Name"] = newName;
            Console.WriteLine($"Randomly chosen new name: '{newName}'");

            WebManager.PageUserRecords.UpdateRecordByFullName(fullName, newName);
            string newFullName = newName + " " + _lastName;
            Assert.IsTrue(WebManager.PageUserRecords.AllNames.Contains(newFullName), $"Newly created record with name '{newFullName}' is not found in the list.");
        }

        [Then(@"I can also delete that record")]
        public void ICanAlsoDeleteThatRecord()
        {
            string fullName = _scenarioContext.Get<string>("Name") + " " + _lastName;
            WebManager.PageUserRecords.DeleteRecordByFullName(fullName);
            Assert.IsTrue(!WebManager.PageUserRecords.AllNames.Contains(fullName), $"Record with name '{fullName}' still exists after deletion.");
        }
    }
}
