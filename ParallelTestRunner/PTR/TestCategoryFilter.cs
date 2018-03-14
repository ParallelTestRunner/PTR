using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class TestCategoryFilter
	{
		private static Dictionary<string, TestCategoryFilter> _dicTestCategoryFilterByStringOfTestCategories
																				= new Dictionary<string, TestCategoryFilter>();

		public static TestCategoryFilter GetTestCategoryFilter(string strTestCategories)
		{
			TestCategoryFilter testCategoryFilter = null;
			lock (_dicTestCategoryFilterByStringOfTestCategories)
			{
				if (_dicTestCategoryFilterByStringOfTestCategories.ContainsKey(strTestCategories))
				{
					testCategoryFilter = _dicTestCategoryFilterByStringOfTestCategories[strTestCategories];
				}
				else
				{
					testCategoryFilter = new TestCategoryFilter(strTestCategories);
					_dicTestCategoryFilterByStringOfTestCategories.Add(strTestCategories, testCategoryFilter);
				}
			}
			return testCategoryFilter;
		}

		class TestCategoryGroup
		{
			public List<string> MustHaveTestCategories = new List<string>();
			public List<string> MustNotHaveTestCategories = new List<string>();

			public bool IsTestCaseToBeConsidered(TestCase testCase)
			{
				return (MustHaveTestCategories.Count <= 0 || testCase.CategoriesItBelongsTo.ContainsAll(MustHaveTestCategories)) &&
							(MustNotHaveTestCategories.Count <= 0 || !testCase.CategoriesItBelongsTo.Intersect(MustNotHaveTestCategories).Any());
			}
		}

		List<TestCategoryGroup> _lstTestCategoryGroup = new List<TestCategoryGroup>();

		TestCategoryFilter(string strTestCategory)
		{
			var testCategoryGroups = strTestCategory.Split('|').Where(x => !string.IsNullOrEmpty(x.Trim())).Select(x => x.Trim()).Distinct();
			foreach (var strTestCategories in testCategoryGroups)
			{
				TestCategoryGroup testCategoryGroup = new TestCategoryGroup();
				_lstTestCategoryGroup.Add(testCategoryGroup);

				var testCategories = strTestCategories.Split('^').Where(x => !string.IsNullOrEmpty(x.Trim())).Select(x => x.Trim()).Distinct();
				foreach (var testCategory in testCategories)
				{
					if (testCategory.StartsWith("!"))
					{
						string strTempTestCategory = testCategory.Substring(1).Trim();
						if (!string.IsNullOrEmpty(strTempTestCategory))
						{
							testCategoryGroup.MustNotHaveTestCategories.Add(strTempTestCategory);
						}
					}
					else
					{
						testCategoryGroup.MustHaveTestCategories.Add(testCategory);
					}
				}
			}
		}

		public bool IsTestCaseToBeConsidered(TestCase testCase)
		{
			return _lstTestCategoryGroup.Count <= 0 || _lstTestCategoryGroup.Any(x => x.IsTestCaseToBeConsidered(testCase));
		}
	}
}
