using System;
using System.IO;
using System.Windows;

namespace PizzaShed.Services.Logging
{
    // Static utility class for logging events
    public static class EventLogger
    {        
        // Change this to the user's ApplicationData directory in production
        private static readonly string logPath = Path.Combine(@"C:\Users\thoma\source\repos\PizzaShed\PizzaShed\Logs",$"{DateTime.Now:yy-MM-dd}-Log.txt");        

        // Functions to create info and error log entries
        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        // Function to write to the log file
        private static void Log(string level, string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now:HH:mm:ss} [{level}]: {message}" + Environment.NewLine;
                File.AppendAllText(logPath, logMessage);
            } catch (Exception ex)
            {
                // Just for testing purposes - remove in production
                MessageBox.Show("Unable to write to log file: " + ex.Message);
            }
        }
    }
}
