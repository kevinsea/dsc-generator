using DesiredState.Common;

namespace DesiredState.Windows
{

	internal class WindowsFeature : DesiredStateBase
	{

		public WindowsFeature(string variableName, string featureName, string ensureState
						, string description = "")
		{
			this.Key = CodeGenHelpers.FormatKey(variableName) + "_Feature";
			this.ObjectDescription = description;

			this.AddAttribute("Name", featureName);
			this.AddAttribute("Ensure", ensureState);
		}

		protected override string DscObjectType
		{
			get { return "WindowsFeature"; }
		}
	}

}
