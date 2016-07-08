using System.Collections.Generic;
using DesiredState.Common;
using Microsoft.Web.Administration;

namespace DesiredState.IIS
{
	class ApplicationDesiredState : DesiredStateBase
	{
		public bool IsRootApplication { get; private set; }
		public string ApplicationPool { get; set; }

		public List<VirtualDirectoryDesiredState> VirtualDirectories = new List<VirtualDirectoryDesiredState>();

		public ApplicationDesiredState(Application application, string siteKey, string siteName)
		{
			Initialize(application, siteKey, siteName);
		}

		private void Initialize(Application application, string siteKey, string siteName)
		{
			this.Key = GetApplicationVariableName(siteName, application.Path);
			this.IsRootApplication = (application.Path == "/");

			AddAttribute("Name", application.Path);
			AddAttribute("Ensure", "Present");
			AddAttribute("Website", siteName);
			AddAttribute("PhysicalPath", application.VirtualDirectories[0].PhysicalPath);

			this.ApplicationPool =application.ApplicationPoolName;
			AddAttribute("WebAppPool", this.ApplicationPool);
			
			AddAttribute("DependsOn", "[xWebAppPool]" + PoolDesiredState.GetPoolVariableName(application.ApplicationPoolName));

			this.VirtualDirectories = GetVirtualDirectories(application.VirtualDirectories, siteName, application.Path);
		}

		private List<VirtualDirectoryDesiredState> GetVirtualDirectories(
						VirtualDirectoryCollection virtualDirectories, string siteName, string applicationName)
		{
			var virtualDirectoryList = new List<VirtualDirectoryDesiredState>();

			foreach (var virtualDirectory in virtualDirectories)
			{
				var b = new VirtualDirectoryDesiredState(virtualDirectory, siteName, applicationName);

				virtualDirectoryList.Add(b);
			}

			return virtualDirectoryList;
		}

		protected override string DscObjectType
		{
			get { return "xWebApplication"; }
		}

		public static string GetApplicationVariableName(string name1, string name2)
		{
			return CodeGenHelpers.FormatKey(name1, name2, "App");
		}

	}
}
