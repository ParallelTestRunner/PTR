using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	abstract class BaseConfig
	{
		public object this[string propertyName]
		{
			get
			{
				var property = GetPropertyInfo(propertyName);
				return property.GetValue(this, null);
			}
			set
			{
				var property = GetPropertyInfo(propertyName);
				property.SetValue(this, value, null);
			}
		}

		public PropertyInfo GetPropertyInfo(string propertyName)
		{
			var property = this.GetType().GetProperty(propertyName);
			if (property == null)
			{
				this.InvalidPropertyAccessHandler(propertyName);
			}
			return property;
		}

		protected abstract void InvalidPropertyAccessHandler(string strPropertyName);

		public abstract string Identifier { get; }

		public string[] GetExecutableAlongWithCommandLineParametersOfAGivenProperty(ConfigProperty configProperty,
																					string strConfigExecutionLocation,
																					IObjectFactory Agent,
																					out List<ResourceSection> lstResourcesAllocated)
		{
			lstResourcesAllocated = new List<ResourceSection>();
			string strConfigPropertyValue = this[configProperty.ToString()].ToString();
			if (string.IsNullOrEmpty(strConfigPropertyValue))
			{
				return null;
			}

			var arr = strConfigPropertyValue.Split(new string[] { CONSTANTS.SeparatorBetweenExecutableFilePathAndItsCommandLineParameter },
																StringSplitOptions.None)
														.Select(x => x.Trim())
														.Where(y => !string.IsNullOrEmpty(y)).ToArray();

			arr[0] = arr[0].GetFullPath(Agent.InputOutputUtil);
			if (!File.Exists(arr[0]))
			{
				arr[0] = Path.Combine(strConfigExecutionLocation, arr[0]);
			}
			if (!File.Exists(arr[0]))
			{
				throw new InvalidConfigurationException(GetInvalidPathExceptionMessageForGivenConfigProperty(arr[0], configProperty));
			}

			string strPathSwitch = "/Path:";
			string strSharedResourcesSwitch = "/SharedResources:";
			for (int i = 1; i < arr.Length; i++ )
			{
				string strParameter = arr[i];
				if (strParameter.ToLower().StartsWith(strPathSwitch.ToLower()))
				{
					strParameter = strParameter.Substring(strPathSwitch.Length);
					if (File.Exists(Path.Combine(strConfigExecutionLocation, strParameter)))
					{
						strParameter = Path.Combine(strConfigExecutionLocation, strParameter);
					}
					else
					{
						var testingProjectLocation = this[ConfigProperty.TestingProjectLocation.ToString()];
						string strTempPath = strParameter.GetFullPath(Agent.InputOutputUtil,
																		testingProjectLocation == null ? null : testingProjectLocation.ToString());
						if (File.Exists(strTempPath))
						{
							strParameter = strTempPath;
						}
					}

					if (!File.Exists(strParameter))
					{
						throw new InvalidConfigurationException(GetInvalidPathExceptionMessageForGivenConfigProperty(strParameter, configProperty));
					}
				}
				else if (strParameter.ToLower().StartsWith(strSharedResourcesSwitch.ToLower()))
				{
					strParameter = strParameter.Substring(strSharedResourcesSwitch.Length);
					var IDs = strParameter.Split(';').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).Shuffle();
					if (!SharedResourcesUtil.SharedResources.Select(x => x.ID).ContainsAll(IDs))
					{
						throw new InvalidConfigurationException(GetInvalidSharedResourceExceptionMessage(strSharedResourcesSwitch + strParameter, configProperty));
					}

					var resource = SharedResourcesUtil.GetLeastUsedResource(IDs);
					lstResourcesAllocated.Add(resource);
					strParameter = resource.SemicolonSeparatedResources;
				}
				arr[i] = strParameter;
			}

			return arr;
		}

		private string GetInvalidPathExceptionMessageForGivenConfigProperty(string strPath, ConfigProperty configProperty)
		{
			return string.Format("Invalid {0} path in {1}.\n The path should either be a full path " +
										"or relative to the folder containing {2} or relative to the path in {3}",
										strPath,
										this.Identifier + "." + configProperty.ToString(),
										System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe",
										this.Identifier + "." + ConfigProperty.TestingProjectLocation.ToString().ToString());
		}

		private string GetInvalidSharedResourceExceptionMessage(string strSharedResourceSwitch, ConfigProperty configProperty)
		{
			return string.Format("One or more shared resources in {0} in {1} are not defined in SharedResources section.",
											strSharedResourceSwitch,
											this.Identifier + "." + configProperty.ToString());
		}
	}
}
