using System.Windows;
using Core.Net.Sockets;

namespace Core.Tests.ServerTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			ConsoleWriter.Initialize();
			ConsoleWriter.WriteEvent += ConsoleWriter_WriteEvent;
			App.MyServer.OnStatusChanged += MyServer_OnStatusChanged;
		}


		private void MyServer_OnStatusChanged(Server server, ServerStatus status)
		{
			ServerSwitch.IsEnabled = true;
			if (status == ServerStatus.Active)
				ServerSwitch.Content = "Stop";
			else
				ServerSwitch.Content = "Start";
		}

		private void ConsoleWriter_WriteEvent(object sender, ConsoleWriterEventArgs e)
		{
			logText.Text += e.Value;
		}

		private void ServerSwitch_Click(object sender, RoutedEventArgs e)
		{
			ServerSwitch.IsEnabled = false;
			if (!App.MyServer.Active)
				App.MyServer.Start();
			else
				App.MyServer.Stop();
		}

	}
}
