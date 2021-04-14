using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using DiscordRPC;

namespace CodeEditorPresences {
	class Program {
		static string Usage = "Nothing";
		static string[] IDENames = {
			"- VIM",
			"- Notepad++",
			"Visual Studio Code"
		};
		static Process IDE = null;

		public static string GetNameOfIDE(Process IDE) {
			if (IDE.MainWindowTitle.Contains("- VIM")) return "VIM";
			if (IDE.MainWindowTitle.Contains("- Notepad++")) return "Notepad++";
			if (IDE.MainWindowTitle.Contains("Visual Studio Code")) return "Visual Studio Code";

			return "UNKNOW_IDE";
		}

		public static string GetNameOfFile(Process IDE) {
			switch (GetNameOfIDE(IDE)) {
			case "VIM":
				return GetUntilOrEmpty(IDE.MainWindowTitle, IDENames[0]);
			case "Notepad++":
				return GetUntilOrEmpty(IDE.MainWindowTitle, IDENames[1]);
			case "UNKNOW_IDE":
				return "UNKNOW_FILE";
			default:
				return GetUntilOrEmpty(IDE.MainWindowTitle, IDENames[2]);
			}
		}
		public static string GetUntilOrEmpty(string text, string stopAt = "-") {
			if (!String.IsNullOrWhiteSpace(text)) {
				int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

				if (charLocation > 0) {
					return text.Substring(0, charLocation);
				}
			}

			return String.Empty;
		}
		static bool inicialized = false;
		static DiscordRpcClient client = new DiscordRpcClient("YOUR DISCORD APP ID HERE");

		public static void CheckProcess() {

			client.OnReady += (sender, msg) = >{
				Console.WriteLine("Connected to discord with user {0}", msg.User.Username);
			};
			client.OnPresenceUpdate += (sender, msg) = >{
				Console.WriteLine("Presence has been updated!");
			};

			ProcessManagement processManagement = new ProcessManagement();
			foreach(Process process in processManagement.processes) {
				if (IDENames.Where(x = >process.MainWindowTitle.Contains(x)).Count() > 0) {
					Console.WriteLine($ "You are using {IDENames.Where(x => process.MainWindowTitle.Contains(x)).First()} with file");

					if (IDE == null || (process.MainWindowTitle != IDE.MainWindowTitle) || process.HasExited || Usage == "Nothing"); {

						if (!inicialized) {
							inicialized = client.Initialize();
						}
						client.SetPresence(new RichPresence() {
							Details = GetNameOfIDE(process),
							State = GetNameOfFile(process),
							Timestamps = Timestamps.FromTimeSpan(10),
							Buttons = new Button[] {
								new Button() {
									Label = "Hecho por Lau",
									Url = "https://github.com/hangardonelli"
								}
							}
						});

					}
					IDE = process;
					break;
				}
			}
		}

		public static void Listen() {
			while (true) {
				if (IDE == null || Process.GetProcesses().Where(x = >x.MainWindowTitle.Contains(IDE.MainWindowTitle)).Count() < 1) break;
				CheckProcess();
				Thread.Sleep(5000);
			}
		}

		static void Main(string[] args) {
			while (true) {
				CheckProcess();
				Listen();
				Thread.Sleep(5000);
				Console.WriteLine("No IDE was detected, listening...");
				Usage = "Nothing";
				client.SetPresence(new RichPresence() {
					Details = Usage,
					State = "Nothing",
					Timestamps = Timestamps.FromTimeSpan(10),
					Buttons = new Button[] {
						new Button() {
							Label = "Hecho por Lau",
							Url = "https://github.com/hangardonelli"
						}
					}
				});
			}
		}

	}
	class ProcessManagement {

		public Process[] processes;
		public ProcessManagement() {
			processes = Process.GetProcesses();
		}
	}
}
