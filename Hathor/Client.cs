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
			if (e.Data.GetDataPresent(DataFormats.Text)) {
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void Input_DragDrop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.Text))
				Input.Text += e.Data.GetData(DataFormats.Text).ToString();
		}

		private void Input_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)Keys.Enter) {
				e.Handled = true;
				string Txt = Input.Text.Trim();
				if (Txt.Length > 0) {
					Input.Text = "";
					OnInput(Txt);
				}
			}
		}

		void OnInput(string In) {
			if (HC.IsStrangerConnected)
				Btn.Text = "Disconnect";
			WriteText(In, MessageSender.You);
			if (HC.SendMessage(In))
				Status.Text = "Message sent";
			else
				Status.Text = "Message failed to send";
		}

		void OnMessage(string Msg) {
			Activate();
			WriteText(Msg, MessageSender.Stranger);
		}

		string GetCurTime() {
			return DateTime.Now.ToString("HH:mm");
		}

		void WriteText(string In, MessageSender MS = MessageSender.Server) {
			Output.DeselectAll();
			Output.Select(Output.TextLength, 0);
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
			HC.MessageReceived += (Msg) => OnMessage(Msg);
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
}