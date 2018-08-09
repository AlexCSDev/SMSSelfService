using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ADLibrary;

namespace UserAdministrator
{
    public partial class SelectUser : Form
    {
        private ActiveDirectory activeDirectory;
        public SelectUser(ActiveDirectory ad)
        {
            InitializeComponent();
            activeDirectory = ad;
        }

        private void find_btn_Click(object sender, EventArgs e)
        {
            try
            {
                userlist_lstview.Items.Clear();
                DirectoryEntry dEntry = activeDirectory.GetDomainRoot();

                //init a directory searcher
                DirectorySearcher dSearcher = new DirectorySearcher(dEntry);

                //This line applies a filter to the search specifying a username to search for
                //modify this line to specify a user name. if you want to search for all
                //users who start with k - set SearchString to "k"
                dSearcher.Filter = "(&(objectClass=user)(cn=*"+lastname_txtbox.Text+"*))"; //TODO: Allow sam search too

                //perform search on active directory
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

        private void userlist_lstview_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (userlist_lstview.SelectedItems.Count < 1)
                ok_btn.Enabled = false;
            else
                ok_btn.Enabled = true;
        }

        private void lastname_txtbox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                find_btn_Click(this, new EventArgs());
            }
        }
    }
}
