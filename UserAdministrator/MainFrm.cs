using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ADLibrary;
using HelperLibrary;
using VAkos;
using XMLConfig;

namespace UserAdministrator
{
    public partial class MainFrm : Form
    {
        private Config _config = new Config(Application.StartupPath + "\\config.xml", true);
        private ActiveDirectory _activeDirectory;

        public MainFrm()
        {
            InitializeComponent();
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            string controllerAddress = _config.GetValue("Domain/ControllerAddress", "");
            string container = _config.GetValue("Domain/Container", "");

            if (string.IsNullOrEmpty(controllerAddress) || string.IsNullOrEmpty(container))
            {
                MessageBox.Show("Invalid Domain Controller settings. You need to configure those settings via Manager before you can use this application.","SMS Self Service - User Administrator",MessageBoxButtons.OK,MessageBoxIcon.Error);
                Application.Exit();
            }

            //Initialize encryption and decrypt admin password if it exists
            string encyptedAdminPassword = _config.GetValue("Domain/AdminPassword", "");
            string adminPassword = "";

            if (!string.IsNullOrEmpty(encyptedAdminPassword))
            {
                try
                {
                    EncryptionHelper encryptionHelper =
                        new EncryptionHelper(_config.GetValue("Encryption/Entropy", ""));

                    if (encryptionHelper.IsEntropyGenerated)
                        _config.SetValue("Encryption/Entropy", encryptionHelper.Entropy);

                    adminPassword = encryptionHelper.Decrypt(encyptedAdminPassword);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to decrypt administrator password, please run Manager and enter the password again.\nApplication will now be closed.", "SMS Self Service - User Administrator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }

            _activeDirectory = new ActiveDirectory(controllerAddress, container, _config.GetValue("Domain/AdminUsername", ""), adminPassword);
            LoadUsers();
            serverconnect_label.Text += $"{controllerAddress} ({container})";
        }

        private void LoadUsers()
        {
            try
            {
                userlist_lstview.Items.Clear();
                DirectoryEntry dEntry = _activeDirectory.GetDomainRoot();

                //Init a directory searcher
                DirectorySearcher dSearcher = new DirectorySearcher(dEntry);

                //Filter to search user by username
                dSearcher.Filter = "(&(objectClass=user)("+_activeDirectory.FingerprintAttribute+"=*))";

                //Perform search on active directory
                SearchResultCollection sResults = dSearcher.FindAll();
                foreach (SearchResult searchResult in sResults)
                {
                    ListViewItem item = new ListViewItem((string)searchResult.GetDirectoryEntry().Properties["sAMAccountName"].Value);
                    item.SubItems.Add((string)searchResult.GetDirectoryEntry().Properties["displayName"].Value);
                    item.SubItems.Add((string)searchResult.GetDirectoryEntry().Properties["l"].Value);
                    userlist_lstview.Items.Add(item);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to domain controller, application will now be closed.",
                    "SMS Self Service - User Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

        }

        private void addnewusr_btn_Click(object sender, EventArgs e)
        {
            AddUser adduserfrm = new AddUser(_activeDirectory);
            if (adduserfrm.ShowDialog() == DialogResult.OK)
                LoadUsers();
        }

        private void userlist_lstview_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (userlist_lstview.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    usermenu_contmnustrip.Show(Cursor.Position);
                }
            } 
        }

        private void RemoveUser(string username)
        {
            ADResult phonenumberChangeResult = _activeDirectory.SetUserFingerprint(
            userlist_lstview.FocusedItem.Text, "");
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
                    MessageBox.Show("Successfully removed phone number for user account");
                    break;
            }
            LoadUsers();
        }

        private void removePhoneNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show("Are you sure you want to delete stored phone number for this user?",
                    "SMS Self Service - User Administrator", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
            {
                RemoveUser(userlist_lstview.FocusedItem.Text);
            }
        }

        private void changePhoneNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("In order to change phone number old one needs to be removed, continue?",
                "SMS Self Service - User Administrator", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
            {
                string username = userlist_lstview.FocusedItem.Text;

                RemoveUser(username);

                AddUser adduserfrm = new AddUser(_activeDirectory,username);
                if (adduserfrm.ShowDialog() == DialogResult.OK)
                    LoadUsers();
            }
        }
    }
}
