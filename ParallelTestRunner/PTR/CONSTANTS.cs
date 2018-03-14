using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class CONSTANTS
	{
		#region Configurations related constants
		public const string NoStringValueProvided = "NoStringValueProvided";
		public const int NoIntValueProvided = -999;
		public const string SeparatorBetweenExecutableFilePathAndItsCommandLineParameter = "@Param";
		#endregion

		#region TestConfigManager related constants
		public const string IntermediateTestResultFileNamePrefix = "TestResults_";
		public const string IntermediateTestResultFileNamePrefixForFailedTestCases = "TestResults_F_";
		public const string ConsolidatedFileName = "Consolidated";
		public const string ThreadExecutionFolderPrefix = "Thread_";
		public const string ThreadExecutionFolderPrefixForFailedTestCases = "Thread_F_";
		#endregion

		public const string ConfigFileExtension = ".config";
		public const string AfterThisStringAllFurtherParametersAreTestCasesOnly = " AfterThisStringAllFurtherParametersAreTestCasesOnly ";

        public const string FileHavingTestAssemblies = "TestAssemblies.xml";
        public const string FileHavingTestCases = "TC.xml";

        public static readonly string TimeStampWhenPTRWasLaunched;
        static CONSTANTS()
        {
            TimeStampWhenPTRWasLaunched = DateTime.Now.ToString("yyyy_MM_dd HH-mm-ss");
        }
    }
}
