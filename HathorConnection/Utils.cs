using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.IO.Compression;

namespace Hathor {
	static class Utils {
		public const int Port = 7418;
		static ImageConverter ICon = new ImageConverter();

		public static byte[] Compress(byte[] B) {
			using (MemoryStream MS = new MemoryStream()) {
				using (GZipStream GZip = new GZipStream(MS, CompressionLevel.Optimal))
					GZip.Write(B, 0, B.Length);
				return MS.ToArray();
			}
		}

		public static byte[] Decompress(byte[] B) {
			using (GZipStream GZipStream = new GZipStream(new MemoryStream(B), CompressionMode.Decompress)) {
				using (MemoryStream Mem = new MemoryStream()) {
					byte[] Buffer = new byte[4096];
					int Cnt = 0;
					do {
						if ((Cnt = GZipStream.Read(Buffer, 0, Buffer.Length)) > 0)
							Mem.Write(Buffer, 0, Cnt);
					} while (Cnt > 0);
					return Mem.ToArray();
				}
			}
		}

		public static void WriteBytes(this NetworkStream NS, byte[] Bytes) {
			Bytes = Compress(Bytes);
			byte[] LenBytes = BitConverter.GetBytes((uint)Bytes.Length);
			NS.Write(LenBytes, 0, sizeof(uint));
			NS.Write(Bytes, 0, Bytes.Length);
			NS.Flush();
		}

		public static byte[] ReadBytes(this NetworkStream NS, out uint Len) {
			byte[] LenBytes = new byte[sizeof(uint)];
			NS.Read(LenBytes, 0, sizeof(uint));
			Len = BitConverter.ToUInt32(LenBytes, 0);
			byte[] Bytes = new byte[Len];
			//NS.Read(Bytes, 0, Bytes.Length);
			for (int i = 0; i < Bytes.Length; i++)
				Bytes[i] = (byte)NS.ReadByte();
			return Decompress(Bytes);
		}

		public static void WriteString(this NetworkStream NS, string Str) {
			NS.WriteBytes(Encoding.Unicode.GetBytes(Str));
		}

		public static string ReadString(this NetworkStream NS) {
			uint Len;
			return Encoding.Unicode.GetString(NS.ReadBytes(out Len));
		}

		public static void WriteImage(this NetworkStream NS, Image Img) {
			NS.WriteBytes((byte[])ICon.ConvertTo(Img, typeof(byte[])));
		}

		public static Image ReadImage(this NetworkStream NS) {
			uint Len;
			return NS.ReadImage(out Len);
		}

		public static Image ReadImage(this NetworkStream NS, out uint Len) {
			return (Image)ICon.ConvertFrom(NS.ReadBytes(out Len));
		}
	}
}