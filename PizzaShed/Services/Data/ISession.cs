using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Model;

namespace PizzaShed.Services.Data
{
    public interface ISession
    {
        event EventHandler? SessionChanged;

        void Login(User user);
        void Logout();

        public User? CurrentUser { get; }
        bool IsLoggedIn { get; }
        string UserRole { get; }
    }
}
