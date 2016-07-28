using System.Net;
using System.Windows;
using Core.Net.Sockets;

namespace Core.Tests.ClientTest
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
			App.MyClient.OnStatusChanged += MyClient_OnStatusChanged;

		}
		private void MyClient_OnStatusChanged(Client server, ClientStatus status)
		{
			ClientSwitch.IsEnabled = true;
			if (status == ClientStatus.Connected)
				ClientSwitch.Content = "Disconnect";
			else
				ClientSwitch.Content = "Connect";
		}
		private void ConsoleWriter_WriteEvent(object sender, ConsoleWriterEventArgs e)
		{
			logText.Text += e.Value;
		}

		private void ClientSwitch_Click(object sender, RoutedEventArgs e)
		{
			ClientSwitch.IsEnabled = false;
			if (!App.MyClient.Connected)
				App.MyClient.ConnectAsync(IPAddress.Parse("192.168.1.40"), 2741);
			else
				App.MyClient.Disconnect();
		}


	}
}
