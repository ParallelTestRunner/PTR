using Cafetownsend.Automation.Configurations;
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
    public sealed class LoginSteps
    {
        [Given(@"I open the login page of the website")]
        public void IOpenTheLoginPageOfTheWebsite()
        {
            WebManager.OpenWebpage(Config.WebsiteURL);
            Assert.IsTrue(WebManager.PageLogin.WaitUntil(() => WebManager.PageLogin.IsVisible), "Login page is not visible");
        }

        [When(@"I enter (.*) as user name")]
        public void IEnterAsUserName(string userName)
        {
            WebManager.PageLogin.EnterUsername(userName);
        }

        [When(@"I enter (.*) as my password")]
        public void IEnterAsMyPassword(string password)
        {
            WebManager.PageLogin.EnterPassword(password);
        }

        [When(@"I click the login button")]
        public void IClickTheLoginButton()
        {
            WebManager.PageLogin.ClickSubmitButton();
        }

        [Then(@"I can see the message (.*)")]
        public void ICanSeeTheMessage(string message)
        {
            Assert.IsTrue(WebManager.PageLogin.WaitUntil(() => WebManager.PageLogin.ErrorMessage == message),
                            $"Expected error message: {message}, Actual one: {WebManager.PageLogin.ErrorMessage}");
        }
    }
}
