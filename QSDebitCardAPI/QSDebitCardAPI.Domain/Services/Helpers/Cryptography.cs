using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QSDataUpdateAPI.Domain.Services.Helpers
{
    public class Cryptography
    {
        public static string token = "7756756757465434343231231243243764586597698yugyghfhgfhcjhvghfvhrftcrcjgjuygubkggyutytj76t87i87698768756r67ff";

        public static string EncryptPhrase(string phrase)
        {
            string encrypted_phrase = string.Empty;
            try
            {
                byte[] baPwd = Encoding.UTF8.GetBytes(token);
                // Hash the password with SHA256
                byte[] baPwdHash = SHA256Managed.Create().ComputeHash(baPwd);
                byte[] baText = Encoding.UTF8.GetBytes(phrase);
                byte[] baSalt = GetRandomBytes();
                byte[] baEncrypted = new byte[baSalt.Length + baText.Length];

                // Combine Salt + Text
                for (int i = 0; i < baSalt.Length; i++)
                    baEncrypted[i] = baSalt[i];
                for (int i = 0; i < baText.Length; i++)
                    baEncrypted[i + baSalt.Length] = baText[i];
                baEncrypted = AES_Encrypt(baEncrypted, baPwdHash);
                encrypted_phrase = Convert.ToBase64String(baEncrypted);
            }
            catch (Exception ex)
            {
                //Logger.LogData(ex.ToString(), "Cryptography/ EncryptPhrase");
            }
            return encrypted_phrase;
        }

        public static string DecryptPhrase(string encrypted_phrase)
        {
            //encrypted_phrase = HttpUtility.UrlDecode(encrypted_phrase);
            string decrypted_phrase = string.Empty;
            try
            {
                byte[] baPwd = Encoding.UTF8.GetBytes(token);

                // Hash the password with SHA256
                byte[] baPwdHash = SHA256Managed.Create().ComputeHash(baPwd);

                byte[] baText = Convert.FromBase64String(encrypted_phrase);

                byte[] baDecrypted = AES_Decrypt(baText, baPwdHash);

                // Remove salt
                int saltLength = GetSaltLength();
                byte[] baResult = new byte[baDecrypted.Length - saltLength];
                for (int i = 0; i < baResult.Length; i++)
                    baResult[i] = baDecrypted[i + saltLength];

                decrypted_phrase = Encoding.UTF8.GetString(baResult);
            }
            catch (Exception ex)
            {
                // Logger.LogData(ex.ToString(), "Cryptography/ DecryptPhrase");
            }
            return decrypted_phrase;
        }

        private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CTS;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CTS;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        private static byte[] GetRandomBytes()
        {
            int saltLength = GetSaltLength();
            byte[] ba = new byte[saltLength];
            RNGCryptoServiceProvider.Create().GetBytes(ba);
            return ba;
        }

        private static int GetSaltLength()
        {
            return 8;
        }
    }
}