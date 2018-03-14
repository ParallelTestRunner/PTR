using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetTestCasesExtractor
{
	class Config
	{
		private static List<string> _lstTestCaseClassAttributes;
		public static List<string> TestCaseClassAttributes { get { return _lstTestCaseClassAttributes; } }

		private static List<string> _lstTestCaseMethodAttributes;
		public static List<string> TestCaseMethodAttributes { get { return _lstTestCaseMethodAttributes; } }

		private static List<string> _lstTestCaseCategoryAttributes;
		public static List<string> TestCaseCategoryAttributes { get { return _lstTestCaseCategoryAttributes; } }

		private static List<string> _lstTestCaseCategoriesPropertyName;
		public static List<string> TestCaseCategoriesPropertyNames { get { return _lstTestCaseCategoriesPropertyName; } }

		private static List<string> _lstTestCaseIgnoreAttributes;
		public static List<string> TestCaseIgnoreAttributes { get { return _lstTestCaseIgnoreAttributes; } }

		private static List<string> _lstTestCaseSourceAttributes;
		public static List<string> TestCaseSourceAttributes { get { return _lstTestCaseSourceAttributes; } }

		private static List<string> _lstTestCaseSourceNames;
		public static List<string> TestCaseSourceNames { get { return _lstTestCaseSourceNames; } }

		private static List<string> _lstTestCaseSourceTypes;
		public static List<string> TestCaseSourceTypes { get { return _lstTestCaseSourceTypes; } }

		private static List<string> _lstTestCaseDataNames;
		public static List<string> TestCaseDataNames { get { return _lstTestCaseDataNames; } }

        static Config()
        {
            FillTestCaseClassAttribute();
            FillTestCaseMethodAttribute();
            FillTestCaseCategoryAttribute();
            FillTestCaseCategoriesPropertyName();
            FillTestCaseIgnoreAttribute();

            FillTestCaseSourceAttribute();
            FillTestCaseSourceName();
            FillTestCaseSourceType();
            FillTestCaseDataName();
        }

        private static void FillTestCaseClassAttribute()
        {
            _lstTestCaseClassAttributes = new List<string>()
            {
                "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute",
                "Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute",
                "NUnit.Framework.TestFixtureAttribute",
                "NUnit.Framework.TestFixtureSourceAttribute"
            };
        }

        private static void FillTestCaseMethodAttribute()
        {
            _lstTestCaseMethodAttributes = new List<string>()
            {
                "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute",
                "NUnit.Framework.TestAttribute",
                "NUnit.Framework.TestCaseAttribute",
                "NUnit.Framework.TestCaseSourceAttribute",
                "NUnit.Framework.TestFixtureSourceAttribute"
            };
        }

        private static void FillTestCaseCategoryAttribute()
        {
            _lstTestCaseCategoryAttributes = new List<string>()
            {
                "Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute",
                "NUnit.Framework.CategoryAttribute"
            };
        }

        private static void FillTestCaseCategoriesPropertyName()
        {
            _lstTestCaseCategoriesPropertyName = new List<string>()
            {
                "TestCategories",
                "Name"
            };
        }

        private static void FillTestCaseIgnoreAttribute()
        {
            _lstTestCaseIgnoreAttributes = new List<string>()
            {
                "Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute",
                "NUnit.Framework.IgnoreAttribute"
            };
        }

        private static void FillTestCaseSourceAttribute()
        {
            _lstTestCaseSourceAttributes = new List<string>()
            {
                "NUnit.Framework.TestCaseSourceAttribute",
                "NUnit.Framework.TestFixtureSourceAttribute"
            };
        }

        private static void FillTestCaseSourceName()
        {
            _lstTestCaseSourceNames = new List<string>()
            {
                "SourceName"
            };
        }

        private static void FillTestCaseSourceType()
        {
            _lstTestCaseSourceTypes = new List<string>()
            {
                "SourceType"
            };
        }

        private static void FillTestCaseDataName()
        {
            _lstTestCaseDataNames = new List<string>()
            {
                "NUnit.Framework.TestCaseData",
                "NUnit.Framework.TestFixtureData"
            };
        }

        //static Config()
        //{
        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseClassAttributes, "TestCaseClassAttribute");
        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseMethodAttributes, "TestCaseMethodAttribute");
        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseCategoryAttributes, "TestCaseCategoryAttribute");
        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseCategoriesPropertyName, "TestCaseCategoriesPropertyName");
        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseIgnoreAttributes, "TestCaseIgnoreAttribute");

        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseSourceAttributes, "TestCaseSourceAttribute");
        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseSourceNames, "TestCaseSourceName");
        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseSourceTypes, "TestCaseSourceType");
        //	FillTestCaseObjectsOfGivenType(ref _lstTestCaseDataNames, "TestCaseDataName");
        //}

        //static void FillTestCaseObjectsOfGivenType(ref List<string> lst, string strTestingFrameworkObject)
		//{
		//	var TestingFrameworksSection = (TestingFrameworksSection)ConfigurationManager.GetSection("TestingFrameworks");
		//	lst = TestingFrameworksSection.TestingFrameworkEntries.Select(x => x[strTestingFrameworkObject]).Distinct().ToList();
		//}
	}

	//class TestingFrameworksSection : ConfigurationSection
	//{
	//	[ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
	//	[ConfigurationCollection(typeof(TestingFramework))]
	//	public TestingFrameworkEntries TestingFrameworkEntries
	//	{
	//		get { return (TestingFrameworkEntries)this[""]; }
	//		set { this[""] = value; }
	//	}
	//}

	//class TestingFrameworkEntries : ConfigurationElementCollection, IEnumerable<TestingFramework>
	//{
	//	protected override ConfigurationElement CreateNewElement()
	//	{
	//		return new TestingFramework();
	//	}

	//	protected override object GetElementKey(ConfigurationElement element)
	//	{
	//		return ((TestingFramework)element).TestCaseClassAttribute;
	//	}

	//	IEnumerator<TestingFramework> IEnumerable<TestingFramework>.GetEnumerator()
	//	{
	//		foreach (var key in this.BaseGetAllKeys()) { yield return (TestingFramework)BaseGet(key); }
	//	}
	//}

	//class TestingFramework : ConfigurationSection
	//{
	//	[ConfigurationProperty("TestCaseClassAttribute", IsRequired = true)]
	//	public string TestCaseClassAttribute { get { return (string)base["TestCaseClassAttribute"]; } }

	//	[ConfigurationProperty("TestCaseMethodAttribute", IsRequired = true)]
	//	public string TestCaseMethodAttribute { get { return (string)base["TestCaseMethodAttribute"]; } }

	//	[ConfigurationProperty("TestCaseCategoryAttribute", IsRequired = true)]
	//	public string TestCaseCategoryAttribute { get { return (string)base["TestCaseCategoryAttribute"]; } }

	//	[ConfigurationProperty("TestCaseCategoriesPropertyName", IsRequired = true)]
	//	public string TestCaseCategoriesPropertyName { get { return (string)base["TestCaseCategoriesPropertyName"]; } }

	//	[ConfigurationProperty("TestCaseIgnoreAttribute", IsRequired = false)]
	//	public string TestCaseIgnoreAttribute { get { return (string)base["TestCaseIgnoreAttribute"]; } }

	//	[ConfigurationProperty("TestCaseSourceAttribute", IsRequired = false)]
	//	public string TestCaseSourceAttribute { get { return (string)base["TestCaseSourceAttribute"]; } }

	//	[ConfigurationProperty("TestCaseSourceName", IsRequired = false)]
	//	public string TestCaseSourceName { get { return (string)base["TestCaseSourceName"]; } }

	//	[ConfigurationProperty("TestCaseSourceType", IsRequired = false)]
	//	public string TestCaseSourceType { get { return (string)base["TestCaseSourceType"]; } }

	//	[ConfigurationProperty("TestCaseDataName", IsRequired = false)]
	//	public string TestCaseDataName { get { return (string)base["TestCaseDataName"]; } }

	//	new public string this[string propertyName]
	//	{
	//		get
	//		{
	//			var property = this.GetType().GetProperty(propertyName); ;
	//			return property.GetValue(this, null).ToString();
	//		}
	//	}
	//}
}

