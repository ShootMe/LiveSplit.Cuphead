using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
namespace LiveSplit.Cuphead {
	public partial class CupheadInfo : Form {
		public SplitterMemory Memory { get; set; }
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
			Text = "Cuphead Info " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
						this.Invoke((Action)delegate () { lblNote.Visible = !hooked; });
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
				lblLevel.Text = "Level: " + Memory.LevelTime().ToString("0.00") + (Memory.Loading() ? " (Loading)" : "") + (Memory.LevelWon() ? " (Won)" : "") + (Memory.LevelEnding() ? " (Ending)" : "");
				lblDeaths.Text = "Coins: " + Memory.Coins() + " Deaths: " + Memory.Deaths();
				lblDetail.Text = Memory.CurrentEnemies();
			}
		}
	}
}