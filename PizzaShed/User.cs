using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed
{
    class User(int id, string name, string pin, string role)
    {
        // User fields can only be set when the object is created
        private readonly int id = id;
        private readonly string name = name;
        private readonly string pin = pin;
        private readonly string role = role;

        // We use public properties to retrieve the user info 
        public int Id { get { return id; } }
        public string Name { get { return name; } } 
        public string Pin { get { return pin; } }
        public string Role { get { return role; } } 
    }
}
