using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class User(int id, string name, string role)
    {
        // User fields can only be set when the object is created
        private readonly int id = id;
        private readonly string name = name;        
        private readonly string role = role;

        // We use public properties to retrieve the user info 
        public int Id { get { return id; } }
        public string Name { get { return name; } }         
        public string Role { get { return role; } } 
    }
}
