using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
namespace LiveSplit.Cuphead {
	public partial class SplitterSplitSettings : UserControl {
		public string Split { get; set; } = "";
		private int mX = 0;
		private int mY = 0;
		private bool isDragging = false;
		public SplitterSplitSettings() {
			InitializeComponent();
		}
		private void cboName_SelectedIndexChanged(object sender, EventArgs e) {
			string splitDescription = cboName.SelectedValue.ToString();
			SplitName split = GetSplitName(splitDescription);
			Split = split.ToString();

			MemberInfo info = typeof(SplitName).GetMember(split.ToString())[0];
			DescriptionAttribute description = (DescriptionAttribute)info.GetCustomAttributes(typeof(DescriptionAttribute), false)[0];
			ToolTipAttribute tooltip = (ToolTipAttribute)info.GetCustomAttributes(typeof(ToolTipAttribute), false)[0];
			ToolTips.SetToolTip(cboName, tooltip.ToolTip);
		}
		public static SplitName GetSplitName(string text) {
			foreach (SplitName split in Enum.GetValues(typeof(SplitName))) {
				string name = split.ToString();
				MemberInfo info = typeof(SplitName).GetMember(name)[0];
				DescriptionAttribute description = (DescriptionAttribute)info.GetCustomAttributes(typeof(DescriptionAttribute), false)[0];

				if (name.Equals(text, StringComparison.OrdinalIgnoreCase) || description.Description.Equals(text, StringComparison.OrdinalIgnoreCase)) {
					return split;
				}
			}
			return SplitName.ManualSplit;
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