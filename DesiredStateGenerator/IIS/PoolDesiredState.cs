using System.Linq;
using DesiredState.Common;
using Microsoft.Web.Administration;

namespace DesiredState.IIS
{
	internal class PoolDesiredState : DesiredStateBase
	{

		public PoolDesiredState(ApplicationPool iisPoolObject, IISCodeGenerator.IisPoolAndSitesOptions iisOptions)
		{
			Initialize(iisPoolObject, iisOptions);
		}

		private void Initialize(ApplicationPool iisPoolObject, IISCodeGenerator.IisPoolAndSitesOptions iisOptions)
		{
			this.Key = GetPoolVariableName(iisPoolObject.Name);

			this.AddAttribute("Name", iisPoolObject.Name);

			if (iisOptions.KeepAppPoolsRunning)
				this.AddAttributeWithOverrideValue("AutoStart", true, iisPoolObject.AutoStart);
			else
				this.AddAttribute("AutoStart", iisPoolObject.AutoStart);

			this.AddAttribute("ManagedPipelineMode", iisPoolObject.ManagedPipelineMode.ToString());
			this.AddAttribute("ManagedRuntimeVersion", iisPoolObject.ManagedRuntimeVersion);
			this.AddAttribute("IdentityType", iisPoolObject.ProcessModel.IdentityType.ToString());
			this.AddAttribute("Enable32BitAppOnWin64", iisPoolObject.Enable32BitAppOnWin64);

			if (iisOptions.StandardizeAppPoolRecycles)
			{
				this.AddAttributeWithOverrideValue("RestartSchedule", "@('02:00:00')", GetScheduleString(iisPoolObject));
			}
			else
			{
				this.AddAttribute("RestartSchedule", GetScheduleString(iisPoolObject));
			}
			
			if (iisOptions.KeepAppPoolsRunning)
			{
				this.AddAttributeWithOverrideValue("IdleTimeout", "00:00:00", iisPoolObject.ProcessModel.IdleTimeout.ToString());
				this.AddAttributeWithOverrideValue("RestartTimeLimit", "00:00:00", iisPoolObject.Recycling.PeriodicRestart.Time.ToString());
			}
			else
			{
				this.AddAttribute("IdleTimeout", iisPoolObject.ProcessModel.IdleTimeout.ToString());
				this.AddAttribute("RestartTimeLimit", iisPoolObject.Recycling.PeriodicRestart.Time.ToString());
			}

		}

		internal static string GetPoolVariableName(string poolName)
		{
			return CodeGenHelpers.FormatKey(poolName, "Pool");
		}

		private string GetScheduleString(ApplicationPool iisPoolObject)
		{
			ScheduleCollection scheduleCollection = iisPoolObject.Recycling.PeriodicRestart.Schedule;

			if (scheduleCollection.Count == 0)
				return "";

			string result = "";

			result += string.Join(", ",  scheduleCollection.Select(s => s.Time));

			return   "@('"+ result.Trim() +"')" ;
		}

		protected override string DscObjectType
		{
			get { return "xWebAppPool"; }
		}

	}

}
