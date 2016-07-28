using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Core.Net.Sockets
{
	public enum ServerStatus { Inactive, Active }
	public delegate void ServerStatusChanged(Server server, ServerStatus status);

	public class Server
	{
		private TcpListener _socket;
		private bool _active;

		#region Constructors
		/// <summary>
		/// Initializes a new socket server that listens on the specified port.
		/// </summary>
		/// <param name="port">The port on which to listen for incoming connection attempts.</param>
		public Server(ushort port)
			: this(new TcpListener(IPAddress.Any, port))
		{
		}

		/// <summary>
		/// Initializes a new socket server that listens to the specified IP address and port.
		/// </summary>
		/// <param name="ipString">An IP String that represents the local IP address.</param>
		/// <param name="port">The port on which to listen for incoming connection attempts.</param>
		public Server(string ipString, ushort port)
			: this(IPAddress.Parse(ipString), port)
		{
		}

		/// <summary>
		/// Initializes a new socket server that listens to the specified IP address and port.
		/// </summary>
		/// <param name="ip">An System.Net.IPAddress that represents the local IP address.</param>
		/// <param name="port">The port on which to listen for incoming connection attempts.</param>
		public Server(IPAddress ip, ushort port)
			: this(new TcpListener(ip, port))
		{
		}
		private Server(TcpListener server)
		{
			_socket = server;

			OnClientConnect = null;
			OnClientReceive = null;
			OnClientDisconnect = null;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the server local IP endpoint.
		/// </summary>
		public IPEndPoint LocalIPEndpoint { get { return (IPEndPoint)_socket.LocalEndpoint; } }
		/// <summary>
		/// Gets a value that indicates whether the Server is actively listening for client connections.
		/// </summary>
		public bool Active { get { return _active; } }
		#endregion

		#region Events
		public event ServerStatusChanged OnStatusChanged;
		/// <summary>
		/// An event raised when a client is connected.
		/// </summary>
		public event ClientConnection OnClientConnect;
		/// <summary>
		/// An event raised when a client is disconnecting.
		/// </summary>
		public event ClientConnection OnClientDisconnect;
		/// <summary>
		/// An event raised when a client has send data to the server.
		/// </summary>
		public event ClientReceive OnClientReceive;
		#endregion

		/// <summary>
		/// Starts listening for incoming connection requests.
		/// </summary>
		public void Start()
		{
			if (_active)
				return;

			try
			{
				_socket.Start();
				Console.WriteLine("ServerStart");
				_active = true;
				AcceptClients();
				OnStatusChanged?.Invoke(this, ServerStatus.Active);
			}
			catch (SocketException ex)
			{
				Console.WriteLine("StartSocketException: " + ex);
			}
		}
		/// <summary>
		/// Closes the listener.
		/// </summary>
		public void Stop()
		{
			if (!_active)
				return;

			try
			{
				_socket.Stop();
				Console.WriteLine("ServerStop");
				_active = false;
				OnStatusChanged?.Invoke(this, ServerStatus.Inactive);
			}
			catch (SocketException ex)
			{
				Console.WriteLine("StopSocketException: " + ex);
			}
		}
		private async void AcceptClients()
		{
			if (!_active)
				return;

			try
			{
				Console.WriteLine("Waiting for a Client");
				InitializeClient(await _socket.AcceptTcpClientAsync());
			}
			catch (SocketException ex) { Console.WriteLine("AcceptClientsSocketException: " + ex); }
			catch (ObjectDisposedException ex) { Console.WriteLine("AcceptClientsObjectDisposedException: " + ex); }
			catch (Exception ex) { Console.WriteLine("AcceptClientsException: " + ex); }
			finally { AcceptClients(); }
		}
		/// <summary>
		/// Initialize an accepted Client.
		/// </summary>
		/// <param name="socket">The Socket of the Client.</param>
		private void InitializeClient(TcpClient socket)
		{
			Console.WriteLine("Accept Client");
			Client client = new Client(socket);
			client.OnReceive += OnClientReceive;
			client.OnStatusChanged += (c, s) =>
			{
				if (s == ClientStatus.Disconnected)
					OnClientDisconnect?.Invoke(c);
			};
			OnClientConnect?.Invoke(client);
		}
	}
}
