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
using System.IO;
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
			Input.ShortcutsEnabled = true;

			FontFamily FntFam = SystemFonts.MessageBoxFont.FontFamily;
			Regular = new Font(FntFam, 12, FontStyle.Regular);
			Bold = new Font(FntFam, 12, FontStyle.Bold);

			Btn.Text = "New";
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
			return DateTime.Now.ToString("HH:mm");
		}

		void WriteText(string In, MessageSender MS = MessageSender.Server, bool Raw = false) {
			Output.DeselectAll();
			Output.Select(Output.TextLength, 0);
			if (Raw) {
				Output.SelectedRtf = In;
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
				Output.Clear();
				Btn.Enabled = true;
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
				HC.RequestStranger();
				Btn.Text = "Disconnect";
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
		static RichTextBox Rtb;

		static Utls() {
			Rtb = new RichTextBox();
		}

		public static string ToRtf(this Image Img) {
			IDataObject OldData = Clipboard.GetDataObject();
			Clipboard.SetImage(Img);
			Rtb.Clear();
			Rtb.Paste();
			Clipboard.SetDataObject(OldData);
			return Rtb.Rtf;
		}
	}
}