using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DesiredState.Common;
using Microsoft.Web.Administration;

namespace DesiredState.IIS
{
	/// <summary>
	/// Orchestrates assembly of all of the objects that make up IIS Site DSC
	/// </summary>
	internal class IISObjectFactory
	{

		public List<SiteDesiredState> BuildSites(IISCodeGenerator.IisPoolAndSitesOptions iisOptions)
		{
			ServerManager serverManager = new ServerManager();
			List<SiteDesiredState> siteCodeList = new List<SiteDesiredState>();
			WebConfigPropertyDesiredStateAssembler gen = new WebConfigPropertyDesiredStateAssembler();

			List<WebConfigPropertyDesiredState> authDesiredStateList = gen.GetAuthenticationDesiredStates();

			foreach (var site in serverManager.Sites)
			{
			    try
			    {
			        var siteName = site.Name;
			        var siteAuthDesiredStateList = authDesiredStateList.Where(a => CodeGenHelpers.AreEqualCI(a.SiteName, siteName));
			        var siteCode = new SiteDesiredState(site, siteAuthDesiredStateList, iisOptions);

			        siteCodeList.Add(siteCode);
			    }
			    catch (Exception ex)
			    {
			        Trace.TraceError(ex.ToString());
			        MessageBox.Show("Error generating config for site '" + site.Name + "'. No config is generated for this site.");
			    }
			}

			return siteCodeList;
		}

		public List<PoolDesiredState> BuildPools(IISCodeGenerator.IisPoolAndSitesOptions iisOptions)
		{
			ServerManager serverManager = new ServerManager();
			List<PoolDesiredState> poolCodeList = new List<PoolDesiredState>();

			var pools = serverManager.ApplicationPools;

			foreach (var pool in pools)
			{
				var poolCode = new PoolDesiredState(pool, iisOptions);
				poolCodeList.Add(poolCode);
			}

			return poolCodeList;
		}

	}
}
