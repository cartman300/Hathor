namespace Hathor {
	partial class Client {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Client));
			this.SplitContainer = new System.Windows.Forms.SplitContainer();
			this.Output = new System.Windows.Forms.RichTextBox();
			this.Input = new System.Windows.Forms.TextBox();
			this.Btn = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.Status = new System.Windows.Forms.ToolStripStatusLabel();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// SplitContainer
			// 
			this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer.Location = new System.Drawing.Point(0, 0);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.Output);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.Input);
			this.SplitContainer.Panel2.Controls.Add(this.Btn);
			this.SplitContainer.Size = new System.Drawing.Size(835, 463);
			this.SplitContainer.SplitterDistance = 382;
			this.SplitContainer.TabIndex = 0;
			// 
			// Output
			// 
			this.Output.BackColor = System.Drawing.Color.White;
			this.Output.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Output.Location = new System.Drawing.Point(0, 0);
			this.Output.Name = "Output";
			this.Output.ReadOnly = true;
			this.Output.Size = new System.Drawing.Size(835, 382);
			this.Output.TabIndex = 0;
			this.Output.Text = "";
			// 
			// Input
			// 
			this.Input.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Input.Location = new System.Drawing.Point(113, 1);
			this.Input.Multiline = true;
			this.Input.Name = "Input";
			this.Input.Size = new System.Drawing.Size(719, 75);
			this.Input.TabIndex = 1;
			// 
			// Btn
			// 
			this.Btn.Dock = System.Windows.Forms.DockStyle.Left;
			this.Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.Btn.Location = new System.Drawing.Point(0, 0);
			this.Btn.Name = "Btn";
			this.Btn.Size = new System.Drawing.Size(114, 77);
			this.Btn.TabIndex = 0;
			this.Btn.Text = "BUTTON!!!";
			this.Btn.UseVisualStyleBackColor = true;
			this.Btn.Click += new System.EventHandler(this.Btn_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Status});
			this.statusStrip1.Location = new System.Drawing.Point(0, 466);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(835, 22);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// Status
			// 
			this.Status.Name = "Status";
			this.Status.Size = new System.Drawing.Size(104, 17);
			this.Status.Text = "Not implemented.";
			// 
			// Client
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(835, 488);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.SplitContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Client";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Hathor";
			this.Load += new System.EventHandler(this.Client_Load);
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer SplitContainer;
		private System.Windows.Forms.RichTextBox Output;
		private System.Windows.Forms.Button Btn;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel Status;
		private System.Windows.Forms.TextBox Input;
	}
}