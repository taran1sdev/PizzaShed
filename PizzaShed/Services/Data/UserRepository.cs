using Microsoft.Data.SqlClient;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System;

namespace PizzaShed.Services.Data
{
    public class UserRepository : IUserRepository
    {
        // Set properties to allow us to change the session and execute queries
        private readonly IDatabaseManager _databaseManager;        

        public UserRepository(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }
        // Login functionality - query the database for user with matching pin and return a User object
        public User? GetUserByPin(string pin)
        {            
            try
            {
                // We call the execute query function that handles the DB connection
                User? user = _databaseManager.ExecuteQuery(conn =>
                {                    
                    string queryString = "SELECT user_id, name, role FROM Users WHERE PIN = @pin;";

                    using (SqlCommand query = new(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@pin", pin);

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            // PIN value is unique - if we have a row then it matches our user
                            if (reader.HasRows)
                            {
                                reader.Read();
                                int id = Convert.ToInt32(reader["user_id"]);
                                string? name = reader["name"].ToString();
                                string? role = reader["role"].ToString();

                                if (name != null && role != null)
                                {                                    
                                    return new User(id, name, role);                                    
                                }
                            }                            
                        }
                    }
                    return null;
                });
                if (user != null)
                {                    
                    return user;
                }
                return null;
            }
            catch (Exception ex) 
            {                
                EventLogger.LogError("UserManager.GetUserByPinError: Error occured during query execution: " + ex.Message);
                return null;
            }                        
        }
    }
}
