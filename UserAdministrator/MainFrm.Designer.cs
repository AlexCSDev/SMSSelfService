namespace UserAdministrator
{
    partial class MainFrm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm));
            this.userlist_lstview = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.usermenu_contmnustrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.changePhoneNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.removePhoneNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addnewusr_btn = new System.Windows.Forms.Button();
            this.serverconnect_label = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.usermenu_contmnustrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // userlist_lstview
            // 
            this.userlist_lstview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.userlist_lstview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader2});
            this.userlist_lstview.Location = new System.Drawing.Point(12, 12);
            this.userlist_lstview.Name = "userlist_lstview";
            this.userlist_lstview.Size = new System.Drawing.Size(575, 243);
            this.userlist_lstview.TabIndex = 7;
            this.userlist_lstview.UseCompatibleStateImageBehavior = false;
            this.userlist_lstview.View = System.Windows.Forms.View.Details;
            this.userlist_lstview.MouseClick += new System.Windows.Forms.MouseEventHandler(this.userlist_lstview_MouseClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Account Name";
            this.columnHeader1.Width = 140;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Person Name";
            this.columnHeader3.Width = 143;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "City";
            this.columnHeader2.Width = 111;
            // 
            // usermenu_contmnustrip
            // 
            this.usermenu_contmnustrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changePhoneNumberToolStripMenuItem,
            this.toolStripSeparator1,
            this.removePhoneNumberToolStripMenuItem});
            this.usermenu_contmnustrip.Name = "contextMenuStrip1";
            this.usermenu_contmnustrip.Size = new System.Drawing.Size(200, 54);
            // 
            // changePhoneNumberToolStripMenuItem
            // 
            this.changePhoneNumberToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("changePhoneNumberToolStripMenuItem.Image")));
            this.changePhoneNumberToolStripMenuItem.Name = "changePhoneNumberToolStripMenuItem";
            this.changePhoneNumberToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.changePhoneNumberToolStripMenuItem.Text = "Change phone number";
            this.changePhoneNumberToolStripMenuItem.Click += new System.EventHandler(this.changePhoneNumberToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(196, 6);
            // 
            // removePhoneNumberToolStripMenuItem
            // 
            this.removePhoneNumberToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("removePhoneNumberToolStripMenuItem.Image")));
            this.removePhoneNumberToolStripMenuItem.Name = "removePhoneNumberToolStripMenuItem";
            this.removePhoneNumberToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.removePhoneNumberToolStripMenuItem.Text = "Remove phone number";
            this.removePhoneNumberToolStripMenuItem.Click += new System.EventHandler(this.removePhoneNumberToolStripMenuItem_Click);
            // 
            // addnewusr_btn
            // 
            this.addnewusr_btn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addnewusr_btn.Location = new System.Drawing.Point(12, 261);
            this.addnewusr_btn.Name = "addnewusr_btn";
            this.addnewusr_btn.Size = new System.Drawing.Size(575, 23);
            this.addnewusr_btn.TabIndex = 9;
            this.addnewusr_btn.Text = "Add new user";
            this.addnewusr_btn.UseVisualStyleBackColor = true;
            this.addnewusr_btn.Click += new System.EventHandler(this.addnewusr_btn_Click);
            // 
            // serverconnect_label
            // 
            this.serverconnect_label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.serverconnect_label.AutoSize = true;
            this.serverconnect_label.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.serverconnect_label.Location = new System.Drawing.Point(9, 287);
            this.serverconnect_label.Name = "serverconnect_label";
            this.serverconnect_label.Size = new System.Drawing.Size(96, 13);
            this.serverconnect_label.TabIndex = 10;
            this.serverconnect_label.Text = "Domain Controller: ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label1.Location = new System.Drawing.Point(402, 288);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(185, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Copyright 2015-2018 Aleksey Tsutsey";
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 310);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serverconnect_label);
            this.Controls.Add(this.addnewusr_btn);
            this.Controls.Add(this.userlist_lstview);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SMS Self Service - User Administrator";
            this.Load += new System.EventHandler(this.MainFrm_Load);
            this.usermenu_contmnustrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView userlist_lstview;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ContextMenuStrip usermenu_contmnustrip;
        private System.Windows.Forms.ToolStripMenuItem changePhoneNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem removePhoneNumberToolStripMenuItem;
        private System.Windows.Forms.Button addnewusr_btn;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label serverconnect_label;
        private System.Windows.Forms.Label label1;
    }
}

