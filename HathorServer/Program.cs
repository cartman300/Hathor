﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Diagnostics;
using System.Drawing;

using Con = System.Console;
using System.Reflection;

namespace Hathor {
	static class Console {
		public static void WriteLine(string Msg) {
			Con.WriteLine(Msg);
			File.AppendAllText("log.txt", Msg + "\n");
		}

		public static void WriteLine(string Fmt, params object[] Args) {
			WriteLine(string.Format(Fmt, Args));
		}
	}

	class NetClient {
		public Socket ClientSocket;
		public NetworkStream NStream;
		string Name;

		public bool HasPartner {
			get {
				return Partner != null;
			}
		}

		public bool SearchingForPartner;

		public NetClient Partner {
			get;
			private set;
		}

		public bool IsConnected {
			get {
				return ClientSocket.Connected;
			}
		}

		public void SetPartner(NetClient Partner) {
			if (this.Partner != null)
				this.Partner.SetPartnerInternal(null);
			SetPartnerInternal(Partner);
			if (Partner != null)
				Partner.SetPartnerInternal(this);
		}

		void SetPartnerInternal(NetClient Partner) {
			if (this.Partner == Partner)
				return;
			if (Partner == null)
				SendCommand(CommandType.StrangerDisconnected);
			else if (Partner != null) {
				SearchingForPartner = false;
				SendCommand(CommandType.StrangerConnected);
			}
			this.Partner = Partner;
		}

		public NetClient(Socket S) {
			ClientSocket = S;
			NStream = new NetworkStream(S);
			Name = ClientSocket.RemoteEndPoint.ToString();
		}

		public void SendCommand(CommandType Cmd) {
			try {
				NStream.WriteByte((byte)Cmd);
				NStream.Flush();
			} catch (IOException) {
			}
		}

		public void SendCommand(CommandType Cmd, object Msg, bool DoCompress = true) {
			try {
				NStream.WriteByte((byte)Cmd);
				if (Msg is string)
					NStream.WriteString((string)Msg);
				else if (Msg is byte[])
					NStream.WriteBytes((byte[])Msg, DoCompress);
				else
					throw new NotImplementedException();
				NStream.Flush();
			} catch (IOException) {
			}
		}

		public bool AwaitCommand(CommandType Cmd) {
			if (!IsConnected)
				return false;
			try {
				return (CommandType)NStream.ReadByte() == Cmd;
			} catch (IOException) {
			}
			return false;
		}

		public bool Ping() {
			SendCommand(CommandType.Ping);
			return AwaitCommand(CommandType.PingResponse);
		}

		public void Disconnect() {
			SetPartner(null);
			SendCommand(CommandType.Disconnect);
			NStream.Dispose();
			ClientSocket.Disconnect(false);
		}

		public override string ToString() {
			return Name;
		}
	}

	class Program {
		static List<NetClient> Clients;
		static Random Rand;

		static void Main(string[] args) {
			Console.WriteLine("Initializing server");
			Rand = new Random();

			AppDomain.CurrentDomain.UnhandledException += (S, E) => {
				File.AppendAllText("error.txt", E.ExceptionObject.ToString());
			};

			Process Cur = Process.GetCurrentProcess();
			Process[] Servers = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location));
			for (int i = 0; i < Servers.Length; i++)
				if (Servers[i].Id != Cur.Id) {
					Console.WriteLine("Killing previous instance ({0})", Servers[i].Id);
					Servers[i].Kill();
					Thread.Sleep(1000);
				}

			Clients = new List<NetClient>();

			IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Any, Utils.Port);
			Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			Sock.Bind(LocalEndPoint);
			Sock.Listen(10);

			Console.WriteLine("Listening");
			while (true) {
				Socket Client = Sock.Accept();
				Thread T = new Thread(() => Run(Client));
				T.Start();
			}
		}

		static void DropClient(NetClient Client) {
			if (Clients.Contains(Client)) {
				Clients.Remove(Client);
				if (Client.HasPartner)
					Client.SetPartner(null);
				Console.WriteLine("Dropping {0}", Client);
				Client.Disconnect();
			}
		}

		static void Run(Socket Client) {
			NetClient NC = new NetClient(Client);
			Console.WriteLine("Client {0} connected", Client.RemoteEndPoint);
			Clients.Add(NC);

			int PingCounter = 0;
			while (NC.IsConnected) {
				if (NC.NStream.DataAvailable) {
					CommandType Cmd = (CommandType)NC.NStream.ReadByte();
					switch (Cmd) {
						case CommandType.Disconnect:
							DropClient(NC);
							break;
						case CommandType.RequestStranger:
							if (NC.HasPartner)
								NC.SendCommand(CommandType.InvalidRequest);
							else {
								NC.SearchingForPartner = true;
								foreach (int i in Enumerable.Range(0, Clients.Count).OrderBy(X => Rand.Next())) {
									NetClient Stranger = Clients[i];
									if (Stranger != null && Stranger != NC && !Stranger.HasPartner &&
										Stranger.SearchingForPartner && NC.SearchingForPartner) {
										Console.WriteLine("Connecting {0} with {1}", NC, Stranger);
										NC.SetPartner(Stranger);
										break;
									}
								}
							}
							break;
						case CommandType.DropStranger:
							if (NC.HasPartner) {
								Console.WriteLine("Disconnecting {0} and {1}", NC, NC.Partner);
								NC.SetPartner(null);
							}
							break;
						case CommandType.Ping:
							NC.SendCommand(CommandType.PingResponse);
							break;
						case CommandType.SendMessage:
							{
								string Msg = NC.NStream.ReadString();
								Console.WriteLine("{0}: {1}", NC, Msg.Trim());

								if (NC.HasPartner)
									NC.Partner.SendCommand(CommandType.ReceiveMessage, Msg);
								else
									NC.SendCommand(CommandType.InvalidRequest);
								break;
							}
						case CommandType.SendImage:
							{
								uint Len;
								byte[] Img = NC.NStream.ReadBytes(out Len, false);
								Console.WriteLine("Image[{1}] from {0}", NC, Len);

								if (NC.HasPartner)
									NC.Partner.SendCommand(CommandType.ReceiveImage, Img, false);
								else
									NC.SendCommand(CommandType.InvalidRequest);
								break;
							}
						case CommandType.PingResponse:
							break;
						default:
							Console.WriteLine("Invalid message {0} from {1}", Cmd, NC);
							//DropClient(NC);
							break;
					}
				}
				if (PingCounter++ >= 500) {
					PingCounter = 0;
					NC.Ping();
				}
				Thread.Sleep(10);
			}

			DropClient(NC);
		}
	}
}