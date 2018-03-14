using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class LogUtil : ILogUtil
	{
		IObjectFactory Agent;
		Logger _logger;
		public LogUtil(IObjectFactory Factory)
		{
			this.Agent = Factory;
			InitializeLogger();
		}

		private void InitializeLogger()
		{
			var config = new LoggingConfiguration();

			var consoleTarget = new ColoredConsoleTarget();
			config.AddTarget("console", consoleTarget);

			var fileTarget = new FileTarget();
			config.AddTarget("file", fileTarget);

			consoleTarget.Layout = @"${longdate} ${level}${logger}${message}";
			string strLogFile = "PTR_" + CONSTANTS.TimeStampWhenPTRWasLaunched + ".log";
			if (!string.IsNullOrEmpty(AppConfigReader.ProcessWideConfigSection.LoggingLocation))
			{
				this.Agent.InputOutputUtil.CreateDirectoryIfDoesNotExist(AppConfigReader.ProcessWideConfigSection.LoggingLocation);
				strLogFile = Path.Combine(AppConfigReader.ProcessWideConfigSection.LoggingLocation, strLogFile);
			}
			else
			{
				strLogFile = Path.Combine(this.Agent.InputOutputUtil.CurrentWorkingDirectory, strLogFile);
			}

			fileTarget.FileName = strLogFile;
			fileTarget.Layout = consoleTarget.Layout;

			CreateLogRules(config, consoleTarget, fileTarget, LogLevel.Trace);
			CreateLogRules(config, consoleTarget, fileTarget, LogLevel.Info);
			CreateLogRules(config, consoleTarget, fileTarget, LogLevel.Warn);
			CreateLogRules(config, consoleTarget, fileTarget, LogLevel.Fatal);

			LogManager.Configuration = config;
			_logger = LogManager.GetLogger(": ");
		}

		private static void CreateLogRules(LoggingConfiguration config, ColoredConsoleTarget consoleTarget, FileTarget fileTarget, LogLevel logLevel)
		{
			var rule = new LoggingRule("*", logLevel, logLevel, fileTarget);
			config.LoggingRules.Add(rule);

			rule = new LoggingRule("*", logLevel, logLevel, consoleTarget);
			config.LoggingRules.Add(rule);
		}


		public void LogMessage(string strMessage)
		{
			_logger.Trace(strMessage);
		}

		public void LogStatus(string strStatus)
		{
			_logger.Info(strStatus);
		}

		public void LogWarning(string strWarning)
		{
			_logger.Warn(strWarning);
		}

		public void LogError(string strError)
		{
			_logger.Fatal(strError);
		}
	}
}
