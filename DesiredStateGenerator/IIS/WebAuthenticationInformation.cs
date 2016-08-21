using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesiredState.Common;
using Microsoft.Web.Administration;

namespace DesiredState.IIS
{

	class WebAuthenticationInformation : DesiredStateBase
	{
		private Position _position;

		internal WebAuthenticationInformation(List<WebConfigEntry> configEntries)
		{
			bool isEnabled;

			if (!configEntries.Any())
				throw new ArgumentException();

			WebConfigEntry firstConfigEntry = configEntries.First();

			if (firstConfigEntry.SiteName == firstConfigEntry.Path)
				_position = Position.WebSite;
			else
				_position = Position.WebApp;


			foreach (var entry in configEntries)
			{
				if (entry.Filter.StartsWith("/system.webServer/security/authentication/"))  //TODO move this so auth config is divided earlier in the process
				{
					string attributeName = entry.Name.Replace("Authentication", "");
					attributeName = attributeName.Substring(0, 1).ToUpper() + attributeName.Substring(1);

					if (bool.TryParse(entry.Attributes.FirstOrDefault(a => a.Name == "enabled")?.Value, out isEnabled))
						AddAttribute(attributeName, isEnabled);
				}

			}

			//Anonymous = $true
			//Basic = $true
			//Digest = $true
			//Windows = $true
		}

		protected override string DscObjectType
		{
			get
			{
				if (_position == Position.WebSite)
					return "MSFT_xWebAuthenticationInformation";
				else
					return "MSFT_xWebApplicationAuthenticationInformation";
			}
		}

		internal enum Position
		{
			WebSite = 1,
			WebApp = 2
		}

	}
}