//<? xml version="1.0" encoding="utf-8" ?>
//<configuration>
//	<configSections>
//		<section name = "TestingFrameworks" type="DotNetTestCasesExtractor.TestingFrameworksSection, DotNetTestCasesExtractor"/>
//	</configSections>
	
//	<runtime>
//		<loadFromRemoteSources enabled = "true" />

//    </ runtime >


//    < TestingFrameworks >

//        < add TestCaseClassAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute"
//			  TestCaseMethodAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"
//			  TestCaseCategoryAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute"
//			  TestCaseCategoriesPropertyName="TestCategories"
//			  TestCaseIgnoreAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestCaseSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestCaseData" />

//		<add TestCaseClassAttribute = "Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute"

//              TestCaseMethodAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"
//			  TestCaseCategoryAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute"
//			  TestCaseCategoriesPropertyName="TestCategories"
//			  TestCaseIgnoreAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestCaseSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestCaseData" />

//		<add TestCaseClassAttribute = "NUnit.Framework.TestFixtureAttribute"

//              TestCaseMethodAttribute="NUnit.Framework.TestAttribute"
//			  TestCaseCategoryAttribute="NUnit.Framework.CategoryAttribute"
//			  TestCaseCategoriesPropertyName="Name"
//			  TestCaseIgnoreAttribute="NUnit.Framework.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestCaseSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestCaseData" />

