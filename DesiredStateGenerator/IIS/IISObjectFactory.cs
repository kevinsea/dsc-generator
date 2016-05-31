using System.Collections.Generic;
using System.Linq;
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

			List<WebConfigPropertyDesiredState> configPropDesiredStateList = gen.GetWebConfigPropertyDesiredStates();

			foreach (var site in serverManager.Sites)
			{
				var siteName = site.Name;
				var siteAuthDesiredStateList = new List<WebConfigPropertyDesiredState>(); // TODO reimplement this for the MS xWeb modules
					//configPropDesiredStateList.Where(a => CodeGenHelpers.AreEqualCI(a.SiteName, siteName));
				var siteCode = new SiteDesiredState(site, siteAuthDesiredStateList, iisOptions);

				siteCodeList.Add(siteCode);
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
