namespace Cockpit
{
    partial class CockPit
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.threadStackTrace = new System.Windows.Forms.RichTextBox();
            this.threadList = new System.Windows.Forms.ListView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.exceptionWindow = new System.Windows.Forms.RichTextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.loadedAssmbt = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.appDomainCb = new System.Windows.Forms.ComboBox();
            this.assembliesList = new System.Windows.Forms.ListView();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.GoButton = new System.Windows.Forms.Button();
            this.BreakPointList = new System.Windows.Forms.ListView();
            this.breakpointStatus = new System.Windows.Forms.RichTextBox();
            this.breakPointText = new System.Windows.Forms.RichTextBox();
            this.CreateBreakPointbt = new System.Windows.Forms.Button();
            this.ShowValButton = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.threadStackTrace);
            this.groupBox2.Controls.Add(this.threadList);
            this.groupBox2.ForeColor = System.Drawing.Color.YellowGreen;
            this.groupBox2.Location = new System.Drawing.Point(11, 8);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(829, 323);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Thread Inforamation";
            // 
            // threadStackTrace
            // 
            this.threadStackTrace.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.threadStackTrace.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.threadStackTrace.Location = new System.Drawing.Point(194, 22);
            this.threadStackTrace.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.threadStackTrace.Name = "threadStackTrace";
            this.threadStackTrace.ReadOnly = true;
            this.threadStackTrace.Size = new System.Drawing.Size(629, 290);
            this.threadStackTrace.TabIndex = 1;
            this.threadStackTrace.Text = "";
            this.threadStackTrace.WordWrap = false;
            
            // 
            // threadList
            // 
            this.threadList.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.threadList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.threadList.FullRowSelect = true;
            this.threadList.GridLines = true;
            this.threadList.Location = new System.Drawing.Point(9, 22);
            this.threadList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.threadList.MultiSelect = false;
            this.threadList.Name = "threadList";
            this.threadList.ShowItemToolTips = true;
            this.threadList.Size = new System.Drawing.Size(177, 290);
            this.threadList.TabIndex = 0;
            this.threadList.UseCompatibleStateImageBehavior = false;
            this.threadList.View = System.Windows.Forms.View.Details;
            this.threadList.SelectedIndexChanged += new System.EventHandler(this.threadList_SelectedIndexChanged);
            this.threadList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.threadList_MouseDoubleClick);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.exceptionWindow);
            this.groupBox3.ForeColor = System.Drawing.Color.YellowGreen;
            this.groupBox3.Location = new System.Drawing.Point(846, 340);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(331, 323);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Exception Catcher";
            // 
            // exceptionWindow
            // 
            this.exceptionWindow.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.exceptionWindow.Location = new System.Drawing.Point(8, 27);
            this.exceptionWindow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.exceptionWindow.Name = "exceptionWindow";
            this.exceptionWindow.ReadOnly = true;
            this.exceptionWindow.Size = new System.Drawing.Size(312, 280);
            this.exceptionWindow.TabIndex = 0;
            this.exceptionWindow.Text = "";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.loadedAssmbt);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.appDomainCb);
            this.groupBox4.Controls.Add(this.assembliesList);
            this.groupBox4.ForeColor = System.Drawing.Color.YellowGreen;
            this.groupBox4.Location = new System.Drawing.Point(13, 337);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Size = new System.Drawing.Size(824, 326);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Loaded Modules";
            // 
            // loadedAssmbt
            // 
            this.loadedAssmbt.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.loadedAssmbt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadedAssmbt.ForeColor = System.Drawing.Color.LightYellow;
            this.loadedAssmbt.Location = new System.Drawing.Point(753, 16);
            this.loadedAssmbt.Name = "loadedAssmbt";
            this.loadedAssmbt.Size = new System.Drawing.Size(56, 26);
            this.loadedAssmbt.TabIndex = 5;
            this.loadedAssmbt.Text = "Get";
            this.loadedAssmbt.UseVisualStyleBackColor = false;
            this.loadedAssmbt.Click += new System.EventHandler(this.loadedAssmbt_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(174, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Application Domain Name";
            // 
            // appDomainCb
            // 
            this.appDomainCb.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.appDomainCb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.appDomainCb.FormattingEnabled = true;
            this.appDomainCb.Location = new System.Drawing.Point(202, 18);
            this.appDomainCb.Name = "appDomainCb";
            this.appDomainCb.Size = new System.Drawing.Size(543, 23);
            this.appDomainCb.TabIndex = 3;
            // 
            // assembliesList
            // 
            this.assembliesList.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.assembliesList.GridLines = true;
            this.assembliesList.Location = new System.Drawing.Point(13, 45);
            this.assembliesList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.assembliesList.MultiSelect = false;
            this.assembliesList.Name = "assembliesList";
            this.assembliesList.Size = new System.Drawing.Size(801, 265);
            this.assembliesList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.assembliesList.TabIndex = 2;
            this.assembliesList.UseCompatibleStateImageBehavior = false;
            this.assembliesList.View = System.Windows.Forms.View.Details;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.ShowValButton);
            this.groupBox6.Controls.Add(this.GoButton);
            this.groupBox6.Controls.Add(this.BreakPointList);
            this.groupBox6.Controls.Add(this.breakpointStatus);
            this.groupBox6.Controls.Add(this.breakPointText);
            this.groupBox6.Controls.Add(this.CreateBreakPointbt);
            this.groupBox6.ForeColor = System.Drawing.Color.GreenYellow;
            this.groupBox6.Location = new System.Drawing.Point(849, 10);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(328, 321);
            this.groupBox6.TabIndex = 4;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "BreakPoints";
            // 
            // GoButton
            // 
            this.GoButton.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.GoButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GoButton.ForeColor = System.Drawing.Color.LightYellow;
            this.GoButton.Location = new System.Drawing.Point(9, 75);
            this.GoButton.Name = "GoButton";
            this.GoButton.Size = new System.Drawing.Size(44, 24);
            this.GoButton.TabIndex = 6;
            this.GoButton.Text = "GO";
            this.GoButton.UseVisualStyleBackColor = false;
            this.GoButton.Click += new System.EventHandler(this.GoButton_Click);
            // 
            // BreakPointList
            // 
            this.BreakPointList.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.BreakPointList.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.BreakPointList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BreakPointList.ForeColor = System.Drawing.Color.OrangeRed;
            this.BreakPointList.FullRowSelect = true;
            this.BreakPointList.GridLines = true;
            this.BreakPointList.Location = new System.Drawing.Point(8, 101);
            this.BreakPointList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BreakPointList.MultiSelect = false;
            this.BreakPointList.Name = "BreakPointList";
            this.BreakPointList.ShowItemToolTips = true;
            this.BreakPointList.Size = new System.Drawing.Size(309, 154);
            this.BreakPointList.TabIndex = 5;
            this.BreakPointList.UseCompatibleStateImageBehavior = false;
            this.BreakPointList.View = System.Windows.Forms.View.Details;
            this.BreakPointList.DoubleClick += new System.EventHandler(this.BreakPointList_DoubleClick);
            // 
            // breakpointStatus
            // 
            this.breakpointStatus.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.breakpointStatus.ForeColor = System.Drawing.Color.Lime;
            this.breakpointStatus.Location = new System.Drawing.Point(8, 20);
            this.breakpointStatus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.breakpointStatus.Name = "breakpointStatus";
            this.breakpointStatus.ReadOnly = true;
            this.breakpointStatus.Size = new System.Drawing.Size(309, 53);
            this.breakpointStatus.TabIndex = 2;
            this.breakpointStatus.Text = "Break info";
            this.breakpointStatus.WordWrap = false;
            // 
            // breakPointText
            // 
            this.breakPointText.Location = new System.Drawing.Point(8, 263);
            this.breakPointText.Name = "breakPointText";
            this.breakPointText.Size = new System.Drawing.Size(255, 43);
            this.breakPointText.TabIndex = 4;
            this.breakPointText.Text = "";
            // 
            // CreateBreakPointbt
            // 
            this.CreateBreakPointbt.BackColor = System.Drawing.SystemColors.ControlDark;
            this.CreateBreakPointbt.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CreateBreakPointbt.ForeColor = System.Drawing.Color.AntiqueWhite;
            this.CreateBreakPointbt.Location = new System.Drawing.Point(265, 263);
            this.CreateBreakPointbt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CreateBreakPointbt.Name = "CreateBreakPointbt";
            this.CreateBreakPointbt.Size = new System.Drawing.Size(61, 43);
            this.CreateBreakPointbt.TabIndex = 3;
            this.CreateBreakPointbt.Text = "Add";
            this.CreateBreakPointbt.UseVisualStyleBackColor = false;
            this.CreateBreakPointbt.Click += new System.EventHandler(this.CreateBreakPointbt_Click);
            // 
            // ShowValButton
            // 
            this.ShowValButton.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.ShowValButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShowValButton.ForeColor = System.Drawing.Color.LightYellow;
            this.ShowValButton.Location = new System.Drawing.Point(55, 75);
            this.ShowValButton.Name = "ShowValButton";
            this.ShowValButton.Size = new System.Drawing.Size(37, 24);
            this.ShowValButton.TabIndex = 7;
            this.ShowValButton.Text = "W";
            this.ShowValButton.UseVisualStyleBackColor = false;
            this.ShowValButton.Click += new System.EventHandler(this.ShowValButton_Click);
            // 
            // CockPit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(1186, 679);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Yellow;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "CockPit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cockpit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CockPit_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox threadStackTrace;
        private System.Windows.Forms.ListView threadList;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RichTextBox exceptionWindow;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView assembliesList;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button CreateBreakPointbt;
        private System.Windows.Forms.RichTextBox breakPointText;
        private System.Windows.Forms.RichTextBox breakpointStatus;
        private System.Windows.Forms.ListView BreakPointList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox appDomainCb;
        private System.Windows.Forms.Button loadedAssmbt;
        private System.Windows.Forms.Button GoButton;
        private System.Windows.Forms.Button ShowValButton;
    }
}

