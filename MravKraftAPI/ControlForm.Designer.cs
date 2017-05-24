namespace MravKraftAPI
{
    partial class ControlForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.LblShowVision = new System.Windows.Forms.Label();
            this.CBoxPlayer2 = new System.Windows.Forms.CheckBox();
            this.CBoxPlayer1 = new System.Windows.Forms.CheckBox();
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // LblShowVision
            // 
            this.LblShowVision.AutoSize = true;
            this.LblShowVision.Location = new System.Drawing.Point(56, 49);
            this.LblShowVision.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.LblShowVision.Name = "LblShowVision";
            this.LblShowVision.Size = new System.Drawing.Size(182, 29);
            this.LblShowVision.TabIndex = 14;
            this.LblShowVision.Text = "Show vision for:";
            // 
            // CBoxPlayer2
            // 
            this.CBoxPlayer2.AutoSize = true;
            this.CBoxPlayer2.Checked = true;
            this.CBoxPlayer2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBoxPlayer2.Location = new System.Drawing.Point(180, 156);
            this.CBoxPlayer2.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.CBoxPlayer2.Name = "CBoxPlayer2";
            this.CBoxPlayer2.Size = new System.Drawing.Size(132, 33);
            this.CBoxPlayer2.TabIndex = 13;
            this.CBoxPlayer2.Text = "Player 2";
            this.CBoxPlayer2.UseVisualStyleBackColor = true;
            this.CBoxPlayer2.CheckedChanged += new System.EventHandler(this.CBoxPlayer2_CheckedChanged);
            // 
            // CBoxPlayer1
            // 
            this.CBoxPlayer1.AutoSize = true;
            this.CBoxPlayer1.Checked = true;
            this.CBoxPlayer1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBoxPlayer1.Location = new System.Drawing.Point(180, 105);
            this.CBoxPlayer1.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.CBoxPlayer1.Name = "CBoxPlayer1";
            this.CBoxPlayer1.Size = new System.Drawing.Size(132, 33);
            this.CBoxPlayer1.TabIndex = 12;
            this.CBoxPlayer1.Text = "Player 1";
            this.CBoxPlayer1.UseVisualStyleBackColor = true;
            this.CBoxPlayer1.CheckedChanged += new System.EventHandler(this.CBoxPlayer1_CheckedChanged);
            // 
            // UpdateTimer
            // 
            this.UpdateTimer.Enabled = true;
            this.UpdateTimer.Interval = 200;
            this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 482);
            this.ControlBox = false;
            this.Controls.Add(this.LblShowVision);
            this.Controls.Add(this.CBoxPlayer2);
            this.Controls.Add(this.CBoxPlayer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.MaximizeBox = false;
            this.Name = "ControlForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Game options";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.ControlForm_Activated);
            this.Deactivate += new System.EventHandler(this.ControlForm_Deactivate);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label LblShowVision;
        private System.Windows.Forms.CheckBox CBoxPlayer2;
        private System.Windows.Forms.CheckBox CBoxPlayer1;
        private System.Windows.Forms.Timer UpdateTimer;
    }
}