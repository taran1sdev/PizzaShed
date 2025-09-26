using System;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace PizzaShed
{
    // Sealed class ensures no other classes can inherit from this class
    public sealed class DatabaseManager
    {
        // We create a static readonly property to ensure we only ever have a single instance of the class
        private static readonly DatabaseManager instance = new();
        private readonly SqlConnection? conn;        

        // Using a private constructor prevents outside code from creating a new instance of this class
        private DatabaseManager()
        {
            // For testing - in production use a secure secrets management solution
            string connectionString = @"Server=localhost\SQLEXPRESS01;Database=PizzaShed;User ID=PizzaShedDB;Password=PizzaShedDBPassword;TrustServerCertificate=True;Trusted_Connection=True;";

            try
            {
                conn = new SqlConnection(connectionString);
            }
            catch (Exception ex)
            {                
                EventLogger.LogError("Failed to establish database connection: " + ex.Message);
            }
        }

        // Public property to retrieve the instance of the class
        public static DatabaseManager Instance { get { return instance; } }

        private void OpenConnection()
        {
            if (conn == null)
            {
                EventLogger.LogError("DatabaseManager.OpenConnection: Database connection is null");
                return;
            }

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    conn.Open();
                    EventLogger.LogInfo("Database connection opened successfully");
                    return;
                }
                catch (Exception ex)
                {
                    EventLogger.LogError("Failed to open database connection: " + ex.Message);                    
                }
            }            
        }

        private void CloseConnection()
        {
            if (conn == null)
            {
                EventLogger.LogError("DatabaseManager.CloseConnection: Database connection is null");
                return;
            }

            if (conn.State == System.Data.ConnectionState.Open)
            {
                try 
                {
                    conn.Close();
                    EventLogger.LogInfo("Connection to database closed successfully");
                    return;
                } catch (Exception ex)
                {
                    EventLogger.LogError("Failed to close database connection: " + ex.Message);                    
                }
            } 
        }

        public User[] GetUsers()
        {
            // We create a list to hold the user objects as we do not know the quantity the query will return
            List<User> users = [];

            // Variables to store our user info
            int id;
            string? name, pin, role;

            string queryString = "SELECT user_id, name, PIN, role FROM Users;";

            try
            {
                // SQL Command object holds our query string and database connection
                SqlCommand query = new(queryString, conn);
                OpenConnection();

                // We execute the query and store the records returned in an SqlDataReader object
                SqlDataReader reader = query.ExecuteReader();

                // Loop over all the records returned by the query
                while (reader.Read())
                {
                    id = Convert.ToInt32(reader["user_id"]);
                    name = reader["name"].ToString();
                    pin = reader["PIN"].ToString();
                    role = reader["role"].ToString();

                    if (name != null && pin != null && role != null)
                    {
                        users.Add(new User(id, name, pin, role));
                    }                    
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to get Users from Database: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            // Check that we have data to return
            if (users.Count > 0)
            {
                // Convert the List to an Array before returning
                return [.. users];
            }
            else
            {
                EventLogger.LogError("GetUsers query returned no items.");
                return [];
            }
        }
    }
}
