using DesiredState.Common;
using Microsoft.Web.Administration;

namespace DesiredState.IIS
{
    class VirtualDirectoryDesiredState : DesiredStateBase
    {
        public bool IsRootOfAnApplication { get; private set; }

        public VirtualDirectoryDesiredState(VirtualDirectory virtualDirectory, string siteName, string webApplicationName)
        {
            Initialize(virtualDirectory, siteName, webApplicationName);
        }

        private void Initialize(VirtualDirectory virtualDirectory, string siteName, string webApplicationName)
        {
            this.Key = GetVirtualDirectoryVariableName(siteName, virtualDirectory.Path);

            this.IsRootOfAnApplication = (virtualDirectory.Path == "/");

            string name = virtualDirectory.Path.StartsWith("/") ? virtualDirectory.Path.Substring(1) : virtualDirectory.Path;
            AddAttribute("Name", name);

            AddAttribute("Ensure", "Present");
            AddAttribute("Website", siteName);
            AddAttribute("PhysicalPath", virtualDirectory.PhysicalPath);
            AddAttribute("WebApplication", FormatWebApplicationName(webApplicationName));
            AddAttribute("DependsOn", "[xWebSite]" + SiteDesiredState.GetSiteKey(siteName));
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
