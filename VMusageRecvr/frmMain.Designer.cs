namespace VMusageRecvr
{
    partial class frmMain
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
            this.txtLog = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.c2DPushGraph1 = new CustomUIControls.Graphing.C2DPushGraph();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUsage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 217);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(627, 130);
            this.txtLog.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 30);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(627, 181);
            this.dataGridView1.TabIndex = 1;
            // 
            // c2DPushGraph1
            // 
            this.c2DPushGraph1.AutoAdjustPeek = false;
            this.c2DPushGraph1.BackColor = System.Drawing.Color.Black;
            this.c2DPushGraph1.GridColor = System.Drawing.Color.Green;
            this.c2DPushGraph1.GridSize = ((ushort)(15));
            this.c2DPushGraph1.HighQuality = true;
            this.c2DPushGraph1.LineInterval = ((ushort)(5));
            this.c2DPushGraph1.Location = new System.Drawing.Point(57, 360);
            this.c2DPushGraph1.MaxLabel = "Max";
            this.c2DPushGraph1.MaxPeekMagnitude = 100;
            this.c2DPushGraph1.MinLabel = "Minimum";
            this.c2DPushGraph1.MinPeekMagnitude = 0;
            this.c2DPushGraph1.Name = "c2DPushGraph1";
            this.c2DPushGraph1.ShowGrid = true;
            this.c2DPushGraph1.ShowLabels = true;
            this.c2DPushGraph1.Size = new System.Drawing.Size(501, 79);
            this.c2DPushGraph1.TabIndex = 2;
            this.c2DPushGraph1.Text = "c2DPushGraph1";
            this.c2DPushGraph1.TextColor = System.Drawing.Color.Yellow;
            // 
            // menuExit
            // 
            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(142, 22);
            this.menuExit.Text = "Exit";
            // 
            // mnuUsage
            // 
            this.mnuUsage.Name = "mnuUsage";
            this.mnuUsage.Size = new System.Drawing.Size(142, 22);
            this.mnuUsage.Text = "Export Usage";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(651, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 20);
            this.mnuFile.Text = "File";
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(152, 22);
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 462);
            this.Controls.Add(this.c2DPushGraph1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.DataGridView dataGridView1;
        private CustomUIControls.Graphing.C2DPushGraph c2DPushGraph1;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.ToolStripMenuItem mnuUsage;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
    }
}

