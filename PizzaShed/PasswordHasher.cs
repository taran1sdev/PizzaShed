using System;
using System.Text;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Printing.IndexedProperties;


namespace PizzaShed
{
    public static class PasswordHasher
    {
        private static readonly int SaltSize = 16; // 128 bits
        private static readonly int HashSize = 32; // 256 bits 
        private static readonly int DegreeOfParallelism = 1; // Any more threads and the login process is too slow
        private static readonly int Iterations = 1; // Iterate 4 times
        private static readonly int MemorySize = 1024 * 32; // 32 MB - any more is too resource intensive

        public static bool VerifyPin(string pin, string hashedPin)
        {
            // Decode the stored hash
            byte[] hashBytes = Convert.FromBase64String(hashedPin);

            // Extract salt and hash
            byte[] salt = new byte[SaltSize];
            byte[] hash = new byte[HashSize];

            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            Array.Copy(hashBytes, SaltSize, hash, 0, HashSize);

            // Use the extracted salt to compute the hash of the user pin
            byte[] userHash = HashPin(pin, salt);

            // Check if they match
            return CryptographicOperations.FixedTimeEquals(hash, userHash);
        }
        // Function for creating new PIN entries
        public static string HashPin(string pin)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using var rand = RandomNumberGenerator.Create();
            rand.GetBytes(salt);

            // Hash the pin with new salt value
            byte[] hash = HashPin(pin, salt);

            // Combine the hash and the salt
            var combined = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, combined, 0, salt.Length);
            Array.Copy(hash, 0, combined, salt.Length, hash.Length);

            return Convert.ToBase64String(combined);
        }

        private static byte[] HashPin(string pin, byte[] salt)
        {
            // Generate the argon2 hash
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(pin))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize
            };

            return argon2.GetBytes(HashSize);
        }
    }
}
