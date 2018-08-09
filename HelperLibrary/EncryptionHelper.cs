using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    ///This class uses Windows Data Protection API for text data encryption
    public class EncryptionHelper
    {
        private readonly byte[] _entropy;

        /// <summary>Entropy data used for encryption, should be saved in order to decrypt data encrypted with current instance of the class</summary>    
        public string Entropy => Convert.ToBase64String(_entropy);

        /// <summary>This property shows if entropy for this instance of class was generated (true) or loaded (false)</summary>
        public bool IsEntropyGenerated { get; }

        public EncryptionHelper(string entropy = null)
        {
            if (entropy != null)
            {
                try
                {
                    _entropy = Convert.FromBase64String(entropy);
                    if(_entropy.Length != 20)
                        throw new ArgumentException($"Invalid entropy length (expected 20 bytes, received: {_entropy.Length})");
                    IsEntropyGenerated = false;
                    return;
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Provided entropy is not valid");
                }
            }

            //If we are here then we need to generate a new entropy
            _entropy = new byte[20];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(_entropy);

            IsEntropyGenerated = true;
        }

        public string Encrypt(string plainText)
        {
            byte[] encryptedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), _entropy, DataProtectionScope.LocalMachine);

            return Convert.ToBase64String(encryptedData);
        }

        public string Decrypt(string cipherText)
        {
            byte[] plaintext = ProtectedData.Unprotect(Convert.FromBase64String(cipherText), _entropy,
                DataProtectionScope.LocalMachine);

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}
