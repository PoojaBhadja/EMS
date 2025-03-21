using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Commons.Helpers
{
    public static class CryptoHelper
    {
        private const int SaltSize = 16; // 128-bit salt
        private const int KeySize = 32;  // 256-bit hash
        private const int Iterations = 10000; // Number of iterations
        //private static readonly string Password = GetEncryptedPassword;
        //private static readonly byte[] VectorBytes = Encoding.ASCII.GetBytes("3kjqwl34akjq8947");
        //private static readonly byte[] SaltBytes = new byte[] { 58, 119, 32, 145, 201, 221, 78, 96 };

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose should not throw when called multiple times")]
        public static string Encrypt(string password)
        {
            using var rng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt); // Generate a cryptographic random salt

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(KeySize);

            // Combine salt + hash and return as Base64
            byte[] hashBytes = new byte[SaltSize + KeySize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

            return Convert.ToBase64String(hashBytes);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose should not throw when called multiple times")]
        public static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(KeySize);

            // Compare stored hash with newly generated hash
            for (int i = 0; i < KeySize; i++)
            {
                if (hashBytes[SaltSize + i] != hash[i])
                    return false;
            }

            return true;
        }
    }
}
