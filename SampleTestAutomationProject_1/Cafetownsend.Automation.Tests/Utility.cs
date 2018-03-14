using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cafetownsend.Automation.Tests
{
    public static class Utility
    {
        public static string GetRandomString()
        {
            var random = new Random();
            int stringLength = random.Next(5, 10);

            int numberOfChars = 'z' - 'a' + 1;
            Func<char> alphabetGenerator = () => random.Next(6) == 5 ? ' ' : (char)(random.Next(numberOfChars) + 'a');
            string randomString = string.Join(string.Empty, Enumerable.Range(1, stringLength).Select(x => alphabetGenerator()));
            randomString = new Regex("[ ]{2,}", RegexOptions.None).Replace(randomString, " ").Trim();

            if (randomString.Length < 3)
            {
                randomString = GetRandomString();
            }

            return randomString.First().ToString().ToUpper() + randomString.Substring(1);
        }
    }
}
