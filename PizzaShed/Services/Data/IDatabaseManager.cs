using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Services.Data
{
    public interface IDatabaseManager
    {
        public T ExecuteQuery<T>(Func<SqlConnection, T> dbQuery);
    }
}
