namespace UserAdministrator
{
    partial class AddUser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddUser));
            this.save_btn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.phonenumber_txtbox = new System.Windows.Forms.TextBox();
            this.username_txtbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.userselect_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // save_btn
            // 
            this.save_btn.Location = new System.Drawing.Point(12, 99);
            this.save_btn.Name = "save_btn";
            this.save_btn.Size = new System.Drawing.Size(278, 23);
            this.save_btn.TabIndex = 9;
            this.save_btn.Text = "Add user";
            this.save_btn.UseVisualStyleBackColor = true;
            this.save_btn.Click += new System.EventHandler(this.save_btn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Phone Number (10 digits):";
            // 
            // phonenumber_txtbox
            // 
            this.phonenumber_txtbox.Location = new System.Drawing.Point(15, 73);
            this.phonenumber_txtbox.MaxLength = 10;
            this.phonenumber_txtbox.Name = "phonenumber_txtbox";
            this.phonenumber_txtbox.Size = new System.Drawing.Size(275, 20);
            this.phonenumber_txtbox.TabIndex = 1;
            // 
            // username_txtbox
            // 
            this.username_txtbox.Location = new System.Drawing.Point(15, 25);
            this.username_txtbox.Name = "username_txtbox";
            this.username_txtbox.ReadOnly = true;
            this.username_txtbox.Size = new System.Drawing.Size(238, 20);
            this.username_txtbox.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Username:";
            // 
            // userselect_btn
            // 
            this.userselect_btn.Location = new System.Drawing.Point(259, 25);
            this.userselect_btn.Name = "userselect_btn";
            this.userselect_btn.Size = new System.Drawing.Size(35, 20);
            this.userselect_btn.TabIndex = 10;
            this.userselect_btn.Text = "...";
            this.userselect_btn.UseVisualStyleBackColor = true;
            this.userselect_btn.Click += new System.EventHandler(this.userselect_btn_Click);
            // 
            // AddUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 137);
            this.Controls.Add(this.userselect_btn);
            this.Controls.Add(this.save_btn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.phonenumber_txtbox);
            this.Controls.Add(this.username_txtbox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AddUser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add new user";
            this.Load += new System.EventHandler(this.AddUser_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button save_btn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox phonenumber_txtbox;
        private System.Windows.Forms.TextBox username_txtbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button userselect_btn;
    }
}