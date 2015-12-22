using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Hathor {
	static class Utils {
		public static void WriteString(this NetworkStream NS, string Str) {
			byte[] StringBytes = Encoding.Unicode.GetBytes(Str);
			byte[] LenBytes = BitConverter.GetBytes((int)StringBytes.Length);
			NS.Write(LenBytes, 0, sizeof(int));
			NS.Write(StringBytes, 0, StringBytes.Length);
		}

		public static string ReadString(this NetworkStream NS) {
			byte[] LenBytes = new byte[sizeof(int)];
			NS.Read(LenBytes, 0, sizeof(int));
			int Len = BitConverter.ToInt32(LenBytes, 0);
			byte[] StrBytes = new byte[Len];
			NS.Read(StrBytes, 0, Len);
			return Encoding.Unicode.GetString(StrBytes);
		}
	}
}