//		<add TestCaseClassAttribute = "NUnit.Framework.TestFixtureSourceAttribute"

//              TestCaseMethodAttribute="NUnit.Framework.TestAttribute"
//			  TestCaseCategoryAttribute="NUnit.Framework.CategoryAttribute"
//			  TestCaseCategoriesPropertyName="Name"
//			  TestCaseIgnoreAttribute="NUnit.Framework.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestFixtureSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestFixtureData" />

//    <add TestCaseClassAttribute = "NUnit.Framework.TestFixtureSourceAttribute"

//              TestCaseMethodAttribute="NUnit.Framework.TestCaseAttribute"
//			  TestCaseCategoryAttribute="NUnit.Framework.CategoryAttribute"
//			  TestCaseCategoriesPropertyName="Name"
//			  TestCaseIgnoreAttribute="NUnit.Framework.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestFixtureSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestFixtureData" />
    
//    <!--<add TestCaseMethodAttribute = "Xunit.FactAttribute" / -->

//    </ TestingFrameworks >

//</ configuration >












//    <? xml version="1.0" encoding="utf-8" ?>
//<configuration>
//	<configSections>
//		<section name = "TestingFrameworks" type="DotNetTestCasesExtractor.TestingFrameworksSection, DotNetX"/>
//	</configSections>
	
//	<runtime>
//		<loadFromRemoteSources enabled = "true" />

//    </ runtime >


//    < TestingFrameworks >

//        < add TestCaseClassAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute"
//			  TestCaseMethodAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"
//			  TestCaseCategoryAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute"
//			  TestCaseCategoriesPropertyName="TestCategories"
//			  TestCaseIgnoreAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestCaseSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestCaseData" />

//		<add TestCaseClassAttribute = "Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute"

//              TestCaseMethodAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"
//			  TestCaseCategoryAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute"
//			  TestCaseCategoriesPropertyName="TestCategories"
//			  TestCaseIgnoreAttribute="Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestCaseSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestCaseData" />

//		<add TestCaseClassAttribute = "NUnit.Framework.TestFixtureAttribute"

//              TestCaseMethodAttribute="NUnit.Framework.TestAttribute"
//			  TestCaseCategoryAttribute="NUnit.Framework.CategoryAttribute"
//			  TestCaseCategoriesPropertyName="Name"
//			  TestCaseIgnoreAttribute="NUnit.Framework.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestCaseSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestCaseData" />

//		<add TestCaseClassAttribute = "NUnit.Framework.TestFixtureSourceAttribute"

//              TestCaseMethodAttribute="NUnit.Framework.TestAttribute"
//			  TestCaseCategoryAttribute="NUnit.Framework.CategoryAttribute"
//			  TestCaseCategoriesPropertyName="Name"
//			  TestCaseIgnoreAttribute="NUnit.Framework.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestFixtureSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestFixtureData" />

//    <add TestCaseClassAttribute = "NUnit.Framework.TestFixtureSourceAttribute"

//              TestCaseMethodAttribute="NUnit.Framework.TestCaseAttribute"
//			  TestCaseCategoryAttribute="NUnit.Framework.CategoryAttribute"
//			  TestCaseCategoriesPropertyName="Name"
//			  TestCaseIgnoreAttribute="NUnit.Framework.IgnoreAttribute"
			  
//			  TestCaseSourceAttribute="NUnit.Framework.TestFixtureSourceAttribute"
//			  TestCaseSourceName="SourceName"
//			  TestCaseSourceType="SourceType"
//			  TestCaseDataName="NUnit.Framework.TestFixtureData" />
//	</TestingFrameworks>

//</configuration>
