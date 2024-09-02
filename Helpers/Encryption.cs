using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BackupApp.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly string EncryptionKey = "your-encryption-key"; // Use a secure key

        public static string EncryptString(string plainText)
        {
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(EncryptionKey);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.GenerateIV();
                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        using (var ms = new MemoryStream())
                        {
                            ms.Write(BitConverter.GetBytes(aes.IV.Length), 0, sizeof(int));
                            ms.Write(aes.IV, 0, aes.IV.Length);
                            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                            {
                                using (var sw = new StreamWriter(cs))
                                {
                                    sw.Write(plainText);
                                }
                            }
                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                throw new Exception("An error occurred during encryption.", ex);
            }
        }
    }
}