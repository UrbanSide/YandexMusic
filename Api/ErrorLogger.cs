using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaMusicRPC
{
    public class ErrorLogger
    {
        private readonly string _logFilePath;

        public ErrorLogger(string logFilePath)
        {
            Directory.CreateDirectory("logs");
            _logFilePath = "logs/"+logFilePath;
        }
        public void LogError(Exception ex)
        {
            using (StreamWriter writer = new StreamWriter(_logFilePath, true))
            {
                writer.WriteLine($"Error: {DateTime.Now}");
                writer.WriteLine($"Message: {ex.Message}");
                writer.WriteLine($"Stack trace: {ex.StackTrace}");
                writer.WriteLine();
            }
        }
        public void LogSting(string ex)
        {
            using (StreamWriter writer = new StreamWriter(_logFilePath, true))
            {
                writer.WriteLine($"Error: {DateTime.Now}");
                writer.WriteLine($"Message: {ex}");
            }
        }
    }
}
