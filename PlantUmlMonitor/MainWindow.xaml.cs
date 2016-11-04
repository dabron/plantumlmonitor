using Microsoft.Win32;
using System.Windows;

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
				Filter = "Plant UML Files (*.uml, *.txt);*.uml;*.txt"
			};
			dialog.ShowDialog();
			PathBox.Text = dialog.FileName;
		}
	}
}