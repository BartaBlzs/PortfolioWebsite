using System.Security.Cryptography;
using System.Text;

namespace Auth.Services
{
    public class Crypto
    {
        public static string Encrypt(string inputString, RSAParameters parameters, bool fOAEP)
        {
            byte[] encryptedData;

            using (RSACryptoServiceProvider rsa = new())
            {
                rsa.ImportParameters(parameters);

                encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(inputString), fOAEP);
            }

            return Convert.ToBase64String(encryptedData);
        }

        public static string Decrypt(string inputString, RSAParameters parameters, bool fOAEP)
        {
            byte[] decryptedData;

            using (RSACryptoServiceProvider rsa = new())
            {
                rsa.ImportParameters(parameters);

                decryptedData = rsa.Decrypt(Convert.FromBase64String(inputString), fOAEP);
            }

            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}

/*
         public static byte[] Encrypt(string plainText, byte[] key, byte[] iv)
        {
            byte[] encrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new())
                {
                    using (CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        encrypted = memoryStream.ToArray();
                    }
                }
            }

            return encrypted;
        }

        public static string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            string decrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new(cipherText))
                {
                    using (CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new(cryptoStream))
                        {
                            decrypted = streamReader.ReadToEnd();
                        }
                    }
                }
            }

            return decrypted;
        }
 */