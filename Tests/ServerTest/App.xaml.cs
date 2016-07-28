using System;
using System.Windows;
using Core.Net.Sockets;

namespace Core.Tests.ServerTest
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static Server MyServer { get; private set; }

		/// <summary>
		/// Application Entry Point.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			MyServer = new Server(2741);
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}
