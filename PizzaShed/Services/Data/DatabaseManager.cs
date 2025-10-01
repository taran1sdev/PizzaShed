using System;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using PizzaShed.Services.Logging;

namespace PizzaShed.Services.Data
{
    // Sealed class ensures no other classes can inherit from this class
    public sealed class DatabaseManager
    {
        // We create a static readonly property to ensure we only ever have a single instance of the class
        private static readonly DatabaseManager instance = new();
        private readonly SqlConnection? conn;        

        // Using a private constructor prevents outside code from creating a new instance of this class
        public DatabaseManager()
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

        private bool OpenConnection()
        {
            if (conn == null)
            {
                EventLogger.LogError("DatabaseManager.OpenConnection: Database connection is null");
                return false;
            }

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    conn.Open();                    
                    return true;
                }
                catch (Exception ex)
                {
                    EventLogger.LogError("Failed to open database connection: " + ex.Message);
                    return false;
                }
            }
            return true; // If we reach here then the Connection is already open
        }

        private bool CloseConnection()
        {
            if (conn == null)
            {
                EventLogger.LogError("DatabaseManager.CloseConnection: Database connection is null");
                return false;
            }

            if (conn.State == System.Data.ConnectionState.Open)
            {
                try 
                {
                    conn.Close();                    
                    return true;
                } catch (Exception ex)
                {
                    EventLogger.LogError("Failed to close database connection: " + ex.Message);
                    return false;
                }
            }
            return true; // Again, if we reach here the connection is already closed
        }
        
        // This function allows us to manage the Database connection within our protected singleton class
        // The function to execute is passed as a parameter and we can specify the return type when calling the method
        public T ExecuteQuery<T>(Func<SqlConnection, T> dbQuery)
        {
            try
            {
                if (conn != null && OpenConnection())
                {
                    return dbQuery(conn);
                } 

                return default!;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Database query failed: " + ex.Message);
                return default!;
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}
