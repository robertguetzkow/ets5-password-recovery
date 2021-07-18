/*
MIT License
Copyright (c) 2021 Robert Gützkow

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace ETS5PasswordRecovery
{
    class Deobfuscator
    {
        private static byte[] AesCbcDecrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(ciphertext, 0, ciphertext.Length);
                    }
                }
                return memoryStream.ToArray();
            }
        }
        private static string Decrypt(string ciphertextBase64, string password, string salt)
        {
            byte[] ciphertext = Convert.FromBase64String(ciphertextBase64);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, saltBytes);
            byte[] key = passwordDeriveBytes.GetBytes(32);
            byte[] iv = passwordDeriveBytes.GetBytes(16);
            byte[] plaintext = AesCbcDecrypt(ciphertext, key, iv);
            return Encoding.Unicode.GetString(plaintext);
        }
        public static string Deobfuscate(string obfuscatedValue)
        {
            return Decrypt(obfuscatedValue, "ETS5Password", "Ivan Medvedev");
        }
    }
}
