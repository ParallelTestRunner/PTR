using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class CommandLineProcessor : ICommandLineProcessor
	{
		/* Commandline parameters can be used to change the properties of configurations specified in App.Config.
		 * Here is how to modify configurations using command line parameters:
		 * 
		 * The template of command line parameter to override an attribute of ProcessWideConfig is "/AttributeName:ProcessWideConfig:Value".
		 * For example to override the "MaxThreads" attribute of ProcessWideConfig, you will pass the command line parameter as below: 
		 * /MaxThreads:ProcessWideConfig:5
		 * 
		 * The template of command line parameter to override an attribute of all "Config" in "TestingConfigurations" is
		 * "/AttributeName:TestingConfigurations:Value". For example to override the "IsEnabled" attribute of all "Config" in "TestingConfigurations",
		 * you will pass the command line parameter as below: 
		 * /IsEnabled:TestingConfigurations:true
		 * 
		 * The template of command line parameter to override an attribute of a specific "Config" in "TestingConfigurations" is
		 * "/AttributeName:ID:Value". For example to override the "TestRunner" attribute of a specific "Config" in "TestingConfigurations",
		 * you will pass the command line parameter as below: 
		 * /TestRunner:ID of the "Config" you want to change:new value of TestRunner
		 * 
		 */

		class Argument
		{
			public ConfigProperty Property { get; private set; }
			public string Locator { get; private set; }
			public string Value { get; private set; }

			public Argument(string strArgument)
			{
				if (!strArgument.StartsWith("/"))
				{
					new FormatException("Command line arguments starts with /");
				}

				var enumerableTemp = strArgument.Remove(0, 1).Split(':').Where(x => !(string.IsNullOrEmpty(x))).Select(x => x.Trim());
				if (enumerableTemp.Count() != 3)
				{
					throw new ArgumentException(string.Format("{0} is not a valid command line argument. Correct format is Attribute:Locator:Value.", strArgument));
				}

                this.Locator = enumerableTemp.ElementAt(0);
                this.Property = enumerableTemp.ElementAt(1).ToProperty();
				this.Value = enumerableTemp.ElementAt(2);
			}

			public override string ToString()
			{
				return "/" + this.Locator + ":" + this.Property.ToString() + ":" + this.Value;
			}
		}

		IObjectFactory Agent;
		public CommandLineProcessor(IObjectFactory Factory)
		{
			this.Agent = Factory;
		}

		public void ApplyCommandLineArguments(ProcessWideConfig processWideConfig, IEnumerable<TestConfig> testConfigCollection)
		{
			Agent.LogUtil.LogMessage("Parsing command line arguments if any provided...");
			var arguments = Environment.GetCommandLineArgs().Skip(1).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x))
                                                                    .Select((x) => new Argument(x.Trim()));

			if (arguments.Count() < 1)
			{
				return;
			}

			Agent.LogUtil.LogMessage("Applying command line arguments...");
			foreach (var argument in arguments)
			{
				ApplyArgument(argument, processWideConfig, testConfigCollection);
			}
		}

		private void ApplyArgument(Argument argument, ProcessWideConfig processWideConfig, IEnumerable<TestConfig> testConfigCollection)
		{
			if (argument.Locator.ToLower() == "ProcessWideConfig".ToLower())
			{
				ApplyArgument(argument, processWideConfig);
				return;
			}

			if (argument.Locator.ToLower() == "TestingConfigurations".ToLower())
			{
				testConfigCollection.Select(x => { ApplyArgument(argument, x); return x; });
				return;
			}

			TestConfig testConfig = testConfigCollection.Where(x => x.OriginalID.ToLower() == argument.Locator.ToLower()).First();
			if (testConfig != null)
			{
				ApplyArgument(argument, testConfig);
				return;
			}

			throw new ArgumentException(string.Format("{0} is not a valid command line argument", argument));
		}

		private void ApplyArgument(Argument argument, BaseConfig configObject)
		{
            try
            {
                var propertyInfo = configObject.GetPropertyInfo(argument.Property.ToString());
                switch (Type.GetTypeCode(propertyInfo.PropertyType))
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        configObject[argument.Property.ToString()] = int.Parse(argument.Value);
                        return;
                    case TypeCode.String:
                        configObject[argument.Property.ToString()] = argument.Value;
                        return;
                    case TypeCode.Boolean:
                        configObject[argument.Property.ToString()] = Convert.ToBoolean(argument.Value);
                        return;
                    case TypeCode.Object:
                        configObject[argument.Property.ToString()] = Convert.ToBoolean(argument.Value);
                        return;
                }
            }
            catch
            { }

            throw new ArgumentException(string.Format("{0} is not a valid command line argument", argument));
        }
	}
}
