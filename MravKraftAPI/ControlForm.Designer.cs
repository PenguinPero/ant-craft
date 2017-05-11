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
            this.LblShowVision = new System.Windows.Forms.Label();
            this.CBoxPlayer2 = new System.Windows.Forms.CheckBox();
            this.CBoxPlayer1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // LblShowVision
            // 
            this.LblShowVision.AutoSize = true;
            this.LblShowVision.Location = new System.Drawing.Point(24, 22);
            this.LblShowVision.Name = "LblShowVision";
            this.LblShowVision.Size = new System.Drawing.Size(82, 13);
            this.LblShowVision.TabIndex = 14;
            this.LblShowVision.Text = "Show vision for:";
            // 
            // CBoxPlayer2
            // 
            this.CBoxPlayer2.AutoSize = true;
            this.CBoxPlayer2.Checked = true;
            this.CBoxPlayer2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBoxPlayer2.Location = new System.Drawing.Point(77, 70);
            this.CBoxPlayer2.Name = "CBoxPlayer2";
            this.CBoxPlayer2.Size = new System.Drawing.Size(64, 17);
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
            this.CBoxPlayer1.Location = new System.Drawing.Point(77, 47);
            this.CBoxPlayer1.Name = "CBoxPlayer1";
            this.CBoxPlayer1.Size = new System.Drawing.Size(64, 17);
            this.CBoxPlayer1.TabIndex = 12;
            this.CBoxPlayer1.Text = "Player 1";
            this.CBoxPlayer1.UseVisualStyleBackColor = true;
            this.CBoxPlayer1.CheckedChanged += new System.EventHandler(this.CBoxPlayer1_CheckedChanged);
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(211, 118);
            this.ControlBox = false;
            this.Controls.Add(this.LblShowVision);
            this.Controls.Add(this.CBoxPlayer2);
            this.Controls.Add(this.CBoxPlayer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
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
    }
}