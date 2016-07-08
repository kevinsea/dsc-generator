using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesiredState.Common;

namespace DesiredState.IIS
{
	/// <summary>
	/// Generates IIS Site DSC
	/// </summary>
	internal class IISCodeGenerator
	{

		public string GenerateCode(IisPoolAndSitesOptions options)
		{
			IISObjectFactory objectFactory = new IISObjectFactory();
			StringBuilder sb = new StringBuilder();

			List<SiteDesiredState> sites = objectFactory.BuildSites(options);
			List<PoolDesiredState> pools = objectFactory.BuildPools(options);

			string baseIndent = CodeGenHelpers.GetIndentString(2);
			string code;

			sb.AppendLine(baseIndent + "# Note this code does not detect server level IIS overrides (it assumes the IIS level settings");
			sb.AppendLine(baseIndent + "# have not been overriden).  See the wiki for information about detecting server level changes.\n");

			if (options.IisPoolAndSitesGenerationMode == IisPoolAndSitesGenerationMode.ConfigFileOrder)
			{
				code = GeneratePools(pools);
				sb.AppendLine(code);

				code = GenerateSites(sites);
				sb.AppendLine(code);
			}
			else
			{
				code = GenerateSitesAndPools(sites, pools);
				sb.AppendLine(code);
			}

			return sb.ToString();
		}

		public string GenerateIisSiteImports()
		{
			string code = "";
			StringBuilder sb = new StringBuilder();

			string indent = CodeGenHelpers.GetIndentString(1);

			sb.AppendLine(indent + "# Information about where to get needed modules and required version information can ");
			sb.AppendLine(indent + "# be found here: https://github.com/kevinsea/dsc-generator/wiki/Powershell-Modules");
			sb.AppendLine(indent + "# ---------------------------------------------------------------------------------");
			sb.AppendLine("");
			sb.AppendLine(indent + "Import-DscResource -ModuleName xWebAdministration");

			return sb.ToString();
		}

		private string GeneratePools(IEnumerable<PoolDesiredState> poolList)
		{
			StringBuilder sb = new StringBuilder();
			string code = "";

			foreach (var poolCode in poolList)
			{
				code = GeneratePool(poolCode);
				sb.AppendLine(code);
			}

			return sb.ToString();
		}

		private static string GeneratePool(PoolDesiredState poolCode)
		{
			return poolCode.GetCode(2);
		}

		private string GenerateSites(List<SiteDesiredState> siteList)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var site in siteList)
			{
				sb.Append(GetSiteHeader(site));
				sb.Append(GenerateSite(site));
			}
			return sb.ToString();
		}

		private string GetSiteHeader(SiteDesiredState site)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(CodeGenHelpers.Indent + CodeGenHelpers.Indent);
			sb.Append($"#================== '{site.Name}' site definition ==================\n");

			return sb.ToString();
		}

		private string GenerateSitesAndPools(IEnumerable<SiteDesiredState> siteList
											, IEnumerable<PoolDesiredState> poolList)
		{
			string code = "";
			StringBuilder sb = new StringBuilder();

			List<PoolDesiredState> poolsLeftToGenerate = new List<PoolDesiredState>(poolList);

			siteList = siteList.OrderBy(s => s.Name);

			foreach (SiteDesiredState site in siteList)
			{
				code = GetSiteHeader(site);
				sb.Append(code);

				foreach (string poolName in site.GetPoolsReferenced())
				{
					var poolToGenerate = poolsLeftToGenerate.FirstOrDefault(p => p.Name == poolName);

					if (poolToGenerate != null)
					{
						code = GeneratePool(poolToGenerate);
						sb.Append(code);

						poolsLeftToGenerate.Remove(poolToGenerate);
					}
					else
					{
						code = CodeGenHelpers.GetIndentString(2)
								+ string.Format("# Pool '{0}' was generated with an earlier site\n", poolName);

						sb.Append(code);
					}

				}

				sb.AppendLine("");
				sb.Append(GenerateSite(site));
			}

			return sb.ToString();
		}

		private string GenerateSite(SiteDesiredState site)
		{
			string code = "";

			code += site.GetCode(2) + "\n";

			foreach (var application in site.Applications)
			{
				if (application.IsRootApplication == false)
				{
					code += application.GetCode(2) + "\n";
				}

				foreach (var virtualDir in application.VirtualDirectories)
				{
					if (virtualDir.IsRootOfAnApplication == false)
					{
						if ((virtualDir.Key != "") && (virtualDir.Key != application.Key))
							code += virtualDir.GetCode(2) + "\n";
					}
				}
			}

			return code;
		}

		public struct IisPoolAndSitesOptions
		{
			public IisPoolAndSitesGenerationMode IisPoolAndSitesGenerationMode;
			public bool StandardizeAppPoolRecycles;
			public bool KeepAppPoolsRunning;
			public bool StandardizeLogFileLocation;
		}

		internal enum IisPoolAndSitesGenerationMode
		{
			NoGeneration = 0,
			Alphabetical = 1,
			ConfigFileOrder = 2
		}

	}
}
