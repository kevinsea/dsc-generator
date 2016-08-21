using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DesiredState.Common;
using Microsoft.Web.Administration;

namespace DesiredState.IIS
{
    internal class SiteDesiredState : DesiredStateBase
    {
        public List<IPBindingDesiredState> Bindings = new List<IPBindingDesiredState>();
        public List<ApplicationDesiredState> Applications = new List<ApplicationDesiredState>();
        public List<WebConfigEntry> AllWebConfigEntryList = new List<WebConfigEntry>();
        public WebAuthenticationInformation AuthenticationInfo;

        private string ApplicationPool { get; set; }

        public SiteDesiredState(Site iisSiteObject
                                , List<WebConfigEntry> configEntries
                                , IISCodeGenerator.IisPoolAndSitesOptions iisOptions)
        {
            var rootApp = iisSiteObject.Applications[0];

            this.Key = GetSiteKey(iisSiteObject.Name);

            this.AddAttribute("Name", iisSiteObject.Name);
            this.AddAttribute("Ensure", "Present");
            this.AddAttribute("State", iisSiteObject.State.ToString());

            this.ApplicationPool = rootApp.ApplicationPoolName;
            this.AddAttribute("ApplicationPool", this.ApplicationPool);

            this.AddAttributeWithComment("PhysicalPath", rootApp.VirtualDirectories[0].PhysicalPath, "This folder must already exist");

            string logAttributeName = "LogPath";
            if (iisOptions.StandardizeLogFileLocation)
                this.AddAttributeWithOverrideValue(logAttributeName, "$logFilePath", iisSiteObject.LogFile.Directory);
            else
                this.AddAttribute(logAttributeName, iisSiteObject.LogFile.Directory);

            this.AddAttribute("DependsOn", "[xWebAppPool]" + PoolDesiredState.GetPoolVariableName(this.ApplicationPool));

            this.Bindings = GetBindings(iisSiteObject.Bindings);

            this.AuthenticationInfo = GetSiteAuthenticationConfiguration(configEntries, iisSiteObject.Name);

            Dictionary<string, WebAuthenticationInformation> authEntries = GetAppsAuthenticationConfigurations(configEntries);

            this.Applications = GetApplications(iisSiteObject.Applications, this.Key, this.Name, authEntries);

            //todo this.AllWebConfigEntryList.AddRange(configEntries);
        }

        private WebAuthenticationInformation GetSiteAuthenticationConfiguration(List<WebConfigEntry> authConfigEntries, string siteName)
        {

            List<WebConfigEntry> siteConfigEntries = authConfigEntries.Where(groupedEntry => groupedEntry.Path == siteName).ToList();

            if (siteConfigEntries.Any())
                return new WebAuthenticationInformation(siteConfigEntries);
            else
            {
                return null;
            }

        }

        private Dictionary<string, WebAuthenticationInformation> GetAppsAuthenticationConfigurations(List<WebConfigEntry> authConfigEntries)
        {
            var results = new Dictionary<string, WebAuthenticationInformation>();

            IEnumerable<IGrouping<string, WebConfigEntry>> configEntryGroups = authConfigEntries.GroupBy(a => a.Path);

            foreach (var configEntriesByLocation in configEntryGroups)
            {
                results.Add(configEntriesByLocation.Key, new WebAuthenticationInformation(configEntriesByLocation.ToList()));
            }

            return results;
        }

        public override string GetChildCode(int baseIndentDepth)
        {
            string baseIndent = CodeGenHelpers.GetIndentString(baseIndentDepth);

            string code = "";
            code += CodeGenHelpers.GenerateChildListCode("BindingInfo", this.Bindings.ToList<DesiredStateBase>(), baseIndentDepth, baseIndent);

            if (AuthenticationInfo != null)
                code += baseIndent + "AuthenticationInfo = \n" + this.AuthenticationInfo.GetCode(baseIndentDepth + 2, CodeGenType.SingleChild);

            return code;
        }

        protected override string DscObjectType
        {
            get { return "xWebSite"; }
        }

        internal static string GetSiteKey(string siteName)
        {
            return CodeGenHelpers.FormatKey(siteName, "Site");
        }

        public List<string> GetPoolsReferenced()
        {
            var pools = new List<string>();

            pools.Add(this.ApplicationPool);

            foreach (ApplicationDesiredState app in this.Applications)
            {
                pools.Add(app.ApplicationPool);
            }

            pools = pools.Distinct().OrderBy(p => p).ToList();

            return pools;
        }

        private List<IPBindingDesiredState> GetBindings(BindingCollection bindings)
        {
            var siteBindingList = new List<IPBindingDesiredState>();

            foreach (var binding in bindings)
            {
                if (binding.Protocol.ToLower().StartsWith("http"))
                {
                    var b = new IPBindingDesiredState(binding);

                    siteBindingList.Add(b);
                }
            }

            return siteBindingList;
        }

        private List<ApplicationDesiredState> GetApplications(ApplicationCollection applications, string siteKey
                                                , string siteName, Dictionary<string, WebAuthenticationInformation> authEntries)
        {
            var webApplicationList = new List<ApplicationDesiredState>();

            foreach (var application in applications)
            {
                WebAuthenticationInformation authInfo;
                authEntries.TryGetValue(siteName + application.Path, out authInfo);

                var app = new ApplicationDesiredState(application, siteKey, siteName, authInfo);

                webApplicationList.Add(app);
            }

            return webApplicationList;
        }

    }

}
