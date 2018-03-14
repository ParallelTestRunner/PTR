using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.Driver
{
    class TestDriver : ITestDriver
    {
        private string _id = Guid.NewGuid().ToString();
        public string ID { get { return _id; } }

        private IWebDriver _iWebdriver;
        private int _defaultExplilicitWaitTimeOutInMilliseconds;

        public TestDriver(IWebDriver iWebdriver,
                            int defaultExplilicitWaitTimeOutInMilliseconds)
        {
            _iWebdriver = iWebdriver;
            _defaultExplilicitWaitTimeOutInMilliseconds = defaultExplilicitWaitTimeOutInMilliseconds;
        }

        public string URL { get { return _iWebdriver.Url; } }

        public string WindowTitle { get { return _iWebdriver.Title; } }

        public void Click(string cssSelector)
        {
            var webDriverWait = new WebDriverWait(_iWebdriver, TimeSpan.FromSeconds(10));
            webDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(cssSelector)));
            _iWebdriver.FindElement(By.CssSelector(cssSelector)).Click();
        }

        public void ClickButtonByText(string cssSelector, string text)
        {
            this.WaitUntil(() =>
            {
                var elements = _iWebdriver.FindElements(By.CssSelector(cssSelector));
                if (elements.Count() < 1)
                {
                    return false;
                }
                return elements.Where(x => x.Text == text && x.Displayed).Count() > 0;
            });

            _iWebdriver.FindElements(By.CssSelector(cssSelector)).Where(x => x.Text == text && x.Displayed).First().Click();
        }

        public object ExecuteJavaScript(string script)
        {
            return ((IJavaScriptExecutor)_iWebdriver).ExecuteScript(script);
        }

        public bool Exists(string cssSelector)
        {
            try
            {
                _iWebdriver.FindElement(By.CssSelector(cssSelector));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public string GetText(string cssSelector)
        {
            return _iWebdriver.FindElement(By.CssSelector(cssSelector)).Text;
        }

        public bool IsVisible(string cssSelector)
        {
            return Exists(cssSelector) && _iWebdriver.FindElement(By.CssSelector(cssSelector)).Displayed;
        }

        public void OpenURL(string URL)
        {
            _iWebdriver.Navigate().GoToUrl(URL);
        }

        public void Quit()
        {
            _iWebdriver.Quit();
        }

        public void SendText(string cssSelector, string text)
        {
            try
            {
                _iWebdriver.FindElement(By.CssSelector(cssSelector)).Clear();
            }
            catch
            { }

            _iWebdriver.FindElement(By.CssSelector(cssSelector)).SendKeys(text);
        }

        private int _pollingInterval = 100;
        public bool WaitUntil(Func<bool> condition, int maxWaitTimeInMilliseconds = -1)
        {
            if (maxWaitTimeInMilliseconds == -1)
            {
                maxWaitTimeInMilliseconds = _defaultExplilicitWaitTimeOutInMilliseconds;
            }

            var webDriverWait = new WebDriverWait(_iWebdriver, TimeSpan.FromMilliseconds(maxWaitTimeInMilliseconds));
            webDriverWait.PollingInterval = TimeSpan.FromMilliseconds(_pollingInterval);
            Func<IWebDriver, bool> formattedCondition = (_iWebdriver => condition());
            bool result = false;
            try
            {
                result = webDriverWait.Until(formattedCondition);
            }
            catch
            { }
            return result;
        }

        public void ClickOkButtonOnConfirmDialog()
        {
            _iWebdriver.SwitchTo().Alert().Accept();
        }

        public int Count(string cssSelector)
        {
            return _iWebdriver.FindElements(By.CssSelector(cssSelector)).Count;
        }

        public List<string> AllValues(string cssSelector)
        {
            return _iWebdriver.FindElements(By.CssSelector(cssSelector)).Select(x => x.Text.Trim()).ToList();
        }
    }
}
