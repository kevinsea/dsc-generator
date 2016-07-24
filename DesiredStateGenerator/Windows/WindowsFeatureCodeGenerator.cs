using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesiredState.Windows
{
    /// <summary>
    ///  Generates Windows Feature DSC
    /// </summary>
    internal class WindowsFeatureCodeGenerator
    {

        public string GenerateWindowsFeatures()
        {
            string code = "";
            StringBuilder sb = new StringBuilder();

            var fr = new WindowsFeatureReader();
            List<WindowsFeature> features = fr.GetFeatures();

            foreach (var windowsFeature in features)
            {
                code = windowsFeature.GetCode(2);
                sb.AppendLine(code);
            }

            return sb.ToString();
        }

    }
}
