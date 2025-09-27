using System;
using System.Text;
using System.Security.Cryptography;
using System.Printing.IndexedProperties;


namespace PizzaShed.Services.Security
{
    public static class PasswordHasher
    {        
        public static bool VerifyPin(string pin, string hashedPin)
        {
            // Decode the stored hash
            byte[] hash = Convert.FromBase64String(hashedPin);
            byte[] pinBytes = Encoding.UTF8.GetBytes(pin);

            // Convert user input to sha512 hash and check if it matches stored hash
            using (SHA512 hasher = SHA512.Create()) 
            {
                byte[] userHash = hasher.ComputeHash(pinBytes);
                return CryptographicOperations.FixedTimeEquals(hash, userHash);
            }
        }
        // Function for creating new PIN entries
        public static string HashPin(string pin)
        {
            // Generate the SHA512 hash
            // Implement a more secure approach for production
            byte[] pinBytes = Encoding.UTF8.GetBytes(pin);

            using (SHA512 hasher = SHA512.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(pinBytes);
                return Convert.ToBase64String(hashBytes);                
            }
        }
    }
}
