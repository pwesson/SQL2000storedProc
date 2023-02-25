namespace Wesson.Win.Dev.StoreProc
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
            this.tvwServerExplorer = new System.Windows.Forms.TreeView();
            this.txtGeneratedCode = new System.Windows.Forms.TextBox();
            this.selServers = new System.Windows.Forms.ComboBox();
            this.txtDatabaseName = new System.Windows.Forms.TextBox();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tvwServerExplorer
            // 
            this.tvwServerExplorer.Location = new System.Drawing.Point(4, 59);
            this.tvwServerExplorer.Name = "tvwServerExplorer";
            this.tvwServerExplorer.Size = new System.Drawing.Size(187, 512);
            this.tvwServerExplorer.TabIndex = 0;
            this.tvwServerExplorer.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvwServerExplorer_BeforeExpand);
            this.tvwServerExplorer.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwServerExplorer_AfterSelect);
            // 
            // txtGeneratedCode
            // 
            this.txtGeneratedCode.Location = new System.Drawing.Point(197, 61);
            this.txtGeneratedCode.Multiline = true;
            this.txtGeneratedCode.Name = "txtGeneratedCode";
            this.txtGeneratedCode.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtGeneratedCode.Size = new System.Drawing.Size(728, 510);
            this.txtGeneratedCode.TabIndex = 1;
            // 
            // selServers
            // 
            this.selServers.FormattingEnabled = true;
            this.selServers.Location = new System.Drawing.Point(12, 12);
            this.selServers.Name = "selServers";
            this.selServers.Size = new System.Drawing.Size(114, 21);
            this.selServers.TabIndex = 3;
            this.selServers.Visible = false;
            // 
            // txtDatabaseName
            // 
            this.txtDatabaseName.Location = new System.Drawing.Point(197, 22);
            this.txtDatabaseName.Name = "txtDatabaseName";
            this.txtDatabaseName.Size = new System.Drawing.Size(123, 20);
            this.txtDatabaseName.TabIndex = 12;
            this.txtDatabaseName.Text = "Database name";
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(12, 10);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(114, 43);
            this.cmdConnect.TabIndex = 10;
            this.cmdConnect.Text = "Connect";
            this.cmdConnect.UseVisualStyleBackColor = true;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(937, 583);
            this.Controls.Add(this.txtDatabaseName);
            this.Controls.Add(this.cmdConnect);
            this.Controls.Add(this.selServers);
            this.Controls.Add(this.txtGeneratedCode);
            this.Controls.Add(this.tvwServerExplorer);
            this.Name = "Form1";
            this.Text = "Create stored procedures for SQL databases";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvwServerExplorer;
        private System.Windows.Forms.TextBox txtGeneratedCode;
        private System.Windows.Forms.ComboBox selServers;
        private System.Windows.Forms.TextBox txtDatabaseName;
        private System.Windows.Forms.Button cmdConnect;
    }
}

