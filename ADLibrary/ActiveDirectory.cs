using HelperLibrary;
using System;
using System.DirectoryServices;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ADLibrary
{
    public enum ADResult
    {
        UserNotFound,
        DisabledAccount,
        InternalError,
        Success
    };

    public enum PasswordChangeResult
    {
        UserNotFound,
        DisabledAccount,
        NoFingerprintAttached,
        InvalidFingerprint,
        InvalidPassword,
        InternalError,
        Success
    };

    public class ActiveDirectory
    {
        private readonly string _controllerAddress;
        private readonly string _container;
        private readonly bool _useAdminAccount;
        private readonly string _adminUsername;
        private readonly string _adminPassword;

        /// <summary>Attribute used to store user's fingerprint (default: extensionAttribute15)</summary>
        public string FingerprintAttribute { get; set; } = "extensionAttribute15";

        public ActiveDirectory(string controllerAddress, string container, string adminUsername = "", string adminPassword = "")
        {
            _controllerAddress = controllerAddress ?? throw new ArgumentNullException(nameof(controllerAddress), "Controller address cannot be null");
            _container = container ?? throw new ArgumentNullException(nameof(container), "Active Directory container path cannot be null"); ; ;

            if (!string.IsNullOrEmpty(adminUsername) && !string.IsNullOrEmpty(adminPassword))
            {
                _adminUsername = adminUsername;
                _adminPassword = adminPassword;
                _useAdminAccount = true;
            }
        }

        //TODO: FOLLOW AD PASSWORD POLICY OR ADD CHECK THAT SETPASSWORD RETURNED ERROR
        public PasswordChangeResult ChangeUserPassword(string userName, string newPassword, string fingerprint)
        {
            if(string.IsNullOrEmpty(newPassword))
                throw new ArgumentNullException(nameof(newPassword), "Password cannot be null or empty");

            if (string.IsNullOrEmpty(fingerprint))
                throw new ArgumentNullException(nameof(fingerprint), "Phone number cannot be null or empty");

            DirectoryEntry userEntry = FindUser(userName);

            if(userEntry == null)
                return PasswordChangeResult.UserNotFound;

            if (!IsActiveAccount(userEntry))
                return PasswordChangeResult.DisabledAccount;

            if (string.IsNullOrEmpty((string)userEntry.Properties[FingerprintAttribute].Value))
                return PasswordChangeResult.NoFingerprintAttached;

            if (!String.Equals((string)userEntry.Properties[FingerprintAttribute].Value, MD5Helper.GetHash(fingerprint)))
                return PasswordChangeResult.InvalidFingerprint;

            userEntry.Invoke("SetPassword", new object[] { newPassword });

            userEntry.Properties["pwdLastSet"].Value = 0;

            if (PasswordNotExpires(userEntry))
                userEntry.Properties["userAccountControl"].Value =
                    (int) userEntry.Properties["userAccountControl"].Value - 0x10000;

            userEntry.Properties["LockOutTime"].Value = 0;

            userEntry.CommitChanges();

            return PasswordChangeResult.Success;
        }

        public ADResult SetUserFingerprint(string username, string fingerprint)
        {
            if (string.IsNullOrEmpty(fingerprint))
                throw new ArgumentNullException(nameof(fingerprint), "Fingerprint cannot be null or empty");

            DirectoryEntry userEntry = FindUser(username);

            if (userEntry == null)
                return ADResult.UserNotFound;

            if (!IsActiveAccount(userEntry))
                return ADResult.DisabledAccount;

            if(string.IsNullOrEmpty(fingerprint))
                userEntry.Properties[FingerprintAttribute].Clear();
            else
                userEntry.Properties[FingerprintAttribute].Value = MD5Helper.GetHash(fingerprint);

            userEntry.CommitChanges();

            return ADResult.Success;
        }

        public bool IsActiveAccount(DirectoryEntry de)
        {
            if (de?.NativeGuid == null)
                throw new ArgumentNullException(nameof(de), "Invalid DirectoryEntry");

            int flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }

        public bool PasswordNotExpires(DirectoryEntry de)
        {
            if (de?.NativeGuid == null)
                throw new ArgumentNullException(nameof(de), "Invalid DirectoryEntry");

            int flags = (int)de.Properties["userAccountControl"].Value;

            return Convert.ToBoolean(flags & 0x10000);
        }

        public DirectoryEntry GetDomainRoot()
        {
            const AuthenticationTypes authenticationTypes = AuthenticationTypes.Secure |
    AuthenticationTypes.Sealing | AuthenticationTypes.ServerBind;

            DirectoryEntry searchRoot;
            if (_useAdminAccount)
                searchRoot = new DirectoryEntry($"LDAP://{_controllerAddress}/{_container}"
                    , _adminUsername, _adminPassword,
                    authenticationTypes
                    );
            else
                searchRoot = new DirectoryEntry($"LDAP://{_controllerAddress}/{_container}"
                );

            return searchRoot;
        }

        public DirectoryEntry FindUser(string username)
        {
            try
            {
                DirectoryEntry searchRoot = GetDomainRoot();

                var searcher = new DirectorySearcher(searchRoot);
                searcher.Filter = $"sAMAccountName={username}";
                searcher.SearchScope = SearchScope.Subtree;
                searcher.CacheResults = false;

                SearchResult searchResult = searcher.FindOne();

                if (searchResult == null)
                    return null;

                var userEntry = searchResult.GetDirectoryEntry();
                return userEntry;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
