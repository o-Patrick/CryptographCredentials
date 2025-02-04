using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using ServiceExternals.Interfaces;

namespace Service.CryptographCredentials
{
    public class CryptographCredentialsService
    {
        #region | Fields |
        private readonly IConfiguration _configuration;
        private readonly ILogHandler _logHandler;
        private readonly string _processName;
        private readonly string _fileNameToBeProcessed;
        #endregion

        #region | Constructor |
        public CryptographCredentialsService(IConfiguration configuration, ILogHandler logHandler)
        {
            _configuration = configuration;
            _logHandler = logHandler;
            _processName = nameof(CryptographCredentialsService);
            _fileNameToBeProcessed = _configuration.GetSection("FileNameToBeProcessed").Value ?? "";
        }
        #endregion

        #region | Methods |
        /// <summary>
        /// Main method
        /// Gets the main directory to be read
        /// Calls for log file to be saved locally
        /// </summary>
        public void Execute()
        {
            var _logContent = new StringBuilder();

            Console.WriteLine("Enter the directory path:");
            //string directoryPath = Console.ReadLine() ?? "";
            string directoryPath = "C:\\Users\\tickt\\source\\repos\\datamotion\\LigueMedicina\\LigueMedicina_MiddlewareServices";

            if (Directory.Exists(directoryPath))
            {
                ProcessDirectory(directoryPath);
                _logContent = _logHandler.FileBuilder("Processing completed.");
            }
            else
            {
                _logContent = _logHandler.FileBuilder("Directory not found.");
            }

            _logHandler.SaveFileLocallyAsync(_processName, _logContent.ToString());
        }

        #region | Private methods |
        /// <summary>
        /// Calls only for appsettings.json files to be read
        /// </summary>
        /// <param name="targetDirectory"></param>
        private void ProcessDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory, _fileNameToBeProcessed, SearchOption.AllDirectories);
            
            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName);
            }
        }

        /// <summary>
        /// Processes files
        /// </summary>
        /// <param name="path"></param>
        private void ProcessFile(string path)
        {
            try
            {
                string jsonContent = File.ReadAllText(path);
                JsonNode? jsonNode = JsonNode.Parse(jsonContent);

                if (jsonNode != null)
                {
                    UpdateJsonValues(jsonNode);
                    File.WriteAllText(path, jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                    _logHandler.FileBuilder($"Processed: {path}");
                }
            }
            catch (Exception ex)
            {
                _logHandler.FileBuilder($"Error processing file {path}: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes JSON properties
        /// </summary>
        /// <param name="node"></param>
        private void UpdateJsonValues(JsonNode node)
        {
            if (node is JsonArray jsonArray)
            {
                // Recursively calls this function if value is an array
                foreach (var item in jsonArray)
                {
                    UpdateJsonValues(item!);
                }
            }
            else if (node is JsonObject jsonObject)
            {
                foreach (var property in jsonObject)
                {
                    if (property.Value is JsonObject || property.Value is JsonArray)
                    {
                        // Recursively calls this function if value is an object or array
                        UpdateJsonValues(property.Value!);
                    }
                    else if (property.Value is JsonValue value)
                    {
                        // Checks if property is a credential
                        if (IsSensitiveKey(property.Key))
                        {
                            string originalValue = value.ToString();
                            jsonObject[property.Key] = ComputeHash(originalValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if property is a credential 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool IsSensitiveKey(string key)
        {
            return key.Contains("login", StringComparison.OrdinalIgnoreCase)
                || key.Contains("user", StringComparison.OrdinalIgnoreCase)
                || key.Contains("email", StringComparison.OrdinalIgnoreCase)
                || key.Contains("password", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Computes SHA-256 hash
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string ComputeHash(string data)
        {
            var stringBuilder = new StringBuilder();
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));

            foreach (var bytes in hashBytes)
            {
                stringBuilder.Append(bytes.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
        #endregion
        #endregion
    }
}
