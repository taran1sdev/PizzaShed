using System;
using System.Collections.Generic;
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

        bool IsLoggedIn { get; }
        string UserRole { get; }
    }
}
