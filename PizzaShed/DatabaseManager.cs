using System;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void OpenConnection()
        {
            if (conn == null)
            {
                EventLogger.LogError("OpenConnection(): Database connection is null");
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

        public void CloseConnection()
        {
            if (conn == null)
            {
                EventLogger.LogError("CloseConnection(): Database connection is null");
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
    }
}
