using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Services.Data
{
    public sealed class Session : ISession
    {
        private static readonly Session _instance = new();
        public static Session Instance { get { return _instance; } } 

        public User? CurrentUser { get; private set; }

        // implement redirection on session change
        public event EventHandler? SessionChanged;
    
        public void Login(User user)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(user);

                CurrentUser = user;
                EventLogger.LogInfo($"New user {user.Name} {user.Role} logged in successfully");
                SessionChanged?.Invoke(this, EventArgs.Empty);                
            } 
            catch (Exception ex)
            {
                EventLogger.LogError("Error occured during Login: " + ex.Message);
            }
        }

        public void Logout()
        {
            CurrentUser = null;
            EventLogger.LogInfo("User logged out.");
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsLoggedIn => CurrentUser != null;
        public string UserRole => CurrentUser?.Role ?? string.Empty;
    }
}
