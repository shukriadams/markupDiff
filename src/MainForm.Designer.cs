namespace MarkupDiff
{
    partial class MainForm
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
            this.rtbSource = new System.Windows.Forms.RichTextBox();
            this.rtbDestination = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblSourceFilePath = new System.Windows.Forms.Label();
            this.lblDestinationFilePath = new System.Windows.Forms.Label();
            this.projectContainer = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnViewProjects = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.lvFiles = new MarkupDiff.ListViewFast();
            this.pnlProjects = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.lvProjects = new System.Windows.Forms.ListView();
            this.pnlNoProjects = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cbShowCode = new System.Windows.Forms.CheckBox();
            this.cbLiveUpdateFileContent = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.projectContainer)).BeginInit();
            this.projectContainer.Panel1.SuspendLayout();
            this.projectContainer.Panel2.SuspendLayout();
            this.projectContainer.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlProjects.SuspendLayout();
            this.pnlNoProjects.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbSource
            // 
            this.rtbSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbSource.Location = new System.Drawing.Point(3, 37);
            this.rtbSource.Name = "rtbSource";
            this.rtbSource.Size = new System.Drawing.Size(212, 180);
            this.rtbSource.TabIndex = 0;
            this.rtbSource.Text = "";
            this.rtbSource.MouseUp += new System.Windows.Forms.MouseEventHandler(this.rtbSource_MouseUp);
            // 
            // rtbDestination
            // 
            this.rtbDestination.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbDestination.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbDestination.Location = new System.Drawing.Point(3, 34);
            this.rtbDestination.Name = "rtbDestination";
            this.rtbDestination.Size = new System.Drawing.Size(194, 186);
            this.rtbDestination.TabIndex = 1;
            this.rtbDestination.Text = "";
            this.rtbDestination.MouseUp += new System.Windows.Forms.MouseEventHandler(this.rtbDestination_MouseUp);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblSourceFilePath);
            this.splitContainer1.Panel1.Controls.Add(this.rtbSource);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lblDestinationFilePath);
            this.splitContainer1.Panel2.Controls.Add(this.rtbDestination);
            this.splitContainer1.Size = new System.Drawing.Size(419, 220);
            this.splitContainer1.SplitterDistance = 215;
            this.splitContainer1.TabIndex = 3;
            // 
            // lblSourceFilePath
            // 
            this.lblSourceFilePath.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSourceFilePath.Location = new System.Drawing.Point(0, 0);
            this.lblSourceFilePath.Name = "lblSourceFilePath";
            this.lblSourceFilePath.Size = new System.Drawing.Size(215, 34);
            this.lblSourceFilePath.TabIndex = 1;
            this.lblSourceFilePath.Text = "[source file]";
            // 
            // lblDestinationFilePath
            // 
            this.lblDestinationFilePath.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDestinationFilePath.Location = new System.Drawing.Point(0, 0);
            this.lblDestinationFilePath.Name = "lblDestinationFilePath";
            this.lblDestinationFilePath.Size = new System.Drawing.Size(200, 34);
            this.lblDestinationFilePath.TabIndex = 2;
            this.lblDestinationFilePath.Text = "[destination file ]";
            // 
            // projectContainer
            // 
            this.projectContainer.Location = new System.Drawing.Point(27, 24);
            this.projectContainer.Name = "projectContainer";
            // 
            // projectContainer.Panel1
            // 
            this.projectContainer.Panel1.Controls.Add(this.panel1);
            this.projectContainer.Panel1.Controls.Add(this.lvFiles);
            // 
            // projectContainer.Panel2
            // 
            this.projectContainer.Panel2.Controls.Add(this.splitContainer1);
            this.projectContainer.Size = new System.Drawing.Size(555, 220);
            this.projectContainer.SplitterDistance = 132;
            this.projectContainer.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbLiveUpdateFileContent);
            this.panel1.Controls.Add(this.cbShowCode);
            this.panel1.Controls.Add(this.btnViewProjects);
            this.panel1.Controls.Add(this.btnReload);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(132, 76);
            this.panel1.TabIndex = 6;
            // 
            // btnViewProjects
            // 
            this.btnViewProjects.Location = new System.Drawing.Point(58, 5);
            this.btnViewProjects.Name = "btnViewProjects";
            this.btnViewProjects.Size = new System.Drawing.Size(59, 23);
            this.btnViewProjects.TabIndex = 1;
            this.btnViewProjects.Text = "Projects";
            this.btnViewProjects.UseVisualStyleBackColor = true;
            this.btnViewProjects.Click += new System.EventHandler(this.btnViewProjects_Click);
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(3, 5);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(49, 23);
            this.btnReload.TabIndex = 0;
            this.btnReload.Text = "Reload";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // lvFiles
            // 
            this.lvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvFiles.GridLines = true;
            this.lvFiles.Location = new System.Drawing.Point(3, 95);
            this.lvFiles.MultiSelect = false;
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(129, 122);
            this.lvFiles.TabIndex = 5;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.List;
            // 
            // pnlProjects
            // 
            this.pnlProjects.Controls.Add(this.label1);
            this.pnlProjects.Controls.Add(this.lvProjects);
            this.pnlProjects.Location = new System.Drawing.Point(132, 265);
            this.pnlProjects.Name = "pnlProjects";
            this.pnlProjects.Size = new System.Drawing.Size(216, 116);
            this.pnlProjects.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select a project to load";
            // 
            // lvProjects
            // 
            this.lvProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvProjects.GridLines = true;
            this.lvProjects.Location = new System.Drawing.Point(3, 27);
            this.lvProjects.MultiSelect = false;
            this.lvProjects.Name = "lvProjects";
            this.lvProjects.Size = new System.Drawing.Size(210, 86);
            this.lvProjects.TabIndex = 0;
            this.lvProjects.UseCompatibleStateImageBehavior = false;
            this.lvProjects.View = System.Windows.Forms.View.List;
            // 
            // pnlNoProjects
            // 
            this.pnlNoProjects.Controls.Add(this.label2);
            this.pnlNoProjects.Location = new System.Drawing.Point(411, 292);
            this.pnlNoProjects.Name = "pnlNoProjects";
            this.pnlNoProjects.Size = new System.Drawing.Size(197, 122);
            this.pnlNoProjects.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(3, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(191, 99);
            this.label2.TabIndex = 0;
            this.label2.Text = "No project files found. Add one to the /projects folder.";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbShowCode
            // 
            this.cbShowCode.AutoSize = true;
            this.cbShowCode.Location = new System.Drawing.Point(3, 33);
            this.cbShowCode.Name = "cbShowCode";
            this.cbShowCode.Size = new System.Drawing.Size(80, 17);
            this.cbShowCode.TabIndex = 2;
            this.cbShowCode.Text = "Show code";
            this.cbShowCode.UseVisualStyleBackColor = true;
            this.cbShowCode.CheckedChanged += new System.EventHandler(this.cbShowCode_CheckedChanged);
            // 
            // cbLiveUpdateFileContent
            // 
            this.cbLiveUpdateFileContent.AutoSize = true;
            this.cbLiveUpdateFileContent.Location = new System.Drawing.Point(3, 56);
            this.cbLiveUpdateFileContent.Name = "cbLiveUpdateFileContent";
            this.cbLiveUpdateFileContent.Size = new System.Drawing.Size(128, 17);
            this.cbLiveUpdateFileContent.TabIndex = 3;
            this.cbLiveUpdateFileContent.Text = "Auto update changes";
            this.cbLiveUpdateFileContent.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(681, 488);
            this.Controls.Add(this.pnlNoProjects);
            this.Controls.Add(this.pnlProjects);
            this.Controls.Add(this.projectContainer);
            this.Name = "MainForm";
            this.Text = "Comparer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.projectContainer.Panel1.ResumeLayout(false);
            this.projectContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.projectContainer)).EndInit();
            this.projectContainer.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlProjects.ResumeLayout(false);
            this.pnlProjects.PerformLayout();
            this.pnlNoProjects.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbSource;
        private System.Windows.Forms.RichTextBox rtbDestination;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer projectContainer;
        private ListViewFast lvFiles;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Label lblSourceFilePath;
        private System.Windows.Forms.Label lblDestinationFilePath;
        private System.Windows.Forms.Panel pnlProjects;
        private System.Windows.Forms.ListView lvProjects;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlNoProjects;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnViewProjects;
        private System.Windows.Forms.CheckBox cbShowCode;
        private System.Windows.Forms.CheckBox cbLiveUpdateFileContent;
    }
}

