using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
namespace LiveSplit.Cuphead {
	public partial class CupheadInfo : Form {
		public SplitterMemory Memory { get; set; }
		private byte[] debugCode;
		public static void Main(string[] args) {
			try {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new CupheadInfo());
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}
		public CupheadInfo() {
			this.DoubleBuffered = true;
			InitializeComponent();
			Text = "Cuphead Info " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
			Memory = new SplitterMemory();
			Thread t = new Thread(UpdateLoop);
			t.IsBackground = true;
			t.Start();
		}

		private void UpdateLoop() {
			bool lastHooked = false;
			while (true) {
				try {
					bool hooked = Memory.HookProcess();
					if (hooked) {
						UpdateValues();
					}
					if (lastHooked != hooked) {
						lastHooked = hooked;
						this.Invoke((Action)delegate () {
							lblNote.Visible = !hooked;
							debugCode = null;
							btnEnableDebug.Text = "Enable Debug";
						});
					}
					Thread.Sleep(12);
				} catch { }
			}
		}
		public void UpdateValues() {
			if (this.InvokeRequired) {
				this.Invoke((Action)UpdateValues);
			} else {
				lblScene.Text = "Scene: " + Memory.SceneName() + (Memory.InGame() ? " (In Game)" : "");
				lblInGame.Text = "Game: " + Memory.GameCompletion().ToString("0.0") + "%";
				lblLevel.Text = "Level: " + Memory.LevelMode().ToString() + " - " + Memory.LevelTime().ToString("0.00") + (Memory.Loading() ? " (Loading)" : "") + (Memory.LevelWon() ? " (Won)" : "") + (Memory.LevelEnding() ? " (Ending)" : "");
				lblDeaths.Text = "Coins: " + Memory.Coins() + " Deaths: " + Memory.Deaths() + " Super: " + Memory.SuperMeter().ToString("0.00");
				lblDetail.Text = Memory.CurrentEnemies();
			}
		}
		private void btnEnableDebug_Click(object sender, EventArgs e) {
			if (btnEnableDebug.Text == "Enable Debug") {
				btnEnableDebug.Text = "Disable Debug";
				debugCode = Memory.ReadDebugCode();
				if (debugCode[0] != 0) {
					Memory.EnableDebugConsole();
				} else {
					btnEnableDebug.Text = "Enable Debug";
					MessageBox.Show("Could not find Debug Console code. Not available in 1.2");
				}
			} else if (debugCode != null) {
				btnEnableDebug.Text = "Enable Debug";
				Memory.DisableDebugConsole(debugCode);
			}
		}
	}
}