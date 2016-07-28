using System;
using System.Windows;
using Core.Net.Sockets;

namespace Core.Tests.ClientTest
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static Client MyClient { get; private set; }

		[STAThread]
		public static void Main()
		{
			MyClient = new Client();
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}
