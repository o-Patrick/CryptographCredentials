using static CryptographCredentials.Framework.LogManagement.LogHandler;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using CryptographCredentials.Domain.Entities;
using CryptographCredentials.Domain.Enums;
using CryptographCredentials.Framework.LogManagement.Interfaces;

namespace Service.CryptographCredentials
{
    public class CryptographCredentialsService
    {
        #region | Fields |
        private readonly IConfiguration _configuration;
        private readonly ILogHandler _logHandler;
        private readonly string _processName;
        private readonly string _fileNameToBeProcessed;
        private readonly ReplaceFor _replaceForConfig;
        private readonly EReplaceOptions? _replaceOption;
        #endregion

        #region | Constructor |
        public CryptographCredentialsService(IConfiguration configuration, ILogHandler logHandler)
        {
            _configuration = configuration;
            _logHandler = logHandler;
            _processName = nameof(CryptographCredentialsService);
            _fileNameToBeProcessed = _configuration.GetSection("FileNameToBeProcessed").Value ?? "";
            _replaceForConfig = _configuration.GetSection("ReplaceFor").Get<ReplaceFor>() ?? new ReplaceFor();

            try
            {
                _replaceOption = _replaceForConfig.GetActiveOption();
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Exception: {exc.Message}");
            }
        }
        #endregion

        #region | Methods |
        /// <summary>
        /// Main method
        /// Gets the main directory to be read
        /// Calls for log file to be saved locally
        /// </summary>
        public async Task ExecuteAsync()
        {
            ConsoleWrite("Process started");
            string? directoryPath = ConsoleRead("Enter the directory path:");

            if (Directory.Exists(directoryPath))
            {
                ProcessDirectory(directoryPath);
                _logHandler.FileBuilder("Processing completed.");
            }
            else
            {
                _logHandler.FileBuilder("Directory not found.");
            }

            await _logHandler.SaveFileLocallyAsync(_processName);
            ConsoleWrite("Process finished");
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
                    
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    
                    File.WriteAllText(path, jsonNode.ToJsonString(options)); 
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

                            jsonObject[property.Key] = _replaceOption switch
                            {
                                EReplaceOptions.Secret => (JsonNode)"<SECRET>",
                                EReplaceOptions.Hash => (JsonNode)ComputeHash(originalValue),
                                EReplaceOptions.Whitespace => (JsonNode)string.Empty,
                                _ => throw new Exception("No valid replacement option selected on app configuration."),
                            };
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
        private static bool IsSensitiveKey(string key)
        {
            var stringComparison = StringComparison.OrdinalIgnoreCase;
            return key.Contains("login", stringComparison)
                || key.Contains("user", stringComparison)
                || key.Contains("email", stringComparison)
                || key.Contains("password", stringComparison);
        }

        /// <summary>
        /// Computes SHA-256 hash
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string ComputeHash(string data)
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
