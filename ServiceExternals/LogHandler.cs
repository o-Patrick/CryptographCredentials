using ServiceExternals.Interfaces;
using System.Text;

namespace ServiceExternals
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
        /// StringBuilder for logging
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public StringBuilder FileBuilder(string message)
        {
            return _fileContent.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {message}");
        }

        /// <summary>
        /// Saves log file locally
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        public async Task SaveFileLocallyAsync(string processName)
        {
            string fileName = $"{DateTime.Now:yyyMMddTHHmmss}_{processName}";
            string fileFullPath = Path.Combine(Directory.GetCurrentDirectory(), "Log", fileName);
            
            using var streamWriter = new StreamWriter(fileFullPath, true, Encoding.UTF8, _fileContent.Length);
            await streamWriter.WriteAsync(_fileContent);
            await streamWriter.FlushAsync();
        }
        #endregion
    }
}
