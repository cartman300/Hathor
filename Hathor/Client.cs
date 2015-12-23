using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Gitdate;

namespace Hathor {
	enum MessageSender {
		Server, Info, You, Stranger
	}

	public partial class Client : Form {
		HathorClient HC;
		Font Regular, Bold;

		public Client() {
			InitializeComponent();

			GotFocus += Client_GotFocus;
			Input.KeyPress += Input_KeyPress;
			Input.AllowDrop = true;
			Input.DragEnter += Input_DragEnter;
			Input.DragDrop += Input_DragDrop;
			Input.TabIndex = 0;

			Output.LinkClicked += (S, E) => Process.Start(E.LinkText);
			Output.Click += Output_Click;

			FontFamily FntFam = SystemFonts.MessageBoxFont.FontFamily;
			Regular = new Font(FntFam, 12, FontStyle.Regular);
			Bold = new Font(FntFam, 12, FontStyle.Bold);

			Btn.Text = "New";
		}

		private void Output_Click(object sender, EventArgs e) {
			string SelectedRtf = Output.SelectedRtf;
			if (Output.SelectionType == RichTextBoxSelectionTypes.Object && SelectedRtf.IndexOf(@"\pict\wmetafile") != -1) {
				//WriteText("Image clicked! " + SelectedRtf.GetHashCode(), MessageSender.Info);
			}
		}

		private void Client_GotFocus(object sender, EventArgs e) {
			ActiveControl = Input;
			Input.Focus();
		}

		private void Input_DragEnter(object sender, DragEventArgs e) {
			e.Effect = DragDropEffects.Copy;
		}

