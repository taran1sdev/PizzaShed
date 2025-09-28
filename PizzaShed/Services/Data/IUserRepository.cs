using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Model;

namespace PizzaShed.Services.Data
{
    public interface IUserRepository
    {
        User? GetUserByPin(string pin);
    }
}
