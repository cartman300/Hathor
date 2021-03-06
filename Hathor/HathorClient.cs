﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;using System.Drawing;
using System.Net;
using System.Net.Sockets;

namespace Hathor {
	class HathorClient {
		//static IPAddress ServerIP = IPAddress.Parse("127.0.0.1");
		static IPAddress ServerIP = IPAddress.Parse("51.254.129.74");
		//static IPAddress ServerIP = IPAddress.Parse("178.150.102.66"); // Mij

		Socket Server;
		NetworkStream NStream;

		public HathorClient() {
			StrangerConnected += () => IsStrangerConnected = true;
			StrangerDisconnected += () => IsStrangerConnected = false;
		}

		public bool IsConnected {
			get {
				if (Server == null)
					return false;
				return Server.Connected;
			}
		}

		public bool IsStrangerConnected {
			get;
			private set;
		}

		public event Action Disconnected, StrangerConnected, StrangerDisconnected;
		public event Action<string> MessageReceived;
		public event Action<Image> ImageReceived;
		public event Action<Exception> FailedToConnect;

		public void Run() {
			while (true) {
				Tick();
				Thread.Sleep(50);
			}
		}

		public void Tick() {
			if (!IsConnected)
				return;
			if (NStream.DataAvailable) {
				CommandType Cmd = (CommandType)NStream.ReadByte();
				switch (Cmd) {
					case CommandType.Disconnect:
						Disconnect(false);
						break;
					case CommandType.StrangerConnected:
						if (StrangerConnected != null)
							StrangerConnected();
						break;
					case CommandType.StrangerDisconnected:
						if (StrangerDisconnected != null)
							StrangerDisconnected();
						break;
					case CommandType.Ping:
						SendCommand(CommandType.PingResponse);
						break;
					case CommandType.ReceiveMessage:
						{
							string Msg = NStream.ReadString();
							if (MessageReceived != null)
								MessageReceived(Msg);
							break;
						}
					case CommandType.ReceiveImage:
						{
							Image Img = NStream.ReadImage();
							if (ImageReceived != null)
								ImageReceived(Img);
							break;
						}
				}
			}
		}

		public Exception Connect() {
			try {
				if (Server == null)
					Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				if (IsConnected)
					Disconnect();
				Server.Connect(new IPEndPoint(ServerIP, Utils.Port));
				NStream = new NetworkStream(Server);
			} catch (Exception E) {
				if (FailedToConnect != null)
					FailedToConnect(E);
				return E;
			}
			return null;
		}

		public void Disconnect(bool SendDisconnectCommand = true) {
			if (SendDisconnectCommand)
				SendCommand(CommandType.Disconnect);
			NStream.Close();
			NStream.Dispose();
			Server.Disconnect(true);
			if (Disconnected != null)
				Disconnected();
		}

		public Exception RequestStranger() {
			Exception Ex = null;
			if (!IsConnected)
				Ex = Connect();
			SendCommand(CommandType.RequestStranger);
			return Ex;
		}

		public void DisconnectFromStranger() {
			SendCommand(CommandType.DropStranger);
		}

		public bool SendMessage(string Msg) {
			return SendCommand(CommandType.SendMessage, Msg);
		}

		public bool SendImage(Image Img) {
			return SendCommand(CommandType.SendImage, Img);
		}

		public bool SendCommand(CommandType Cmd) {
			if (IsConnected) {
				NStream.WriteByte((byte)Cmd);
				NStream.Flush();
				return true;
			}
			return false;
		}

		public bool SendCommand(CommandType Cmd, object Msg) {
			if (IsConnected) {
				NStream.WriteByte((byte)Cmd);
				if (Msg is string)
					NStream.WriteString((string)Msg);
				else if (Msg is Image)
					NStream.WriteImage((Image)Msg);
				else
					throw new NotImplementedException();
				NStream.Flush();
				return true;
			}
			return false;
		}
	}
}