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

        public void Login(User user);
        public void Logout();

        public User? CurrentUser { get; }
        bool IsLoggedIn { get; }
        string UserRole { get; }
    }
}
