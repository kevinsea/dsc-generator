using System.Collections.Generic;
using DesiredState.Common;
using Microsoft.Web.Administration;
using DesiredState.IIS;

namespace DesiredState.IIS
{
    class ApplicationDesiredState : DesiredStateBase
    {
        public bool IsRootApplication { get; private set; }
        public string ApplicationPool { get; set; }
        public List<VirtualDirectoryDesiredState> VirtualDirectories = new List<VirtualDirectoryDesiredState>();
        public WebAuthenticationInformation AuthenticationInfo;

        public ApplicationDesiredState(Application application, string siteKey, string siteName, WebAuthenticationInformation authInfo)
        {
            Initialize(application, siteKey, siteName, authInfo);
        }

        private void Initialize(Application application, string siteKey, string siteName, WebAuthenticationInformation authInfo)
        {
            this.Key = GetApplicationVariableName(siteName, application.Path);
            this.IsRootApplication = (application.Path == "/");

            AddAttribute("Name", application.Path);
            AddAttribute("Ensure", "Present");
            AddAttribute("Website", siteName);
            AddAttribute("PhysicalPath", application.VirtualDirectories[0].PhysicalPath);

            this.ApplicationPool = application.ApplicationPoolName;
            AddAttribute("WebAppPool", this.ApplicationPool);

            AddAttribute("DependsOn", "[xWebAppPool]" + PoolDesiredState.GetPoolVariableName(application.ApplicationPoolName));

            this.VirtualDirectories = GetVirtualDirectories(application.VirtualDirectories, siteName, application.Path);
            this.AuthenticationInfo = authInfo;
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


        public static string GetApplicationVariableName(string name1, string name2)
        {
            return CodeGenHelpers.FormatKey(name1, name2, "App");
        }


        public override string GetChildCode(int baseIndentDepth)
        {
            string baseIndent = CodeGenHelpers.GetIndentString(baseIndentDepth);

            string code = "";

            if (AuthenticationInfo != null)
                code += baseIndent + "AuthenticationInfo = \n" + this.AuthenticationInfo.GetCode(baseIndentDepth + 2, CodeGenType.SingleChild);

            return code;
        }

        protected override string DscObjectType
        {
            get { return "xWebApplication"; }
        }

    }
}
