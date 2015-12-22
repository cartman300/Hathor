using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace Hathor {
	class Program {
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.Run(new Client());

			/*Console.Title = "Hathor client";

			WriteMsg("/c - Request stranger\n/d - Disconnect from stranger");

			HathorClient HC = new HathorClient();
			HC.StrangerConnected += () => WriteMsg("Stranger connected");
			HC.StrangerDisconnected += () => WriteMsg("Stranger disconnected");
			HC.MessageReceived += (Msg) => WriteLine(ConsoleColor.Green, "Stranger", Msg);
			HC.Connect();
			
			new Thread(HC.Run).Start();
			string In = "";
			while (true) {
				while ((In = Read()).Length > 0) {
					if (In.StartsWith("/")) {
						string Cmd = In.Substring(1);
						switch (Cmd) {
							case "c":
								HC.RequestStranger();
								break;
							case "d":
								HC.DisconnectFromStranger();
								break;
							default:
								WriteMsg("Unknown command '" + Cmd + "'");
								break;
						}
					} else {
						WriteLine(ConsoleColor.Red, "You", In);
						HC.SendMessage(In);
					}
				}
			}//*/
		}
	}
}
