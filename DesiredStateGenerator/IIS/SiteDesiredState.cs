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

		public WebAuthenticationInformation WebAuthenticationInfo;

		private string ApplicationPool { get; set; }

		public SiteDesiredState(Site iisSiteObject
								, IEnumerable<WebConfigEntry> configEntries
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
				this.AddAttributeWithOverrideValue(logAttributeName, @"D:\IISLogs", iisSiteObject.LogFile.Directory);
			else
				this.AddAttribute(logAttributeName, iisSiteObject.LogFile.Directory);

			this.AddAttribute("DependsOn", "[xWebAppPool]" + PoolDesiredState.GetPoolVariableName(this.ApplicationPool));

			this.Bindings = GetBindings(iisSiteObject.Bindings);

			var authConfigEntries = configEntries.Where(c => c.SiteLocation == iisSiteObject.Name);

			if(authConfigEntries != null  && authConfigEntries.Count() > 0)
			{
				this.WebAuthenticationInfo = new WebAuthenticationInformation(authConfigEntries);
			}

			this.Applications = GetApplications(iisSiteObject.Applications, this.Key, this.Name, configEntries);

            this.AllWebConfigEntryList.AddRange(configEntries);
		}

		public override string GetChildCode(int baseIndentDepth)
		{
			string baseIndent = CodeGenHelpers.GetIndentString(baseIndentDepth);

			var code = "";
			code += CodeGenHelpers.GenerateChildListCode("BindingInfo", this.Bindings.ToList<DesiredStateBase>(), baseIndentDepth, baseIndent);

			if (WebAuthenticationInfo !=null)
				code += baseIndent + "AuthenticationInfo = \n" + this.WebAuthenticationInfo.GetCode(baseIndentDepth + 2, CodeGenType.SingleChild);

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
												, string siteName, IEnumerable<WebConfigEntry> configEntries)
		{
			var webApplicationList = new List<ApplicationDesiredState>();

			foreach (var application in applications)
			{
				var authConfigEntry = configEntries.Where(c => c.SiteLocation == application.Path).FirstOrDefault();
				var b = new ApplicationDesiredState(application, siteKey, siteName);

				webApplicationList.Add(b);

			}

			return webApplicationList;
		}

	}

}
