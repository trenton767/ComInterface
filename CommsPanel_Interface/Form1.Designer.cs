namespace CommsPanel_Interface
{
    partial class Form1
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
            this.debugBox_tb = new System.Windows.Forms.TextBox();
            this.VATSIM_ScapeTMR = new System.Windows.Forms.Timer(this.components);
            this.CheckForConnectionsTMR = new System.Windows.Forms.Timer(this.components);
            this.ListenTMR = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // debugBox_tb
            // 
            this.debugBox_tb.Location = new System.Drawing.Point(24, 23);
            this.debugBox_tb.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.debugBox_tb.Multiline = true;
            this.debugBox_tb.Name = "debugBox_tb";
            this.debugBox_tb.Size = new System.Drawing.Size(1548, 816);
            this.debugBox_tb.TabIndex = 0;
            // 
            // VATSIM_ScapeTMR
            // 
            this.VATSIM_ScapeTMR.Interval = 10000;
            this.VATSIM_ScapeTMR.Tick += new System.EventHandler(this.VATSIM_ScapeTMR_Tick);
            // 
            // CheckForConnectionsTMR
            // 
            this.CheckForConnectionsTMR.Interval = 30000;
            // 
            // ListenTMR
            // 
            this.ListenTMR.Interval = 500;
            this.ListenTMR.Tick += new System.EventHandler(this.ListenTMR_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 865);
            this.Controls.Add(this.debugBox_tb);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox debugBox_tb;
        private System.Windows.Forms.Timer VATSIM_ScapeTMR;
        private System.Windows.Forms.Timer CheckForConnectionsTMR;
        private System.Windows.Forms.Timer ListenTMR;
    }
}

