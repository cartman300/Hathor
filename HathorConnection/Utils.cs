using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Drawing;

namespace Hathor {
	static class Utils {
		static ImageConverter ICon = new ImageConverter();

		public static void WriteBytes(this NetworkStream NS, byte[] Bytes) {
			byte[] LenBytes = BitConverter.GetBytes((uint)Bytes.Length);
			NS.Write(LenBytes, 0, sizeof(uint));
			NS.Write(Bytes, 0, Bytes.Length);
		}

		public static byte[] ReadBytes(this NetworkStream NS) {
			byte[] LenBytes = new byte[sizeof(uint)];
			NS.Read(LenBytes, 0, sizeof(uint));
			int Len = BitConverter.ToInt32(LenBytes, 0);
			byte[] Bytes = new byte[Len];
			NS.Read(Bytes, 0, Len);
			return Bytes;
		}

		public static void WriteString(this NetworkStream NS, string Str) {
			NS.WriteBytes(Encoding.Unicode.GetBytes(Str));
		}

		public static string ReadString(this NetworkStream NS) {
			return Encoding.Unicode.GetString(NS.ReadBytes());
		}

		public static void WriteImage(this NetworkStream NS, Image Img) {
			NS.WriteBytes((byte[])ICon.ConvertTo(Img, typeof(byte[])));

		}

		public static Image ReadImage(this NetworkStream NS) {
			return (Image)ICon.ConvertFrom(NS.ReadBytes());
		}
	}
}