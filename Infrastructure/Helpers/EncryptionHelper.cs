using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AracKiralamaApp.Infrastructure.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("AracKiralamaAppERPKeySecret2026!"); // 32 bytes key for AES-256
        private static readonly byte[] Iv = new byte[16]; // Fixed IV for simpler demonstration (in production use a random IV)

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = Iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var msEncrypt = new MemoryStream();
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch (Exception)
            {
                return "[Encryption Error]";
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = Iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception)
            {
                return "[Decryption Error]";
            }
        }
    }
}
