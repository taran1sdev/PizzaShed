using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Services.Data
{
    public interface IUserRepository
    {
        bool GetUserByPin(string pin);
    }
}
