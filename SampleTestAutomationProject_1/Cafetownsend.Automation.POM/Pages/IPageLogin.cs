using Cafetownsend.Automation.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.POM
{
    public interface IPageLogin : IWebComponent
    {
        void EnterUsername(string userName);
        void EnterPassword(string password);
        void ClickSubmitButton();
        string ErrorMessage { get; }
    }

    class PageLogin : PageBase, IPageLogin
    {
        private PageLoginUIMap UIMap => (PageLoginUIMap)_uiMap;
        public PageLogin(ITestDriver driver) : base(driver, new PageLoginUIMap())
        { }

        public bool IsVisible => Driver.IsVisible(UIMap.UsernameCss);

        public void EnterUsername(string userName) => Driver.SendText(UIMap.UsernameCss, userName);

        public void EnterPassword(string password) => Driver.SendText(UIMap.PasswordCss, password);

        public void ClickSubmitButton() => Driver.Click(UIMap.SubmitButtonCss);

        public string ErrorMessage => Driver.GetText(UIMap.InvalidUsernamePasswordMessageCss);
    }

    class PageLoginUIMap : UIMap
    {
        public string UsernameCss => "input[ng-model='user.name']";
        public string PasswordCss => "input[ng-model='user.password']";
        public string SubmitButtonCss => "button[type=submit]";

        public string InvalidUsernamePasswordMessageCss => ".error-message";
    }

    public class PageLoginFactory
    {
        public static IPageLogin GetPageLogin(ITestDriver driver) => new PageLogin(driver);
    }
}
