using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
namespace LiveSplit.Cuphead {
	public partial class SplitterSplitSettings : UserControl {
		public string Split { get; set; } = "";
		public string Grade { get; set; } = "Any";
		public string Difficulty { get; set; } = "Any";
		private int mX = 0;
		private int mY = 0;
		private bool isDragging = false;
		public SplitterSplitSettings() {
			InitializeComponent();
			cboGrade.SelectedItem = Grade;
			cboDifficulty.SelectedItem = Difficulty;
		}
		private void cboName_Validating(object sender, CancelEventArgs e) {
			string item = GetItemInList(cboGrade);
			if (string.IsNullOrEmpty(item)) {
				cboName.SelectedItem = SplitName.ManualSplit;
			} else {
				cboName.SelectedItem = GetEnumValue<SplitName>(item);
			}
		}
		private void cboName_SelectedIndexChanged(object sender, EventArgs e) {
			string splitDescription = cboName.SelectedValue.ToString();
			SplitName split = GetEnumValue<SplitName>(splitDescription);
			Split = split.ToString();
			if (splitDescription.IndexOf("(Boss)") < 0 && splitDescription.IndexOf("(Run 'n Gun)") < 0) {
				cboGrade.SelectedItem = "Any";
				cboDifficulty.SelectedItem = "Any";
				cboGrade.Visible = false;
				cboDifficulty.Visible = false;
				btnRemove.Location = new System.Drawing.Point(274, 2);
			} else if (splitDescription.IndexOf("(Run 'n Gun)") > 0) {
				cboDifficulty.SelectedItem = "Any";
				btnRemove.Location = new System.Drawing.Point(331, 2);
				cboGrade.Visible = true;
				cboDifficulty.Visible = false;
			} else {
				btnRemove.Location = new System.Drawing.Point(399, 2);
				cboGrade.Visible = true;
				cboDifficulty.Visible = true;
			}

			MemberInfo info = typeof(SplitName).GetMember(split.ToString())[0];
			DescriptionAttribute description = (DescriptionAttribute)info.GetCustomAttributes(typeof(DescriptionAttribute), false)[0];
			ToolTipAttribute tooltip = (ToolTipAttribute)info.GetCustomAttributes(typeof(ToolTipAttribute), false)[0];
			ToolTips.SetToolTip(cboName, tooltip.ToolTip);
		}
		public static T GetEnumValue<T>(string text) {
			foreach (T item in Enum.GetValues(typeof(T))) {
				string name = item.ToString();
				MemberInfo info = typeof(T).GetMember(name)[0];
				object[] attributes = info.GetCustomAttributes(typeof(DescriptionAttribute), false);
				DescriptionAttribute description = attributes != null && attributes.Length > 0 ? (DescriptionAttribute)attributes[0] : null;

				if (name.Equals(text, StringComparison.OrdinalIgnoreCase) || (description != null && description.Description.Equals(text, StringComparison.OrdinalIgnoreCase))) {
					return item;
				}
			}
			return default(T);
		}
		public static string GetEnumDescription<T>(T item) {
			string name = item.ToString();
			MemberInfo info = typeof(T).GetMember(name)[0];
			object[] attributes = info.GetCustomAttributes(typeof(DescriptionAttribute), false);
			DescriptionAttribute description = attributes != null && attributes.Length > 0 ? (DescriptionAttribute)attributes[0] : null;

			return description == null ? name : description.Description;
		}
		private void cboGrade_Validating(object sender, CancelEventArgs e) {
			string item = GetItemInList(cboGrade);
			if (string.IsNullOrEmpty(item)) {
				item = "Any";
			}
			cboGrade.SelectedItem = item;
		}
		private void cboGrade_SelectedIndexChanged(object sender, EventArgs e) {
			if (cboGrade.SelectedItem != null) {
				Grade = cboGrade.SelectedItem.ToString();
			}
		}
		private void cboDifficulty_Validating(object sender, CancelEventArgs e) {
			string item = GetItemInList(cboDifficulty);
			if (string.IsNullOrEmpty(item)) {
				item = "Any";
			}
			cboDifficulty.SelectedItem = item;
		}
		private void cboDifficulty_SelectedIndexChanged(object sender, EventArgs e) {
			if (cboDifficulty.SelectedItem != null) {
				Difficulty = cboDifficulty.SelectedItem.ToString();
			}
		}
		private string GetItemInList(ComboBox cbo) {
			string item = cbo.Text;
			for (int i = cbo.Items.Count - 1; i >= 0; i += -1) {
				object ob = cbo.Items[i];
				if (ob.ToString().Equals(item, StringComparison.OrdinalIgnoreCase)) {
					return ob.ToString();
				}
			}
			return null;
		}
		private void picHandle_MouseMove(object sender, MouseEventArgs e) {
			if (!isDragging) {
				if (e.Button == MouseButtons.Left) {
					int num1 = mX - e.X;
					int num2 = mY - e.Y;
					if (((num1 * num1) + (num2 * num2)) > 20) {
						DoDragDrop(this, DragDropEffects.All);
						isDragging = true;
						return;
					}
				}
			}
		}
		private void picHandle_MouseDown(object sender, MouseEventArgs e) {
			mX = e.X;
			mY = e.Y;
			isDragging = false;
		}
	}
	public class ToolTipAttribute : Attribute {
		public string ToolTip { get; set; }
		public ToolTipAttribute(string text) {
			ToolTip = text;
		}
	}
}