using DesiredState.Common;
using Microsoft.Web.Administration;

namespace DesiredState.IIS
{
	class VirtualDirectoryDesiredState : DesiredStateBase
	{
		public bool IsRootOfAnApplication { get; private set; }

		public VirtualDirectoryDesiredState(VirtualDirectory VirtualDirectory, string siteName, string webApplicationName)
		{
			Initialize(VirtualDirectory, siteName, webApplicationName);
		}

		private void Initialize(VirtualDirectory virtualDirectory, string siteName, string webApplicationName)
		{
			this.Key = GetVirtualDirectoryVariableName(siteName, virtualDirectory.Path);

			this.IsRootOfAnApplication = (virtualDirectory.Path == "/");

			AddAttribute("Name", virtualDirectory.Path);
			
			AddAttribute("Ensure", "Present");
			AddAttribute("Website", siteName);
			AddAttribute("PhysicalPath", virtualDirectory.PhysicalPath);
			AddAttribute("WebApplication", FormatWebApplicationName( webApplicationName));
			AddAttribute("DependsOn", "[cWebSite]" + SiteDesiredState.GetSiteKey(siteName));
		}

		private static string FormatWebApplicationName(string webApplicationName)
		{
			if (webApplicationName == "/")
				webApplicationName = "";
			return webApplicationName;
		}

		private static string GetVirtualDirectoryVariableName(string name1, string name2)
		{
			return CodeGenHelpers.FormatKey(name1, name2, "Virt");
		}
		
		protected override string DscObjectType
		{
			get { return "xWebVirtualDirectory"; }
		}

	}
}