		private void Input_DragDrop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.Text))
				Input.Text += e.Data.GetData(DataFormats.Text).ToString();
			else if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				string In = GetInput();
				if (In.Length > 0)
					OnInput(In);
				string[] FileDrop = (string[])e.Data.GetData(DataFormats.FileDrop);
				for (int i = 0; i < FileDrop.Length; i++) {
					try {
						OnInput(Image.FromFile(FileDrop[i]));
					} catch (Exception) {
					}
				}
			}
		}

		private void Input_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)Keys.Enter) {
				e.Handled = true;
				string Txt = GetInput();
				if (Txt.Length > 0)
					OnInput(Txt);
			}
		}

		string GetInput() {
			string In = Input.Text.Trim();
			Input.Text = "";
			return In;
		}

		void OnInput(object Obj) {
			bool Sent = false;

			if (HC.IsStrangerConnected)
				Btn.Text = "Disconnect";
			if (Obj is string) {
				string Txt = (string)Obj;
				WriteText(Txt, MessageSender.You);
				Sent = HC.SendMessage(Txt);
			} else if (Obj is Image) {
				Image Img = (Image)Obj;
				WriteText("", MessageSender.You);
				WriteText(Img.ToRtf(), MessageSender.You, true);
				Sent = HC.SendImage(Img);
			}

			if (Sent)
				Status.Text = "Message sent";
			else
				Status.Text = "Message failed to send";
		}

		void OnMessage(object Msg) {
			Invoke(new Action(() => {
				Activate();
				if (Msg is string)
					WriteText((string)Msg, MessageSender.Stranger);
				else if (Msg is Image) {
					WriteText("", MessageSender.Stranger);
					WriteText(((Image)Msg).ToRtf(), MessageSender.Stranger, true);
				} else
					throw new NotImplementedException();
			}));
		}

		string GetCurTime() {
			return DateTime.Now.ToShortTimeString();
		}

		void WriteText(string In, MessageSender MS = MessageSender.Server, bool Raw = false) {
			Invoke(new Action(() => {
				Output.DeselectAll();
				Output.Select(Output.TextLength, 0);
				if (Raw) {
					Output.SelectedRtf = In;
					Output.AppendText("\n");
					return;
				}

				Output.SelectionFont = Bold;
				if (MS == MessageSender.Server) {
					Output.SelectionColor = Color.BlueViolet;
				} else if (MS == MessageSender.You) {
					Output.SelectionColor = Color.Blue;
				} else if (MS == MessageSender.Stranger) {
					Output.SelectionColor = Color.Green;
				} else if (MS == MessageSender.Info) {
					Output.SelectionColor = Color.DarkCyan;
					Status.Text = In;
				}
				if (MS != MessageSender.Info) {
					Output.AppendText(GetCurTime());
					Output.AppendText(" - ");
					Output.AppendText(MS.ToString());
					Output.AppendText(": ");
					ResetTextStyle();
				}
				Output.AppendText(In.Trim() + "\n");
				ResetTextStyle();
				Output.ScrollToCaret();
			}));
		}

		void ResetTextStyle() {
			Output.SelectionFont = Regular;
			Output.SelectionColor = Color.Black;
		}

		private void Client_Load(object sender, EventArgs e) {
			new Thread(() => {
				Updater.Username = "cartman300";
				Updater.Repository = "Hathor";
				Updater.CheckAndUpdate((L) => {
					WriteText("Downloading update " + L.tag_name, MessageSender.Info);
				});
			}).Start();

			HC = new HathorClient();
			HC.StrangerConnected += () => {
				Output.Invoke(new Action(Output.Clear));
				Btn.Invoke(new Action(() => Btn.Enabled = true));
				WriteText("Stranger has connected", MessageSender.Info);
			};
			HC.StrangerDisconnected += () => {
				Btn.Text = "New";
				WriteText("Stranger has disconnected", MessageSender.Info);
			};
			HC.MessageReceived += OnMessage;
			HC.ImageReceived += OnMessage;

			Thread RunThread = new Thread(HC.Run);
			RunThread.IsBackground = true;
			RunThread.Start();
		}

		private void Btn_Click(object sender, EventArgs e) {
			if (Btn.Text == "New") {
				Btn.Enabled = false;
				Output.Invoke(new Action(Output.Clear));
				Exception E = null;
				if ((E = HC.RequestStranger()) == null) {
					Btn.Text = "Disconnect";
					WriteText("Waiting for stranger", MessageSender.Info);
				} else {
					Btn.Enabled = true;
					WriteText(E.Message, MessageSender.Info);
					WriteText("Failed to connect to master server, please try again", MessageSender.Info);
				}
			} else if (Btn.Text == "Disconnect") {
				Btn.Text = "Are you sure?";
			} else {
				Btn.Text = "New";
				WriteText("Dropping stranger", MessageSender.Info);
				HC.DisconnectFromStranger();
			}
		}
	}

	static class Utls {
		[Flags]
		enum EmfToWmfBitsFlags {
			EmfToWmfBitsFlagsDefault = 0x00000000,
			EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
			EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
			EmfToWmfBitsFlagsNoXORClip = 0x00000004
		}

		const int MM_ISOTROPIC = 7;
		const int MM_ANISOTROPIC = 8;

		[DllImport("gdiplus")]
		static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize, byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);
		[DllImport("gdi32")]
		static extern IntPtr SetMetaFileBitsEx(uint _bufferSize, byte[] _buffer);
		[DllImport("gdi32")]
		static extern IntPtr CopyMetaFile(IntPtr hWmf, string filename);
		[DllImport("gdi32")]
		static extern bool DeleteMetaFile(IntPtr hWmf);
		[DllImport("gdi32")]
		static extern bool DeleteEnhMetaFile(IntPtr hEmf);

		public static string ToRtf(this Bitmap Img, int H = 100) {
			Metafile metafile = null;
			float dpiX;
			float dpiY;

			using (Graphics g = Graphics.FromImage(Img)) {
				IntPtr hDC = g.GetHdc();
				metafile = new Metafile(hDC, EmfType.EmfOnly);
				g.ReleaseHdc(hDC);
			}

			using (Graphics g = Graphics.FromImage(metafile)) {
				g.DrawImage(Img, 0, 0);
				dpiX = g.DpiX;
				dpiY = g.DpiY;
			}

			IntPtr _hEmf = metafile.GetHenhmetafile();
			uint _bufferSize = GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC,
			EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
			byte[] _buffer = new byte[_bufferSize];
			GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC,
										EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
			IntPtr hmf = SetMetaFileBitsEx(_bufferSize, _buffer);
			string tempfile = Path.GetTempFileName();
			CopyMetaFile(hmf, tempfile);
			DeleteMetaFile(hmf);
			DeleteEnhMetaFile(_hEmf);

			var stream = new MemoryStream();
			byte[] data = File.ReadAllBytes(tempfile);
			//File.Delete (tempfile);
			int count = data.Length;
			stream.Write(data, 0, count);

			if (Img.Height < H)
				H = Img.Height;
			int W = (int)((float)Img.Width / Img.Height * H);

			string proto = @"{\rtf1{\pict\wmetafile8\picw" + (int)(((float)Img.Width / dpiX) * 2540)
							  + @"\pich" + (int)(((float)Img.Height / dpiY) * 2540)
							  + @"\picwgoal" + (int)(((float)W / dpiX) * 1440)
							  + @"\pichgoal" + (int)(((float)H / dpiY) * 1440)
							  + " "
				  + BitConverter.ToString(stream.ToArray()).Replace("-", "")
							  + "}}";
			return proto;
		}

		public static string ToRtf(this Image Img, int H = 256) {
			return new Bitmap(Img).ToRtf(H);
		}

	}
}