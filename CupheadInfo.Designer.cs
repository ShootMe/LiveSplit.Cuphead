namespace LiveSplit.Cuphead {
	partial class CupheadInfo {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CupheadInfo));
			this.lblNote = new System.Windows.Forms.Label();
			this.lblInGame = new System.Windows.Forms.Label();
			this.lblScene = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblNote
			// 
			this.lblNote.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblNote.Font = new System.Drawing.Font("Courier New", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblNote.Location = new System.Drawing.Point(0, 0);
			this.lblNote.Name = "lblNote";
			this.lblNote.Size = new System.Drawing.Size(411, 115);
			this.lblNote.TabIndex = 0;
			this.lblNote.Text = "Not available";
			this.lblNote.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblInGame
			// 
			this.lblInGame.AutoSize = true;
			this.lblInGame.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblInGame.Location = new System.Drawing.Point(6, 25);
			this.lblInGame.Name = "lblInGame";
			this.lblInGame.Size = new System.Drawing.Size(72, 16);
			this.lblInGame.TabIndex = 1;
			this.lblInGame.Text = "In Game:";
			// 
			// lblScene
			// 
			this.lblScene.AutoSize = true;
			this.lblScene.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblScene.Location = new System.Drawing.Point(22, 9);
			this.lblScene.Name = "lblScene";
			this.lblScene.Size = new System.Drawing.Size(56, 16);
			this.lblScene.TabIndex = 2;
			this.lblScene.Text = "Scene:";
			// 
			// CupheadInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(411, 115);
			this.Controls.Add(this.lblScene);
			this.Controls.Add(this.lblInGame);
			this.Controls.Add(this.lblNote);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "CupheadInfo";
			this.Text = "Cuphead Info";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label lblNote;
		private System.Windows.Forms.Label lblInGame;
		private System.Windows.Forms.Label lblScene;
	}
}