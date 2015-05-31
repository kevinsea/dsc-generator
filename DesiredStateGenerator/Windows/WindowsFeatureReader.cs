using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DesiredState.Common;
using wmi = System.Management;
using System.Management.Automation;

namespace DesiredState.Windows
{
	internal class WindowsFeatureReader
	{

		public List<WindowsFeature> GetFeatures()
		{
			var features = new List<WindowsFeature>();

			var featureLookupSupport = GetFeatureLookupSupport();

			PowerShell ps = PowerShell.Create();
			if (featureLookupSupport == FeatureLookupSupport.GetWindowsFeature)  // Windows server >= 2008
			{
				ps.AddCommand("Get-WindowsFeature");  //ps.AddParameter("-online"); // 
				var psResult = ps.Invoke();

				var listToProcess = psResult
						.Where(o => o.Properties["Name"].Value.ToString().StartsWith("Web-"))
						.OrderBy(o => o.Properties["Name"].Value.ToString());

				foreach (var psFeature in listToProcess)
				{
					string featureName = (string)psFeature.Properties["Name"].Value;
					string displayName = (string)psFeature.Properties["DisplayName"].Value;
					string installed = (string)psFeature.Properties["Installed"].Value.ToString();
				
					var stateStr = (CodeGenHelpers.AreEqualCI(installed, "true")) ? "Present" : "Absent";

					var feature = new WindowsFeature(featureName, featureName, stateStr, displayName);

					features.Add(feature);
				}
			}
			else if (featureLookupSupport == FeatureLookupSupport.GetWindowsOptionalFeature) // Windows 8 version
			{
				var x = ps.AddCommand("Get-WindowsOptionalFeature").AddParameter("online");

				Collection<PSObject> psResult;

				psResult = ps.Invoke();		//this call throws a first chance exception that is supressed internally

				var listToProcess = psResult
										.Where(o => o.Properties["FeatureName"].Value.ToString().StartsWith("IIS-"))
										.OrderByDescending(o => o.Properties["State"].Value.ToString())
										.ThenBy(o => o.Properties["FeatureName"].Value.ToString());

				foreach (var psFeature in listToProcess)
				{
					var featureName = (string)psFeature.Properties["FeatureName"].Value;

					var state = (string)psFeature.Properties["State"].Value.ToString();

					var stateStr = (state == "Enabled") ? "Present" : "Absent";

					var feature = new WindowsFeature(featureName, featureName, stateStr);

					features.Add(feature);
				}

			}

			else  
			{
				var feature = new WindowsFeature("IIS", "Web-Server", "Present", "Correct PowerShell command not present, settting IIS Base");
				features.Add(feature);
			}

			return features;
		}

		private FeatureLookupSupport GetFeatureLookupSupport()
		{
			FeatureLookupSupport featureLookupSupport = FeatureLookupSupport.None;

			try
			{
				PowerShell psFeatureChecker = PowerShell.Create();
				psFeatureChecker.AddCommand("Get-Command").AddParameter("Name", "Get-WindowsFeature");

				var featureResult = psFeatureChecker.Invoke();

				if (featureResult.Any())
					featureLookupSupport = FeatureLookupSupport.GetWindowsFeature;

				else
				{
					psFeatureChecker.Commands.Clear();
					psFeatureChecker.AddCommand("Get-Command").AddParameter("Name", "Get-WindowsOptionalFeature");
					featureResult = psFeatureChecker.Invoke();

					if (featureResult.Any())
						featureLookupSupport = FeatureLookupSupport.GetWindowsOptionalFeature;
				}

			}

			catch (Exception)
			{
				featureLookupSupport = FeatureLookupSupport.None;
			}
			return featureLookupSupport;
		}

		enum FeatureLookupSupport
		{
			None = 0,
			GetWindowsOptionalFeature = 1,
			GetWindowsFeature = 2
		}

	}
}
