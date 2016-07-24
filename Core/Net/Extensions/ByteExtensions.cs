using System;
using System.Text;

namespace Core
{
	public static class ByteExtensions
	{
		public static string ToString(this byte[] byteArray)
		{
			// I've experienced problems with Encoding.ASCII.GetBytes in the past, so I want to avoid that.
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in byteArray)
				stringBuilder.Append((char)b);
			return stringBuilder.ToString();
		}

		public static byte[] Copy(this byte[] byteArray)
		{
			return byteArray.Copy(byteArray.Length);
		}

		public static byte[] Copy(this byte[] byteArray, int length)
		{
			byte[] dest = new byte[length];
			Array.Copy(byteArray, 0, dest, 0, length);
			return dest;
		}
	}
}
