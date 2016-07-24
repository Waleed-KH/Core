using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Core.Net.Sockets
{
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
			_socket = socket;
			_buffer = new byte[_socket.ReceiveBufferSize];
		}

		#region Properties
		/// <summary>
		/// Gets a value indicating whether the Client is connected to the Server.
		/// </summary>
		public bool Connected { get { return _socket?.Connected ?? false; } }
		/// <summary>
		/// Gets the client remote IP endpoint.
		/// </summary>
		public IPEndPoint RemoteIPEndPoint { get { return (IPEndPoint)_socket?.Client.RemoteEndPoint ?? null; } }
		/// <summary>
		/// An object which is the owner of the client.
		/// </summary>
		public object Owner { get; set; }
		#endregion

		#region Events
		/// <summary>
		/// An event raised when the client is disconnecting.
		/// </summary>
		public event ClientConnection OnDisconnect;
		/// <summary>
		/// An event raised when the client has send data to the server.
		/// </summary>
		public event ClientReceive OnReceive;
		#endregion

		public async void ConnectAsync(string host, int port)
		{
			await _socket.ConnectAsync(host, port);
			StartReceive();
		}

		public async void ConnectAsync(IPAddress address, int port)
		{
			await _socket.ConnectAsync(address, port);
			StartReceive();
		}

		/// <summary>
		/// Disconnects the client and requests that the underlying TCP connection be closed.
		/// </summary>
		public void Disconnect()
		{
			if (!Connected)
				return;

			Dispose();
			Initialize();

			OnDisconnect?.Invoke(this);
		}
		private async void StartReceive()
		{
			if (!Connected)
				return;

			try
			{
				int length = await _socket.Client.ReceiveAsync(new ArraySegment<byte>(_buffer), SocketFlags.None);
				if (length > 0)
					OnReceive?.Invoke(this, _buffer.Copy(length));
				Console.WriteLine("Received data from Client " + length);
				StartReceive();
			}
			catch (SocketException ex) { Console.WriteLine("BeginReceiveSocketException: " + ex); Disconnect(); }
			catch (ObjectDisposedException ex) { Console.WriteLine("BeginReceiveDisposedException: " + ex); Dispose(false); }
			catch (Exception ex) { Console.WriteLine("BeginReceiveException: " + ex); }
		}

		public async void Send(byte[] data)
		{
			if (!Connected)
				return;

			try { await _socket.Client.SendAsync(new ArraySegment<byte>(data), SocketFlags.None); }
			catch (SocketException ex) { Console.WriteLine("SendSocketException: " + ex); Disconnect(); }
			catch (ObjectDisposedException ex) { Console.WriteLine("SendDisposedException: " + ex); Dispose(false); }
			catch (Exception ex) { Console.WriteLine("SendException: " + ex); }
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
				OnDisconnect = null;
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
