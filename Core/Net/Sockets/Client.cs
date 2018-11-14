using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Core.Net.Sockets
{
	public enum ClientStatus { Connected, Disconnected }
	public delegate void ClientStatusChanged(Client server, ClientStatus status);

	/// <summary>
	/// A delegate associated with socket-connection events.
	/// </summary>
	public delegate void ClientConnection(Client client);
	/// <summary>
	/// A delegate associated with socket-buffer events.
	/// </summary>
	public delegate void ClientReceive(Client client, byte[] buffer);
	public class Client : IDisposable
	{
		private TcpClient _socket;
		private byte[] _buffer;
		/// <summary>
		/// Initializes a new socket client and connects to the specified port on the specified host.
		/// </summary>
		public Client()
		{
			Initialize();
		}
		/// <summary>
		/// Initializes a new socket client that connected to a server.
		/// </summary>
		/// <param name="socket">The Socket of the Client.</param>
		internal Client(TcpClient socket)
		{
			Initialize(socket);
		}

		private void Initialize()
		{
			Initialize(new TcpClient());
		}
		private void Initialize(TcpClient socket)
		{
			Console.WriteLine("Initialize Client");
			_socket = socket;
			_buffer = new byte[_socket.ReceiveBufferSize];
			if (_socket.Connected)
			{
				Console.WriteLine("Client Connected, ReceiveBufferSize: " + _buffer.Length);
				StartReceive();
			}
		}

		#region Properties
		/// <summary>
		/// Gets a value indicating whether the Client is connected to the Server.
		/// </summary>
		public bool Connected => _socket?.Connected ?? false;
		/// <summary>
		/// Gets the client remote IP endpoint.
		/// </summary>
		public IPEndPoint RemoteIPEndPoint => (IPEndPoint)_socket?.Client.RemoteEndPoint ?? null;
		/// <summary>
		/// An object which is the owner of the client.
		/// </summary>
		public object Owner { get; set; }
		#endregion

		#region Events
		/// <summary>
		/// An event raised when the client is disconnecting.
		/// </summary>
		public event ClientStatusChanged OnStatusChanged;
		/// <summary>
		/// An event raised when the client has send data to the server.
		/// </summary>
		public event ClientReceive OnReceive;
		#endregion

		public async void ConnectAsync(string host, int port)
		{
			try
			{
				if (!Connected)
				{
					await _socket.ConnectAsync(host, port);
					OnConnected();
				}
			}
			catch (SocketException ex)
			{
				Console.WriteLine("ClientConnectSocketException: " + ex);
			}
		}

		public async void ConnectAsync(IPAddress address, int port)
		{
			try
			{
				if (!Connected)
				{
					await _socket.ConnectAsync(address, port);
					OnConnected();
				}
			}
			catch (SocketException ex)
			{
				Console.WriteLine("ClientConnectSocketException: " + ex);
			}
		}

		/// <summary>
		/// Disconnects the client and requests that the underlying TCP connection be closed.
		/// </summary>
		public void Disconnect()
		{
			_socket.Dispose();
			_socket = null;
			_buffer = null;
			Initialize();

			OnStatusChanged?.Invoke(this, ClientStatus.Disconnected);
		}
		public async void SendAsync(byte[] data)
		{
			if (!Connected || data?.Length <= 0)
				return;

			try { await _socket.Client.SendAsync(new ArraySegment<byte>(data), SocketFlags.None); }
			catch (SocketException ex) { Console.WriteLine("SendSocketException: " + ex); Disconnect(); }
			catch (ObjectDisposedException ex) { Console.WriteLine("SendDisposedException: " + ex); Dispose(false); }
			catch (Exception ex) { Console.WriteLine("SendException: " + ex); }
		}
		private async void StartReceive()
		{
			if (!Connected)
				return;

			Console.WriteLine("Start Recieve");

			try
			{
				int length = await _socket.Client.ReceiveAsync(new ArraySegment<byte>(_buffer), SocketFlags.None);
				if (length > 0)
					OnReceive?.Invoke(this, _buffer.Copy(length));
				else
					throw new SocketException();
				Console.WriteLine("Received data from Client " + length);
				StartReceive();
			}
			catch (SocketException ex) { Console.WriteLine("BeginReceiveSocketException: " + ex); }
			catch (ObjectDisposedException ex) { Console.WriteLine("BeginReceiveDisposedException: " + ex); }
			catch (Exception ex) { Console.WriteLine("BeginReceiveException: " + ex); }
		}
		private void OnConnected()
		{
			StartReceive();
			OnStatusChanged?.Invoke(this, ClientStatus.Connected);
		}
		private void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					_socket.Dispose();
			}
			catch { }
			finally
			{
				OnStatusChanged?.Invoke(this, ClientStatus.Disconnected);
				OnStatusChanged = null;
				OnReceive = null;
				_socket = null;
				_buffer = null;
				Owner = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		~Client()
		{
			Dispose(false);
		}
	}
}
