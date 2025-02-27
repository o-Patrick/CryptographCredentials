using CryptographCredentials.Framework.LogManagement.Interfaces;
using System.Text;

namespace CryptographCredentials.Framework.LogManagement
{
    public class LogHandler : ILogHandler
    {
        #region | Fields |
        private readonly StringBuilder _fileContent;
        #endregion

        #region | Constructor |
        public LogHandler()
        {
            _fileContent = new StringBuilder();
        }
        #endregion

        #region | Methods |
        /// <summary>
        /// Logs messages into the console
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void ConsoleWrite(string message)
        {
            message = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss.fff} | {message}";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Writes a message and reads another on the console
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string? ConsoleRead(string message)
        {
            ConsoleWrite(message);
            return Console.ReadLine();
        }

        /// <summary>
        /// StringBuilder for logging
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public void FileBuilder(string message)
        {
            message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {message}";
            ConsoleWrite(message);
            _fileContent.AppendLine(message);
        }

        /// <summary>
        /// Saves log file locally
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        public async Task<bool> SaveFileLocallyAsync(string processName)
        {
            try
            {
                string fileName = $"{DateTime.Now:yyyMMddTHHmmss}_{processName}.txt";
                string fileFullPath = Path.Combine(Directory.GetCurrentDirectory(), "Log", fileName);

                using var streamWriter = new StreamWriter(fileFullPath, true, Encoding.UTF8, _fileContent.Length);
                await streamWriter.WriteAsync(_fileContent);
                await streamWriter.FlushAsync();

                ConsoleWrite($"{fileName} saved successfully");
                return true;
            }
            catch (Exception exc)
            {
                ConsoleWrite($"Exception {exc}");
            }

            return false;
        }
        #endregion
    }
}
