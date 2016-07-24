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

        public WebAuthenticationInformation(IEnumerable<WebConfigEntry> configEntries)
        {
            bool isEnabled;
            foreach (var entry in configEntries)
            {
                if (entry.Filter.StartsWith("/system.webServer/security/authentication/"))  //TODO move this so auth config is divided earlier in the process
                {
                    string attributeName = entry.Name.Replace("Authentication", "");
                    attributeName = attributeName.Substring(0, 1).ToUpper() + attributeName.Substring(1);

                    if (bool.TryParse(entry.Attributes.First(a => a.Name == "enabled").Value, out isEnabled))
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
            get { return "MSFT_xWebAuthenticationInformation"; }
        }

    }
}
