using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class TestCase
	{
		public string Name { get; private set; }
		public string Class { get; private set; }
		public string FullName { get { return Class + "." + Name; } }
		public List<string> CategoriesItBelongsTo { get; private set; }

		public TestCase(string testCaseName, string testCaseClass, string strSemeicolonSeparatedTestCategoriesTestCaseBelongsTo)
		{
			this.Name = testCaseName;
			this.Class = testCaseClass;

			CategoriesItBelongsTo = !string.IsNullOrEmpty(strSemeicolonSeparatedTestCategoriesTestCaseBelongsTo) ?
											strSemeicolonSeparatedTestCategoriesTestCaseBelongsTo.Split(';').Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList() :
											new List<string>();
		}

		public new bool Equals(object testCase)
		{
			return (testCase is TestCase) && this.FullName == (testCase as TestCase).FullName;
		}
	}
}
