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
            this.debugBox_tb.Location = new System.Drawing.Point(12, 12);
            this.debugBox_tb.Multiline = true;
            this.debugBox_tb.Name = "debugBox_tb";
            this.debugBox_tb.Size = new System.Drawing.Size(776, 426);
            this.debugBox_tb.TabIndex = 0;
            // 
            // VATSIM_ScapeTMR
            // 
            this.VATSIM_ScapeTMR.Interval = 10000;
            this.VATSIM_ScapeTMR.Tick += new System.EventHandler(this.VATSIM_ScapeTMR_Tick);
            // 
            // CheckForConnectionsTMR
            // 
            this.CheckForConnectionsTMR.Interval = 10000;
            this.CheckForConnectionsTMR.Tick += new System.EventHandler(this.CheckForConnectionsTMR_Tick);
            // 
            // ListenTMR
            // 
            this.ListenTMR.Interval = 500;
            this.ListenTMR.Tick += new System.EventHandler(this.ListenTMR_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.debugBox_tb);
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

