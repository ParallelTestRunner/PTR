using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.Driver
{
    public interface ITestDriver
    {
        string ID { get; }
        void OpenURL(string URL);
        string URL { get; }
        void Quit();

        bool Exists(string cssSelector);
        bool IsVisible(string cssSelector);
        void Click(string cssSelector);
        void ClickButtonByText(string cssSelector, string text);
        string GetText(string cssSelector);
        void SendText(string cssSelector, string text);
        int Count(string cssSelector);
        List<string> AllValues(string cssSelector);

        void ClickOkButtonOnConfirmDialog();

        string WindowTitle { get; }
        object ExecuteJavaScript(string script);

        bool WaitUntil(Func<bool> condition, int maxWaitTimeInMilliseconds = -1);
    }
}
