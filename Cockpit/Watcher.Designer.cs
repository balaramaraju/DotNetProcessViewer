namespace Cockpit
{
    partial class Watcher
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
            this.paramValuetxt = new System.Windows.Forms.RichTextBox();
            this.paramTree = new System.Windows.Forms.TreeView();
            this.paramDesc = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // paramValuetxt
            // 
            this.paramValuetxt.BackColor = System.Drawing.Color.Gray;
            this.paramValuetxt.ForeColor = System.Drawing.Color.Lime;
            this.paramValuetxt.Location = new System.Drawing.Point(330, 57);
            this.paramValuetxt.Name = "paramValuetxt";
            this.paramValuetxt.ReadOnly = true;
            this.paramValuetxt.Size = new System.Drawing.Size(313, 272);
            this.paramValuetxt.TabIndex = 0;
            this.paramValuetxt.Text = "";
            this.paramValuetxt.TextChanged += new System.EventHandler(this.paramValuetxt_TextChanged);
            // 
            // paramTree
            // 
            this.paramTree.BackColor = System.Drawing.Color.Gray;
            this.paramTree.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.paramTree.ForeColor = System.Drawing.Color.Lime;
            this.paramTree.Location = new System.Drawing.Point(6, 5);
            this.paramTree.Name = "paramTree";
            this.paramTree.Size = new System.Drawing.Size(317, 326);
            this.paramTree.TabIndex = 1;
            this.paramTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.paramTree_AfterSelect);
            this.paramTree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.paramTree_MouseUp);
            // 
            // paramDesc
            // 
            this.paramDesc.AutoSize = true;
            this.paramDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.paramDesc.Location = new System.Drawing.Point(329, 21);
            this.paramDesc.Name = "paramDesc";
            this.paramDesc.Size = new System.Drawing.Size(135, 20);
            this.paramDesc.TabIndex = 2;
            this.paramDesc.Text = "Parameter Type";
            // 
            // Watcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(652, 343);
            this.Controls.Add(this.paramDesc);
            this.Controls.Add(this.paramTree);
            this.Controls.Add(this.paramValuetxt);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.Name = "Watcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Watcher";
            this.Load += new System.EventHandler(this.Watcher_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox paramValuetxt;
        private System.Windows.Forms.TreeView paramTree;
        private System.Windows.Forms.Label paramDesc;
    }
}