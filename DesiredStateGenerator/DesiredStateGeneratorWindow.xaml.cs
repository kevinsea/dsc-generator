using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DesiredState.IIS;
using DesiredState.Common;

namespace DesiredState
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class DesiredStateGeneratorWindow : Window
	{
		public DesiredStateGeneratorWindow()
		{
			InitializeComponent();
		}

		private void GenerateDscCtl_Click(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			try
			{
				CodeGenerator.Options options = GetCodeGenOptions();

				CodeGenerator sm = new CodeGenerator();

				ResultsCtl.Text = sm.GenerateConfig(options);
			}
			catch (UnauthorizedAccessException)
			{
				ShowErrorMessage("Error: you don't have admin rights or UAC is enabled.  Please re-run as an administrator. (UnauthorizedAccessException)");
			}

			catch (System.IO.DirectoryNotFoundException ex)
			{
				ShowErrorMessage("Is IIS installed?\n\n A DirectoryNotFoundException occurred: " + ex.Message);
			}

			catch (Exception ex)
			{
				ShowExceptionDialog(ex);
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		private CodeGenerator.Options GetCodeGenOptions()
		{
			var options = new CodeGenerator.Options();
			options.GenerateIisWindowsFeatures = GenerateIisFeaturesCtl.IsChecked.GetValueOrDefault();
			options.IisOptions = GetIisOptions();

			return options;
		}

		private IISCodeGenerator.IisPoolAndSitesOptions GetIisOptions()
		{
			var options = new IISCodeGenerator.IisPoolAndSitesOptions();

			options.StandardizeAppPoolRecycles = StandardizeAppPoolRecyclesCtl.IsChecked.GetValueOrDefault();
			options.KeepAppPoolsRunning = KeepAppPoolsRunningCtl.IsChecked.GetValueOrDefault();
			options.StandardizeLogFileLocation = StandardizeLogFileLocationCtl.IsChecked.GetValueOrDefault();

			if (GenerateIISSitesPoolsCtl.IsChecked.GetValueOrDefault() == false)
				options.IisPoolAndSitesGenerationMode = IISCodeGenerator.IisPoolAndSitesGenerationMode.NoGeneration;

			else if (GenerateIisSitesPoolsSortedCtl.IsChecked.GetValueOrDefault())
				options.IisPoolAndSitesGenerationMode = IISCodeGenerator.IisPoolAndSitesGenerationMode.Alphabetical;

			else
				options.IisPoolAndSitesGenerationMode = IISCodeGenerator.IisPoolAndSitesGenerationMode.ConfigFileOrder;

			return options;
		}

		private void ShowErrorMessage(string errorMsg)
		{
			Trace.TraceError(errorMsg);

			MessageBox.Show(errorMsg, "An Error Occurred.");
		}

		private void ShowExceptionDialog(Exception ex)
		{
			Trace.TraceError(ex.Message);

			var window = new ErrorWindow(ex);
			window.ShowDialog();
		}

	}
}
