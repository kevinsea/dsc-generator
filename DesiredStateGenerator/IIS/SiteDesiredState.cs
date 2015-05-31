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
		public List<WebConfigPropertyDesiredState> AuthDesiredStateList = new List<WebConfigPropertyDesiredState>();

		private string ApplicationPool { get; set; }

		public SiteDesiredState(Site iisSiteObject
								, IEnumerable<WebConfigPropertyDesiredState> authDesiredStateList
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

			if (iisOptions.StandardizeLogFileLocation)
			{
				this.AddAttributeWithOverrideValue("LogFileDirectory", @"D:\IISLogs", iisSiteObject.LogFile.Directory); 
			}
			else
			{
				this.AddAttribute("LogFileDirectory", iisSiteObject.LogFile.Directory);
			}
			
			this.AddAttribute("DependsOn", "[cAppPool]" + PoolDesiredState.GetPoolVariableName(this.ApplicationPool));

			this.Bindings = GetBindings(iisSiteObject.Bindings);
			this.Applications = GetApplications(iisSiteObject.Applications, this.Key, this.Name);
			this.AuthDesiredStateList.AddRange(authDesiredStateList);
		}

		public override string GetChildCode(int baseIndentDepth)
		{
			string baseIndent = CodeGenHelpers.GetIndentString(baseIndentDepth);

			var code = "";
			code += GetChildListCode("BindingInfo", this.Bindings.ToList<DesiredStateBase>(), baseIndentDepth, baseIndent);

			code += GetChildListCode("WebConfigProp", this.AuthDesiredStateList.ToList<DesiredStateBase>(), baseIndentDepth, baseIndent);

			return code;
		}

		protected override string DscObjectType
		{
			get { return "cWebSite"; }
		}

		internal static string GetSiteKey(string siteName)
		{
			return CodeGenHelpers.FormatKey(siteName) + "_Site";
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

		private List<ApplicationDesiredState> GetApplications(ApplicationCollection applications, string siteKey, string siteName)
		{
			var webApplicationList = new List<ApplicationDesiredState>();

			foreach (var application in applications)
			{

					var b = new ApplicationDesiredState(application,siteKey, siteName);

					webApplicationList.Add(b);

			}

			return webApplicationList;
		}

	}

}
