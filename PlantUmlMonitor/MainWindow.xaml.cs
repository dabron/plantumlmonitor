using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PlantUmlMonitor
{
	public partial class MainWindow : Window
	{
		private readonly object _lock;
		private DateTime _lastGenerated;

		public MainWindow()
		{
			_lock = new object();
			ResetLastGenerated();
			InitializeComponent();
			SetWorking(false);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			new Task(() =>
			{
				while (true)
				{
					try
					{
						string path = string.Empty;
						Dispatcher.Invoke(() => path = PathBox.Text);
						if (!string.IsNullOrWhiteSpace(path))
						{
							bool changed = false;
							lock (_lock)
							{
								var lastChanged = File.GetLastWriteTime(path);
								if (lastChanged > _lastGenerated)
								{
									changed = true;
									_lastGenerated = lastChanged;
								}
							}
							if (changed)
								GenerateImage(path);
						}
					}
					catch (Exception ex)
					{
						SetError(ex.Message);
						SetWorking(false);
					}
					Thread.Sleep(100);
				}
			}).Start();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
			{
				Filter = "Plant UML files (*.uml, *.txt)|*.uml;*.txt"
			};
			dialog.ShowDialog();
			SetPath(dialog.FileName);
		}

		private void ResetLastGenerated()
		{
			_lastGenerated = DateTime.MinValue;
		}

		private void SetPath(string path)
		{
			lock (_lock)
			{
				PathBox.Text = path;
				ResetLastGenerated();
			}
		}

		private void SetWorking(bool working)
		{
			Dispatcher.Invoke(() =>
			{
				if (working)
				{
					WorkingBar.Visibility = Visibility.Visible;
					WorkingBar.IsIndeterminate = true;
				}
				else
				{
					WorkingBar.Visibility = Visibility.Hidden;
					WorkingBar.IsIndeterminate = false;
				}
			});
		}

		private void SetError(string message)
		{
			Dispatcher.Invoke(() =>
			{
				if (string.IsNullOrEmpty(message))
				{
					ErrorBox.Visibility = Visibility.Hidden;
					ScrollViewer.Visibility = Visibility.Visible;
				}
				else
				{
					ScrollViewer.Visibility = Visibility.Hidden;
					ErrorBox.Text = message;
					ErrorBox.Visibility = Visibility.Visible;
				}
			});
		}

		private void GenerateImage(string path)
		{
			SetWorking(true);
			var startInfo = new ProcessStartInfo("plantuml.exe", path) { WindowStyle = ProcessWindowStyle.Hidden };
			using (var p = new Process() { StartInfo = startInfo })
			{
				p.Start();
				p.WaitForExit();
				if (p.ExitCode != 0)
					throw new Exception("Exited with error code " + p.ExitCode);
			}
			Dispatcher.Invoke(() =>
			{
				var source = new BitmapImage();
				source.BeginInit();
				source.CacheOption = BitmapCacheOption.OnLoad;
				source.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
				source.UriSource = new Uri(Path.ChangeExtension(path, ".png"));
				source.EndInit();
				GraphImage.Source = source;
			});
			SetError(string.Empty);
			SetWorking(false);
		}
	}
}