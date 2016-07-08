using DesiredState.Common;

namespace DesiredState.IIS
{

	internal class WebConfigPropertyDesiredState : DesiredStateBase
	{
		internal string SiteName { get; set; }
		internal string SiteLocation { get; set; }

		public WebConfigPropertyDesiredState(string filter, string propertyName, string value, string siteLocation)
		{
			SiteName = siteLocation.Split('/')[0];
			
			var elements = filter.Split('/');
			var parentElementName = elements[elements.Length - 1];

			this.Key = CodeGenHelpers.FormatKey(siteLocation + "_" + parentElementName.Replace("Authentication", "Auth") + "_" + propertyName, "xxx");

			this.SiteLocation = siteLocation;
			this.AddAttribute("Location", siteLocation);
			this.AddAttribute("Filter", filter);
			this.AddAttribute("Name", propertyName);
			this.AddAttribute("Value", value);
			this.AddAttribute("PSPath", "MACHINE/WEBROOT/APPHOST");
		}

		protected override string DscObjectType
		{
			get { return "xWebConfigKeyValue"; }
		}
	}

}
