using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.Cuphead {
	public partial class SplitterSettings : UserControl {
		public List<SplitInfo> Splits { get; private set; }
		public bool SplitAfterScoreboard { get; set; }
		public bool SplitAfterScoreboardSaltbaker { get; set; }
        private bool isLoading;
		public SplitterSettings() {
			isLoading = true;
			InitializeComponent();

			Splits = new List<SplitInfo>();
			isLoading = false;
		}

		private void Settings_Load(object sender, EventArgs e) {
			LoadSettings();
		}
		public void LoadSettings() {
			isLoading = true;
			this.flowMain.SuspendLayout();

			for (int i = flowMain.Controls.Count - 1; i > 0; i--) {
				flowMain.Controls.RemoveAt(i);
			}

			foreach (SplitInfo split in Splits) {
				SplitterSplitSettings setting = new SplitterSplitSettings();
				setting.cboName.DataSource = GetAvailableSplits();
				setting.cboName.Text = SplitterSplitSettings.GetEnumDescription<SplitName>(split.Split);
				setting.cboGrade.Text = SplitterSplitSettings.GetEnumDescription<Grade>(split.Grade);
				setting.cboDifficulty.Text = SplitterSplitSettings.GetEnumDescription<Mode>(split.Difficulty);
				AddHandlers(setting);

				flowMain.Controls.Add(setting);
			}

            chkScoreBoardTiming.Checked = SplitAfterScoreboard;
			chkScoreBoardTimingSaltbaker.Checked = SplitAfterScoreboardSaltbaker;
			chkScoreBoardTimingSaltbaker.Enabled = chkScoreBoardTiming.Checked;

            isLoading = false;
			this.flowMain.ResumeLayout(true);
		}
		private void AddHandlers(SplitterSplitSettings setting) {
			setting.cboName.SelectedIndexChanged += new EventHandler(ControlChanged);
			setting.cboGrade.SelectedIndexChanged += new EventHandler(ControlChanged);
			setting.cboDifficulty.SelectedIndexChanged += new EventHandler(ControlChanged);
			setting.btnRemove.Click += new EventHandler(btnRemove_Click);
		}
		private void RemoveHandlers(SplitterSplitSettings setting) {
			setting.cboName.SelectedIndexChanged -= ControlChanged;
			setting.cboGrade.SelectedIndexChanged -= ControlChanged;
			setting.cboDifficulty.SelectedIndexChanged -= ControlChanged;
			setting.btnRemove.Click -= btnRemove_Click;
		}
		public void btnRemove_Click(object sender, EventArgs e) {
			for (int i = flowMain.Controls.Count - 1; i > 0; i--) {
				if (flowMain.Controls[i].Contains((Control)sender)) {
					RemoveHandlers((SplitterSplitSettings)((Button)sender).Parent);

					flowMain.Controls.RemoveAt(i);
					break;
				}
			}
			UpdateSplits();
		}
		public void ControlChanged(object sender, EventArgs e) {
			UpdateSplits();
		}
		public void UpdateSplits() {
			if (isLoading) return;

			Splits.Clear();
			foreach (Control c in flowMain.Controls) {
				if (c is SplitterSplitSettings) {
					SplitterSplitSettings setting = (SplitterSplitSettings)c;
					if (!string.IsNullOrEmpty(setting.cboName.Text)) {
						Splits.Add(new SplitInfo() {
							Split = SplitterSplitSettings.GetEnumValue<SplitName>(setting.cboName.Text),
							Grade = SplitterSplitSettings.GetEnumValue<Grade>(setting.cboGrade.Text),
							Difficulty = SplitterSplitSettings.GetEnumValue<Mode>(setting.cboDifficulty.Text)
						});
					}
				}
			}

			SplitAfterScoreboard = chkScoreBoardTiming.Checked;
            SplitAfterScoreboardSaltbaker = chkScoreBoardTimingSaltbaker.Checked;

			if (SplitAfterScoreboard)
				chkScoreBoardTimingSaltbaker.Enabled = true;
			else
				chkScoreBoardTimingSaltbaker.Enabled = false;
        }
		public XmlNode UpdateSettings(XmlDocument document) {
			XmlElement xmlSettings = document.CreateElement("Settings");

			XmlElement xmlSplits = document.CreateElement("Splits");
			xmlSettings.AppendChild(xmlSplits);

			foreach (SplitInfo split in Splits) {
				XmlElement xmlSplit = document.CreateElement("Split");
				xmlSplit.InnerText = split.ToString();

				xmlSplits.AppendChild(xmlSplit);
			}

			XmlElement xmlSplitAfterScoreboard = document.CreateElement("SplitAfterScoreboard");
			xmlSplitAfterScoreboard.InnerText = SplitAfterScoreboard.ToString();
			xmlSettings.AppendChild(xmlSplitAfterScoreboard);

            XmlElement xmlSplitAfterScoreboardSaltbaker = document.CreateElement("SplitAfterScoreboardSaltbaker");
            xmlSplitAfterScoreboardSaltbaker.InnerText = SplitAfterScoreboardSaltbaker.ToString();
            xmlSettings.AppendChild(xmlSplitAfterScoreboardSaltbaker);

            return xmlSettings;
		}
		public void SetSettings(XmlNode settings) {
			Splits.Clear();
			XmlNodeList splitNodes = settings.SelectNodes(".//Splits/Split");
			foreach (XmlNode splitNode in splitNodes) {
				string splitDescription = splitNode.InnerText;
				Splits.Add(new SplitInfo(splitDescription));
			}


			XmlNode splitAfterScoreboard = settings.SelectSingleNode(".//SplitAfterScoreboard");
			if (splitAfterScoreboard != null)
			{
				bool result;
				if (bool.TryParse(splitAfterScoreboard.InnerText, out result))
				{
					SplitAfterScoreboard = result;
                }
			}

            XmlNode splitAfterScoreboardSaltbaker = settings.SelectSingleNode(".//SplitAfterScoreboardSaltbaker");
            if (splitAfterScoreboardSaltbaker != null)
            {
                bool result;
                if (bool.TryParse(splitAfterScoreboardSaltbaker.InnerText, out result))
                {
                    SplitAfterScoreboardSaltbaker = result;
                }
            }
        }
		private void btnAddSplit_Click(object sender, EventArgs e) {
			SplitterSplitSettings setting = new SplitterSplitSettings();
			List<string> splitNames = GetAvailableSplits();
			setting.cboName.DataSource = splitNames;
			setting.cboName.Text = splitNames[0];
			AddHandlers(setting);

			flowMain.Controls.Add(setting);
			UpdateSplits();
		}
		private List<string> GetAvailableSplits() {
			List<string> splits = new List<string>();
			foreach (SplitName split in Enum.GetValues(typeof(SplitName))) {
				MemberInfo info = typeof(SplitName).GetMember(split.ToString())[0];
				DescriptionAttribute description = (DescriptionAttribute)info.GetCustomAttributes(typeof(DescriptionAttribute), false)[0];
				splits.Add(description.Description);
			}
			if (rdAlpha.Checked) {
				splits.Sort(delegate (string one, string two) {
					return one.CompareTo(two);
				});
			}
			return splits;
		}
		private void radio_CheckedChanged(object sender, EventArgs e) {
			foreach (Control c in flowMain.Controls) {
				if (c is SplitterSplitSettings) {
					SplitterSplitSettings setting = (SplitterSplitSettings)c;
					string text = setting.cboName.Text;
					setting.cboName.DataSource = GetAvailableSplits();
					setting.cboName.Text = text;
				}
			}
		}
		private void flowMain_DragDrop(object sender, DragEventArgs e) {
			UpdateSplits();
		}
		private void flowMain_DragEnter(object sender, DragEventArgs e) {
			e.Effect = DragDropEffects.Move;
		}
		private void flowMain_DragOver(object sender, DragEventArgs e) {
			SplitterSplitSettings data = (SplitterSplitSettings)e.Data.GetData(typeof(SplitterSplitSettings));
			FlowLayoutPanel destination = (FlowLayoutPanel)sender;
			Point p = destination.PointToClient(new Point(e.X, e.Y));
			var item = destination.GetChildAtPoint(p);
			int index = destination.Controls.GetChildIndex(item, false);
			if (index == 0) {
				e.Effect = DragDropEffects.None;
			} else {
				e.Effect = DragDropEffects.Move;
				int oldIndex = destination.Controls.GetChildIndex(data);
				if (oldIndex != index) {
					destination.Controls.SetChildIndex(data, index);
					destination.Invalidate();
				}
			}
		}

        private void chkScoreBoardTiming_CheckedChanged(object sender, EventArgs e)
        {
			UpdateSplits();
        }

        private void chkScoreBoardTimingSaltbaker_CheckedChanged(object sender, EventArgs e)
        {
			UpdateSplits();
        }

    }
    public class SplitInfo {
		public static SplitInfo EndGame = new SplitInfo() { Split = SplitName.EndGame, Grade = Grade.Any, Difficulty = Mode.Any };
		public SplitName Split { get; set; }
		public Grade Grade { get; set; }
		public Mode Difficulty { get; set; }
		public SplitInfo() { }
		public SplitInfo(string copy) {
			string[] info = copy.Split(',');
			if (info.Length > 0) {
				SplitName temp;
				if (Enum.TryParse(info[0], out temp)) {
					Split = temp;
				}
			}
			Grade = Grade.Any;
			if (info.Length > 1) {
				Grade temp;
				if (Enum.TryParse(info[1], out temp)) {
					Grade = temp;
				}
			}
			Difficulty = Mode.Any;
			if (info.Length > 2) {
				Mode temp;
				if (Enum.TryParse(info[2], out temp)) {
					Difficulty = temp;
				}
			}
		}
		public override string ToString() {
			return Split.ToString() + "," + Grade.ToString() + "," + Difficulty.ToString();
		}
	}
}