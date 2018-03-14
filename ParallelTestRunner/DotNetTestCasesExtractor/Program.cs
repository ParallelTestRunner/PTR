using Reflec;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DotNetTestCasesExtractor
{
    class Program
    {
        private static object _objLock = new object();
        private static bool _bAllowParameterizationOfTestObjects = false;

        static void Main(string[] args)
        {
            ILicenseChecker licenseChecker = new LicenseChecker();
            if (!licenseChecker.DoesThisMachineHaveAValidLicense())
            {
                Environment.Exit(0);
                return;
            }

            string strFile = args[0];
            string strXmlFileThatWillContainTestCasesOrTestAssemblies;
            if (Path.GetExtension(strFile).ToLower() == ".xml")
            {
                strXmlFileThatWillContainTestCasesOrTestAssemblies = args[0];
                strFile = Path.GetDirectoryName(strFile);
            }
            else
            {
                strXmlFileThatWillContainTestCasesOrTestAssemblies = args[1];
                if (args.Length > 2)
                {
                    _bAllowParameterizationOfTestObjects = Convert.ToBoolean(args[2]);
                }
            }

            //string strFile =
            //    @"C:\Users\aseem\Downloads\OnePlanner\3\NewOneP\Release\OnePlanner.CoreEngine.Tests.dll";
            //string strXmlFileThatWillContainTestCasesOrTestAssemblies =
            //    @"C:\Users\aseem\Downloads\OnePlanner\3\NewOneP\Release\OnePlanner.CoreEngine.TestsTC.xml";

            //string strFile =
            //    @"C:\Users\aseem\Desktop\ParallelTestExecution\NUnitTestProject_3\NUnitTestProject\bin\Debug\NUnitTestProject.dll";
            //string strXmlFileThatWillContainTestCasesOrTestAssemblies =
            //    @"C:\Users\aseem\Desktop\ParallelTestExecution\NUnitTestProject_3\NUnitTestProject\bin\Debug\NUnitTestProjectTC.xml";

            //string strFile = @"C:\Users\aseem\Desktop\ParallelTestExecution\NUnitTestProject_3\NUnitTestProject\bin\Debug\";
            //string strXmlFileThatWillContainTestCasesOrTestAssemblies = @"C:\Users\aseem\Desktop\ParallelTestExecution\NUnitTestProject_3\NUnitTestProject\bin\Debug\NUnitTestProjectTC.xml";

            try
            {
                if (!File.Exists(strFile))
                {
                    if (!Directory.Exists(strFile))
                    {
                        Environment.Exit(0);
                        return;
                    }
                    CreateXmlDocHavingTestingAssembliesRelativePaths(strFile, strXmlFileThatWillContainTestCasesOrTestAssemblies);
                    RunDotNetX(args[0], "");
                    Environment.Exit(0);
                    return;
                }

                if (File.Exists(strXmlFileThatWillContainTestCasesOrTestAssemblies))
                {
                    File.Delete(strXmlFileThatWillContainTestCasesOrTestAssemblies);
                }

                var assembly = Assembly.LoadFrom(strFile);
                CreateXmlDocHavingTestCases(assembly, strXmlFileThatWillContainTestCasesOrTestAssemblies);
                if (!File.Exists(strXmlFileThatWillContainTestCasesOrTestAssemblies))
                {
                    try
                    {
                        RunDotNetX(strFile, strXmlFileThatWillContainTestCasesOrTestAssemblies);
                    }
                    catch (Exception)
                    { }
                }
                Environment.Exit(0);
                return;
            }
            catch
            { }

            if (!File.Exists(strXmlFileThatWillContainTestCasesOrTestAssemblies))
            {
                try
                {
                    RunDotNetX(strFile, strXmlFileThatWillContainTestCasesOrTestAssemblies);
                }
                catch (Exception)
                { }
            }

            //if (!File.Exists(strXmlFileThatWillContainTestCases))
            //{
            //	var assembly = Assembly.UnsafeLoadFrom(strFile);
            //	CreateXmlDocHavingTestCases(assembly, strXmlFileThatWillContainTestCases);
            //}
            Environment.Exit(0);
        }

        private static void RunDotNetX(string strFile, string strXmlFileThatWillContainTestCasesOrTestAssemblies)
        {
            if (!System.Reflection.Assembly.GetExecutingAssembly().FullName.StartsWith("DotNetTestCasesExtractor"))
            {
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "DotNetX.exe";
            if (string.IsNullOrEmpty(strXmlFileThatWillContainTestCasesOrTestAssemblies))
            {
                psi.Arguments = "\"" + strFile + "\"";
            }
            else
            {
                psi.Arguments = "\"" + strFile + "\" " + "\"" + strXmlFileThatWillContainTestCasesOrTestAssemblies + "\" ";
            }
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process.Start(psi).WaitForExit();
        }

        private static List<string> _lstAssembliesHavingTestCases = new List<string>();
        private static void CreateXmlDocHavingTestingAssembliesRelativePaths(string strDirectory, string strXmlFileThatWillContainTestCasesOrTestAssemblies)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (!File.Exists(strXmlFileThatWillContainTestCasesOrTestAssemblies))
            {
                StringBuilder sbTestCasesXml = new StringBuilder("<?xml version='1.0' encoding='utf-8' ?>");
                sbTestCasesXml.AppendLine("<TestAssemblies></TestAssemblies>");
                xmlDoc.LoadXml(sbTestCasesXml.ToString());
            }
            else
            {
                xmlDoc.Load(strXmlFileThatWillContainTestCasesOrTestAssemblies);
                File.Delete(strXmlFileThatWillContainTestCasesOrTestAssemblies);
            }

            string strTestAssemblyNodeName = "TestAssembly";
            string strPathAttributeName = "Path";
            var testAssemblyNodes = (IEnumerable)xmlDoc.GetElementsByTagName(strTestAssemblyNodeName);
            foreach (XmlNode xmlNode in testAssemblyNodes)
            {
                _lstAssembliesHavingTestCases.Add(xmlNode.GetAttributeValue(strPathAttributeName));
            }

            GetFilesHavingTestCasesInGivenDirectory(new DirectoryInfo(strDirectory));

            xmlDoc.DocumentElement.RemoveAll();

            foreach (var strFile in _lstAssembliesHavingTestCases)
            {
                XmlNode xmlNodeForAssemblyHavingTestCases = xmlDoc.CreateNode(XmlNodeType.Element, strTestAssemblyNodeName, "");
                xmlNodeForAssemblyHavingTestCases.SetAttributeValue(strPathAttributeName, strFile);
                xmlDoc.DocumentElement.AppendChild(xmlNodeForAssemblyHavingTestCases);
            }

            xmlDoc.Save(strXmlFileThatWillContainTestCasesOrTestAssemblies);
        }

        private static DirectoryInfo GetFilesHavingTestCasesInGivenDirectory(DirectoryInfo directoryInfo)
        {
            var files = directoryInfo.GetFiles();
            Parallel.ForEach(files,
                                (currentFile) =>
                                {
                                    if ((!currentFile.FullName.ToLower().EndsWith(".dll") && !currentFile.FullName.ToLower().EndsWith(".exe")) ||
                                            _lstAssembliesHavingTestCases.Contains(currentFile.FullName))
                                    {
                                        return;
                                    }

                                    if (DoesThisFileHaveTestCases(currentFile))
                                    {
                                        lock (_lstAssembliesHavingTestCases)
                                        {
                                            _lstAssembliesHavingTestCases.Add(currentFile.FullName);
                                        }
                                    }
                                });

            foreach (DirectoryInfo diSubDir in directoryInfo.GetDirectories())
            {
                GetFilesHavingTestCasesInGivenDirectory(diSubDir);
            }

            return directoryInfo;
        }

        //private static bool DoesThisFileHaveTestCases(FileInfo fileInfo)
        //{
        //    try
        //    {
        //        var assembly = Assembly.LoadFrom(fileInfo.FullName);
        //        Type[] testClasses = assembly.GetTypes();
        //        foreach (var testClass in testClasses)
        //        {
        //            var classAttributes = testClass.GetCustomAttributes(true);
        //            if (classAttributes != null &&
        //                    classAttributes.Count() > 0 &&
        //                    classAttributes.Select(x => x.ToString()).Intersect(Config.TestCaseClassAttributes).Any())
        //            {
        //                return true;
        //            }
        //        }

        //        foreach (var testClass in testClasses)
        //        {
        //            if (testClass.IsPublic)
        //            {
        //                foreach (var testMethod in testClass.GetMethods())
        //                {
        //                    var methodAttributes = testMethod.GetCustomAttributes(true);
        //                    if (methodAttributes != null &&
        //                            methodAttributes.Length > 0 &&
        //                            methodAttributes.Select(x => x.ToString()).Intersect(Config.TestCaseMethodAttributes).Any())
        //                    {
        //                        return true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch(Exception exp)
        //    { }

        //    return false;
        //}

        private static bool DoesThisFileHaveTestCases(FileInfo fileInfo)
        {
            try
            {
                SeperateAppDomainAssemblyLoader seperateAppDomainAssemblyLoader = new SeperateAppDomainAssemblyLoader();
                return seperateAppDomainAssemblyLoader.DoesThisFileHaveTestCases(fileInfo);
            }
            catch
            { }

            return false;
        }

        private static void CreateXmlDocHavingTestCases(Assembly assembly, string strXmlFileThatWillContainTestCases)
        {
            StringBuilder sbTestCasesXml = new StringBuilder("<?xml version='1.0' encoding='utf-8' ?>");
            sbTestCasesXml.AppendLine("<TestCases>");

            var actions = GetActionsForAllTestClasses(sbTestCasesXml, assembly.GetTypes());

            //Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = 1 }, actions);
            Parallel.Invoke(actions);

            sbTestCasesXml.AppendLine("</TestCases>");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sbTestCasesXml.ToString());
            xmlDoc.Save(strXmlFileThatWillContainTestCases);
        }

        private static Action[] GetActionsForAllTestClasses(StringBuilder sbTestCasesXml, Type[] testClasses)
        {
            return testClasses.Select(testClass => new Action(() =>
            {
                var classAttributes = testClass.GetCustomAttributes(true);
                //if (classAttributes != null &&
                //        classAttributes.Count() > 0 &&
                //        classAttributes.Select(x => x.ToString()).Intersect(Config.TestCaseClassAttributes).Any())
                {
                    StringBuilder sbClassCategories = new StringBuilder();
                    List<string> namesOfTestClasses = new List<string>();
                    bool bIgnoreAllTestCasesInCurrentClass = false;
                    GetCategories_Names_And_Ignored_ValuesOfTestObject(testClass.FullName,
                                                                        classAttributes,
                                                                        sbClassCategories,
                                                                        namesOfTestClasses,
                                                                        ref bIgnoreAllTestCasesInCurrentClass);

                    var testClassSources = GetTestObjectSources(testClass, classAttributes, testClass.ToString(), testClasses).Distinct();
                    List<string> tempNamesOfTestClasses = new List<string>();
                    foreach (var testClassSource in testClassSources)
                    {
                        foreach (var nameOfTestClass in namesOfTestClasses)
                        {
                            tempNamesOfTestClasses.Add(nameOfTestClass + testClassSource);
                        }
                    }

                    if (tempNamesOfTestClasses.Count > 0)
                    {
                        namesOfTestClasses = tempNamesOfTestClasses;
                    }

                    foreach (var testMethod in testClass.GetMethods())
                    {
                        try
                        {
                            var methodAttributes = testMethod.GetCustomAttributes(true);
                            if (methodAttributes != null &&
                                    methodAttributes.Length > 0 &&
                                    methodAttributes.Select(x => x.ToString()).Intersect(Config.TestCaseMethodAttributes).Any())
                            {
                                StringBuilder sbSemicolonSeparatedCategories = new StringBuilder(sbClassCategories.ToString());
                                List<string> namesOfTestMethod = new List<string>();
                                bool bIgnore = bIgnoreAllTestCasesInCurrentClass;
                                GetCategories_Names_And_Ignored_ValuesOfTestObject(testMethod.Name,
                                                                                    methodAttributes,
                                                                                    sbSemicolonSeparatedCategories,
                                                                                    namesOfTestMethod,
                                                                                    ref bIgnore);

                                var testCaseSources = GetTestObjectSources(testClass,
                                                                            testMethod.GetCustomAttributes(true),
                                                                            testMethod.ToString(),
                                                                            testClasses).Distinct();

                                foreach (var nameOfTestClass in namesOfTestClasses)
                                {
                                    foreach (var nameOfTestCase in namesOfTestMethod)
                                    {
                                        if (testCaseSources.Count() > 0)
                                        {
                                            foreach (var testCaseSource in testCaseSources)
                                            {
                                                string strTempTestCaseName = testCaseSource.StartsWith("(") ? nameOfTestCase + testCaseSource : testCaseSource;
                                                string strTestCase = GetTestCaseNode(sbSemicolonSeparatedCategories, bIgnore, nameOfTestClass, strTempTestCaseName);
                                                lock (_objLock)
                                                {
                                                    sbTestCasesXml.AppendLine(strTestCase);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string strTestCase = GetTestCaseNode(sbSemicolonSeparatedCategories, bIgnore, nameOfTestClass, nameOfTestCase);
                                            lock (_objLock)
                                            {
                                                sbTestCasesXml.AppendLine(strTestCase);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        { }
                    }
                }
            })).ToArray();
        }

        private static void GetCategories_Names_And_Ignored_ValuesOfTestObject(string strTestObjectName,
                                                                               object[] testObjectAttributes,
                                                                               StringBuilder sbCategories,
                                                                               List<string> namesOfTestObject,
                                                                               ref bool bIgnoreTestObject)
        {
            if (testObjectAttributes != null)
            {
                foreach (var attribute in testObjectAttributes)
                {
                    try
                    {
                        sbCategories.Append(GetTestCategories(attribute));
                        if (Config.TestCaseIgnoreAttributes.Contains(attribute.ToString()))
                        {
                            bIgnoreTestObject = true;
                        }
                        string strParameterisedNameOfTestObject = GetParameterisedNameOfTestObject(strTestObjectName, attribute);
                        if (!string.IsNullOrEmpty(strParameterisedNameOfTestObject) && !namesOfTestObject.Contains(strParameterisedNameOfTestObject))
                        {
                            namesOfTestObject.Add(strParameterisedNameOfTestObject);
                        }
                    }
                    catch
                    { }
                }
            }

            if (namesOfTestObject.Count <= 0)
            {
                namesOfTestObject.Add(strTestObjectName);
            }
        }

        private static string AfterSpecialCharactersRemoved(string strText)
        {
            return strText.Replace("&", "&amp;")
                                .Replace("<", "&lt;")
                                .Replace(">", "&gt;")
                                .Replace("\"", "&quot;")
                                .Replace("\'", "&apos;")
                                .Replace("'", "&apos;");
        }

        private static StringBuilder GetTestCategories(object classOrMethodAttribute)
        {
            StringBuilder sbSemicolonSeparatedCategories = new StringBuilder();
            if (Config.TestCaseCategoryAttributes.Contains(classOrMethodAttribute.ToString()))
            {
                var properties = classOrMethodAttribute.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (Config.TestCaseCategoriesPropertyNames.Contains(property.Name))
                    {
                        object propertyValue = property.GetValue(classOrMethodAttribute);
                        if (propertyValue is string)
                        {
                            sbSemicolonSeparatedCategories.Append(propertyValue.ToString() + ";");
                        }
                        else
                        {
                            IEnumerable<object> categories = propertyValue as IEnumerable<object>;
                            if (categories != null)
                            {
                                foreach (var category in categories)
                                {
                                    sbSemicolonSeparatedCategories.Append(category.ToString() + ";");
                                }
                            }
                        }
                    }
                }
            }
            return sbSemicolonSeparatedCategories;
        }

        private static string GetCommaSeparatedArguments(object attribute)
        {
            StringBuilder sbArguments = new StringBuilder();
            var properties = attribute.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.Name == "Arguments")
                {
                    object propertyValue = property.GetValue(attribute);
                    IEnumerable<object> arguments = propertyValue as IEnumerable<object>;
                    if (arguments != null)
                    {
                        foreach (var argument in arguments)
                        {
                            sbArguments.Append(",");
                            sbArguments.Append(GetTestCaseArgumentWithRequiredStringFormatting(argument));
                        }
                        break;
                    }
                }
            }
            return sbArguments.ToString().Trim(',');
        }

        private static string GetParameterisedNameOfTestObject(string strTestObjectName, object testObjectAttribute)
        {
            if (!_bAllowParameterizationOfTestObjects)
            {
                return string.Empty;
            }

            var properties = testObjectAttribute.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.Name == "TestName")
                {
                    var testNameValue = property.GetValue(testObjectAttribute);
                    if (testNameValue != null && !string.IsNullOrEmpty(testNameValue.ToString()))
                    {
                        return GetTestCaseArgumentWithRequiredStringFormatting(testNameValue);
                    }
                    break;
                }
            }

            string strAguments = GetCommaSeparatedArguments(testObjectAttribute);
            if (!string.IsNullOrEmpty(strAguments))
            {
                return strTestObjectName + string.Format("({0})", strAguments);
            }
            return string.Empty;
        }

        private static List<string> GetTestObjectSources(Type testClass,
                                                         IEnumerable<object> customAttributesRelatedTestCaseOrTestClass,
                                                         string strNameOfRelatedTestCaseOrTestClass,
                                                         Type[] testClasses)
        {
            List<string> lstSourceValues = new List<string>();
            if (!_bAllowParameterizationOfTestObjects)
            {
                return lstSourceValues;
            }

            var testCaseSources = customAttributesRelatedTestCaseOrTestClass.Where(x => Config.TestCaseSourceAttributes.Contains(x.ToString()));
            if (testCaseSources.Count() <= 0)
            {
                return lstSourceValues;
            }

            var matchedSourceTypes = testCaseSources.First().GetType().GetProperties().Where(x => Config.TestCaseSourceTypes.Contains(x.Name));
            if (matchedSourceTypes.Count() <= 0)
            {
                return lstSourceValues;
            }

            var sourceType = matchedSourceTypes.First().GetValue(testCaseSources.First());
            if (sourceType != null && !string.IsNullOrEmpty(sourceType.ToString()))
            {
                testClass = testClasses.Where(x => x.FullName == sourceType.ToString()).First();
            }

            var matchedSourceNames = testCaseSources.First().GetType().GetProperties().Where(x => Config.TestCaseSourceNames.Contains(x.Name));

            string strSourceName = string.Empty;
            if (matchedSourceNames.Count() > 0)
            {
                var sourceObject = matchedSourceNames.First().GetValue(testCaseSources.First());
                if (sourceObject != null)
                {
                    strSourceName = sourceObject.ToString();
                }
            }

            if (string.IsNullOrEmpty(strSourceName))
            {
                strSourceName = "AnyStringValueThatDoesNotExists";
            }

            IEnumerable<object> parameterSources = null;
            var fields = testClass.GetRuntimeFields().Where(x => x.Name == strSourceName);
            if (fields.Count() > 0)
            {
                try
                {
                    parameterSources = fields.First().GetValue(testClass) as IEnumerable<object>;
                }
                catch
                { }
            }
            else
            {
                var properties = testClass.GetRuntimeProperties().Where(x => x.Name == strSourceName);
                if (properties.Count() >= 1)
                {
                    try
                    {
                        parameterSources = properties.First().GetValue(testClass) as IEnumerable<object>;
                    }
                    catch
                    { }
                }
                else
                {
                    if (testClass.GetMethods().Where(x => x.Name == "GetEnumerator").Count() > 0)
                    {
                        var testClassObject = Activator.CreateInstance(testClass);
                        if (testClassObject != null)
                        {
                            var enumerable = testClassObject as System.Collections.IEnumerable;
                            if (enumerable != null)
                            {
                                var enumerator = enumerable.GetEnumerator();
                                List<object> lst = new List<object>();
                                while (enumerator.MoveNext())
                                {
                                    lst.Add(enumerator.Current);
                                }

                                if (lst.Count > 0)
                                {
                                    parameterSources = lst;
                                }
                            }
                        }
                    }
                }
            }

            if (parameterSources != null)
            {
                FillListWithParameterVaues(lstSourceValues, parameterSources);
            }
            else
            {
                lstSourceValues.Add(strNameOfRelatedTestCaseOrTestClass.Substring(strNameOfRelatedTestCaseOrTestClass.IndexOf("(")));
            }

            return lstSourceValues;
        }

        private static void FillListWithParameterVaues(List<string> lstSourceValues, IEnumerable<object> parameterSources)
        {
            foreach (var value in parameterSources)
            {
                if (Config.TestCaseDataNames.Contains(value.ToString()))
                {
                    lstSourceValues.Add(GetParameterisedNameOfTestObject("", value));
                    continue;
                }

                var parameters = value as Array;
                if (parameters == null)
                {
                    lstSourceValues.Add(string.Format("({0})", GetTestCaseArgumentWithRequiredStringFormatting(value)));
                    continue;
                }

                StringBuilder sbParameters = new StringBuilder();
                foreach (var parameter in parameters)
                {
                    sbParameters.Append("," + GetTestCaseArgumentWithRequiredStringFormatting(parameter));
                }

                sbParameters.Append(")");
                string strParameters = "(" + sbParameters.ToString().Trim(',');
                if (!strParameters.Contains("["))
                {                    
                    lstSourceValues.Add(strParameters);
                }
            }
        }

        private static string GetTestCaseArgumentWithRequiredStringFormatting(object objTestCaseArgument)
        {
            if (objTestCaseArgument is string)
            {
                string strArgument = objTestCaseArgument.ToString();
                strArgument = strArgument.Replace("\"", "\\\"");
                strArgument = strArgument.Replace("\'", "\\\'");

                return "\"" + strArgument + "\"";
            }

            if (objTestCaseArgument == null)
            {
                return "";
            }
            return objTestCaseArgument.ToString();
        }

        private static string GetTestCaseNode(StringBuilder sbSemicolonSeparatedCategories, bool bIgnore, string nameOfTestClass, string strTempNameOfTestCase)
        {
            return string.Format("<TestCase TestClassFullName='{0}' TestCaseName='{1}' TestCategories='{2}' Ignore='{3}'></TestCase>",
                                                                                      AfterSpecialCharactersRemoved(nameOfTestClass),
                                                                                      AfterSpecialCharactersRemoved(strTempNameOfTestCase),
                                                                                      AfterSpecialCharactersRemoved(sbSemicolonSeparatedCategories.ToString().Trim(';')),
                                                                                      bIgnore.ToString());
        }
    }

    static class XmlExtensions
    {
        public static string GetAttributeValue(this XmlNode xmlNode, string strAttributeName)
        {
            if (xmlNode.Attributes[strAttributeName] != null)
            {
                return xmlNode.Attributes[strAttributeName].Value;
            }
            return null;
        }

        public static void SetAttributeValue(this XmlNode xmlNode, string strAttributeName, string strAttributeValue)
        {
            if (xmlNode.Attributes[strAttributeName] == null)
            {
                XmlAttribute xmlAttrbute = xmlNode.OwnerDocument.CreateAttribute(strAttributeName);
                xmlNode.Attributes.Append(xmlAttrbute);
            }
            xmlNode.Attributes[strAttributeName].Value = strAttributeValue;
        }
    }

    /// <span class="code-SummaryComment"><summary></span>
    /// Loads an assembly into a new AppDomain and obtains all the
    /// namespaces in the loaded Assembly, which are returned as a 
    /// List. The new AppDomain is then Unloaded.
    /// 
    /// This class creates a new instance of a 
    /// <span class="code-SummaryComment"><c>AssemblyLoader</c> class</span>
    /// which does the actual ReflectionOnly loading 
    /// of the Assembly into
    /// the new AppDomain.
    /// <span class="code-SummaryComment"></summary></span>
    public class SeperateAppDomainAssemblyLoader
    {
        #region Public Methods
        /// <span class="code-SummaryComment"><summary></span>
        /// Loads an assembly into a new AppDomain and obtains all the
        /// namespaces in the loaded Assembly, which are returned as a 
        /// List. The new AppDomain is then Unloaded
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="assemblyLocation">The Assembly file </span>
        /// location<span class="code-SummaryComment"></param></span>
        /// <span class="code-SummaryComment"><returns>A list of found namespaces</returns></span>
        public List<String> GetNamespaces(FileInfo assemblyLocation)
        {
            List<String> namespaces = new List<String>();

            //if (string.IsNullOrEmpty(assemblyLocation.Directory.FullName))
            //{
            //    throw new InvalidOperationException("Directory can't be null or empty.");
            //}

            //if (!Directory.Exists(assemblyLocation.Directory.FullName))
            //{
            //    throw new InvalidOperationException(
            //       string.Format(CultureInfo.CurrentCulture,
            //       "Directory not found {0}",
            //       assemblyLocation.Directory.FullName));
            //}

            AppDomain childDomain = BuildChildDomain(AppDomain.CurrentDomain);

            try
            {
                Type loaderType = typeof(AssemblyLoader);
                if (loaderType.Assembly != null)
                {
                    var loader = (AssemblyLoader)childDomain.CreateInstanceFrom(loaderType.Assembly.Location, loaderType.FullName).Unwrap();
                    loader.LoadAssembly(assemblyLocation.FullName);
                    namespaces = loader.GetNamespaces(assemblyLocation.Directory.FullName);
                }
                return namespaces;
            }
            finally
            {
                AppDomain.Unload(childDomain);
            }
        }

        public bool DoesThisFileHaveTestCases(FileInfo assemblyLocation)
        {
            AppDomain childDomain = BuildChildDomain(AppDomain.CurrentDomain);

            try
            {
                Type loaderType = typeof(AssemblyLoader);
                if (loaderType.Assembly != null)
                {
                    var loader = (AssemblyLoader)childDomain.CreateInstanceFrom(loaderType.Assembly.Location, loaderType.FullName).Unwrap();
                    loader.LoadAssemblyUsingAssemblyDotLoadFrom(assemblyLocation.FullName);
                    return loader.DoesThisFileHaveTestCases(assemblyLocation);
                }
            }
            finally
            {
                AppDomain.Unload(childDomain);
            }

            return false;
        }
        #endregion

        #region Private Methods
        /// <span class="code-SummaryComment"><summary></span>
        /// Creates a new AppDomain based on the parent AppDomains 
        /// Evidence and AppDomainSetup
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="parentDomain">The parent AppDomain</param></span>
        /// <span class="code-SummaryComment"><returns>A newly created AppDomain</returns></span>
        private AppDomain BuildChildDomain(AppDomain parentDomain)
        {
            Evidence evidence = new Evidence(parentDomain.Evidence);
            AppDomainSetup setup = parentDomain.SetupInformation;
            return AppDomain.CreateDomain("DiscoveryRegion", evidence, setup);
        }
        #endregion

        /// <span class="code-SummaryComment"><summary></span>
        /// Remotable AssemblyLoader, this class 
        /// inherits from <span class="code-SummaryComment"><c>MarshalByRefObject</c> </span>
        /// to allow the CLR to marshall
        /// this object by reference across 
        /// AppDomain boundaries
        /// <span class="code-SummaryComment"></summary></span>
        class AssemblyLoader : MarshalByRefObject
        {
            #region Private/Internal Methods
            /// <span class="code-SummaryComment"><summary></span>
            /// Gets namespaces for ReflectionOnly Loaded Assemblies
            /// <span class="code-SummaryComment"></summary></span>
            /// <span class="code-SummaryComment"><param name="path">The path to the Assembly</param></span>
            /// <span class="code-SummaryComment"><returns>A List of namespace strings</returns></span>
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            internal List<String> GetNamespaces(string path)
            {
                List<String> namespaces = new List<String>();

                DirectoryInfo directory = new DirectoryInfo(path);
                ResolveEventHandler resolveEventHandler = (s, e) =>
                {
                    return OnReflectionOnlyResolve(e, directory);
                };

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveEventHandler;

                Assembly reflectionOnlyAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().First();

                foreach (Type type in reflectionOnlyAssembly.GetTypes())
                {
                    if (!namespaces.Contains(type.Namespace))
                    {
                        namespaces.Add(type.Namespace);
                    }
                }

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;
                return namespaces;
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            internal bool DoesThisFileHaveTestCases(FileInfo assemblyLocation)
            {
                DirectoryInfo directory = new DirectoryInfo(assemblyLocation.Directory.FullName);
                ResolveEventHandler resolveEventHandler = (s, e) =>
                {
                    return OnReflectionOnlyResolve(e, directory);
                };

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveEventHandler;

                Assembly reflectionOnlyAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.Location == assemblyLocation.FullName).First();

                Type[] testClasses = reflectionOnlyAssembly.GetTypes();
                foreach (var testClass in testClasses)
                {
                    var classAttributes = testClass.GetCustomAttributes(true);
                    if (classAttributes != null &&
                            classAttributes.Count() > 0 &&
                            classAttributes.Select(x => x.ToString()).Intersect(Config.TestCaseClassAttributes).Any())
                    {
                        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;
                        return true;
                    }
                }

                foreach (var testClass in testClasses)
                {
                    if (testClass.IsPublic)
                    {
                        foreach (var testMethod in testClass.GetMethods())
                        {
                            var methodAttributes = testMethod.GetCustomAttributes(true);
                            if (methodAttributes != null &&
                                methodAttributes.Length > 0 &&
                                methodAttributes.Select(x => x.ToString()).Intersect(Config.TestCaseMethodAttributes).Any())
                            {
                                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;
                                return true;
                            }
                        }
                    }
                }

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;
                return false;
            }

            /// <span class="code-SummaryComment"><summary></span>
            /// Attempts ReflectionOnlyLoad of current 
            /// Assemblies dependants
            /// <span class="code-SummaryComment"></summary></span>
            /// <span class="code-SummaryComment"><param name="args">ReflectionOnlyAssemblyResolve </span>
            /// event args<span class="code-SummaryComment"></param></span>
            /// <span class="code-SummaryComment"><param name="directory">The current Assemblies </span>
            /// Directory<span class="code-SummaryComment"></param></span>
            /// <span class="code-SummaryComment"><returns>ReflectionOnlyLoadFrom loaded</span>
            /// dependant Assembly<span class="code-SummaryComment"></returns></span>
            private Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
            {

                Assembly loadedAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                                                                    .FirstOrDefault(asm =>
                                                                                    string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));

                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

                AssemblyName assemblyName = new AssemblyName(args.Name);
                string dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");

                if (File.Exists(dependentAssemblyFilename))
                {
                    return Assembly.ReflectionOnlyLoadFrom(dependentAssemblyFilename);
                }
                return Assembly.ReflectionOnlyLoad(args.Name);
            }

            /// <span class="code-SummaryComment"><summary></span>
            /// ReflectionOnlyLoad of single Assembly based on 
            /// the assemblyPath parameter
            /// <span class="code-SummaryComment"></summary></span>
            /// <span class="code-SummaryComment"><param name="assemblyPath">The path to the Assembly</param></span>
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            internal void LoadAssembly(String assemblyPath)
            {
                try
                {
                    Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                }
                catch (FileNotFoundException)
                {
                    /* Continue loading assemblies even if an assembly
                     * can not be loaded in the new AppDomain. */
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            internal void LoadAssemblyUsingAssemblyDotLoadFrom(String assemblyPath)
            {
                try
                {
                    Assembly.LoadFrom(assemblyPath);
                }
                catch (FileNotFoundException)
                {
                    /* Continue loading assemblies even if an assembly
                     * can not be loaded in the new AppDomain. */
                }
            }
            #endregion
        }
    }
}
