using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ADLibrary;
using HelperLibrary;

namespace UserAdministrator
{
    public partial class AddUser : Form
    {
        private ActiveDirectory activeDirectory;
        public AddUser(ActiveDirectory ad, string username = "")
        {
            InitializeComponent();
            activeDirectory = ad;
            if (!string.IsNullOrEmpty(username))
                username_txtbox.Text = username;
        }

        private void AddUser_Load(object sender, EventArgs e)
        {

        }

        private bool IsPhone(String strPhone)
        {
            Regex objPhonePattern = new Regex(@"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$");
            return objPhonePattern.IsMatch(strPhone);
        }	

        private void save_btn_Click(object sender, EventArgs e)
        {
            if (!IsPhone(phonenumber_txtbox.Text))
            {
                MessageBox.Show("Specified phone number is not valid");
                return;
            }
            ADResult phonenumberChangeResult = activeDirectory.SetUserFingerprint(username_txtbox.Text,
                MD5Helper.GetHash(phonenumber_txtbox.Text));
            switch (phonenumberChangeResult)
            {
                case ADResult.UserNotFound:
                    MessageBox.Show("User Not Found");
                    break;
                case ADResult.DisabledAccount:
                    MessageBox.Show("Specified account is disabled");
                    break;
                case ADResult.InternalError:
                    MessageBox.Show("Unable to set phone number due to internal error");
                    break;
                case ADResult.Success:
                    MessageBox.Show("Successfully set phone number");
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
            }
        }

        private void userselect_btn_Click(object sender, EventArgs e)
        {
            SelectUser su = new SelectUser(activeDirectory);
            if (su.ShowDialog() == DialogResult.OK)
                username_txtbox.Text = su.userlist_lstview.SelectedItems[0].Text;
        }
    }
}
