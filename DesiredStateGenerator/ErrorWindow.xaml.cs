using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesiredState
{
	/// <summary>
	/// Interaction logic for Error.xaml
	/// </summary>
	public partial class ErrorWindow : Window
	{

		public ErrorWindow(Exception ex)
		{
			InitializeComponent();

			ErrorMsgCtl.Text = $"{ex.Message} (Type: {ex.GetType().Name})";

			StackTraceCtl.Text = ex.StackTrace;

			Mouse.OverrideCursor = null;
		}

		private void CopyCtl_Click(object sender, RoutedEventArgs e)
		{
			string errMsg = "Error Message:\n   " + ErrorMsgCtl.Text + "\n\n";
			errMsg += "Stack Trace:\n" + StackTraceCtl.Text;

			Clipboard.SetText(errMsg);
		}

		private void OkCtl_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

	}
}
