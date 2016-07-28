using System;
using System.IO;
using System.Text;

namespace Core
{
	public class ConsoleWriterEventArgs : EventArgs
	{
		public string Value { get; private set; }
		public ConsoleWriterEventArgs(string value)
		{
			Value = value;
		}
	}
	public class ConsoleWriter : TextWriter
	{
		private static ConsoleWriter _writer;
		private static TextWriter _defaultWriter;
		private static Encoding _encoding;

		public static event EventHandler<ConsoleWriterEventArgs> WriteEvent;

		private ConsoleWriter() { }
		static ConsoleWriter()
		{
			_defaultWriter = Console.Out;
			_writer = new ConsoleWriter();
			_encoding = Encoding.UTF8;
		}

		public static void Initialize()
		{
			Console.SetOut(_writer);
		}

		public static void Reset()
		{
			Console.SetOut(_defaultWriter);
		}

		public static void SetEncoding(Encoding encoding)
		{
			_encoding = encoding;
		}

		public override Encoding Encoding { get { return _encoding; } }
		public override void Write(string value)
		{
			WriteEvent?.Invoke(this, new ConsoleWriterEventArgs(value));
			//base.Write(value);
		}

		public static void Test()
		{
			Console.WriteLine("Hello");
		}

		//public override void WriteLine(string value)
		//{
		//	//WriteEvent?.Invoke(this, new ConsoleWriterEventArgs(value));
		//	//base.WriteLine(value);
		//}

		public override void Write(char value)
		{
			Write(value.ToString());
		}

	}
}
