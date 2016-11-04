using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PlantUmlMonitor
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
			{
				Filter = "Plant UML files (*.uml, *.txt)|*.uml;*.txt"
			};
			dialog.ShowDialog();
			string path = PathBox.Text = dialog.FileName;
			using (var p = Process.Start(new ProcessStartInfo("plantuml.exe", path){WindowStyle = ProcessWindowStyle.Hidden}))
				p.WaitForExit();
			GraphImage.Source = new BitmapImage(new Uri(Path.ChangeExtension(path, ".png")));
		}
	}
}