using static CryptographCredentials.Framework.LogManagement.LogHandler;
using CryptographCredentials.Domain.Enums;
using CryptographCredentials.Framework.LogManagement.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CryptographCredentials.Service
{
    public class CryptographCredentialsService
    {
        #region | Fields |
        private readonly ILogHandler _logHandler;
        private readonly string _processName;
        private readonly string _fileNameToBeProcessed;
        #endregion

        #region | Constructor |
        public CryptographCredentialsService(IConfiguration configuration,
            ILogHandler logHandler)
        {
            _logHandler = logHandler;
            _processName = GetType().Name;
            _fileNameToBeProcessed = configuration.GetSection("FileNameToBeProcessed").Value ?? string.Empty;
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
            bool restartProgram;

            do
            {
                Console.Clear();
                bool startProgram = ConsoleRead("Start program? (y/n)").Equals("y", StringComparison.InvariantCultureIgnoreCase);

                if (startProgram)
                {
                    string? directoryPath = ConsoleRead("Enter the directory path:").Replace("\"", string.Empty);

                    if (Directory.Exists(directoryPath))
                    {
                        _logHandler.FileBuilder("Directory found.");
                        var cryptographyType = GetCryptographyType(ConsoleRead("Choose the cryptography type:\n[1] Secret\n[2] Hash\n[3]Whitespace"));
                        
                        if (cryptographyType != null)
                        {
                            ProcessDirectory(directoryPath, (ECryptographyType)cryptographyType);
                            _logHandler.FileBuilder("Processing completed.");
                        }
                        else
                        {
                            _logHandler.FileBuilder("Invalid cryptography type selected. Please restart the program when prompted.");
                        }
                    }
                    else
                    {
                        _logHandler.FileBuilder("Directory not found.");
                    }
                
                    await _logHandler.SaveFileLocallyAsync(_processName);
                }
                else
                {
                    Environment.Exit(0);
                }

                restartProgram = ConsoleRead("Restart program? (y/n)").Equals("y", StringComparison.InvariantCultureIgnoreCase);
            } while (restartProgram);
            
            ConsoleWrite("Program finished");
        }

        #region | Private methods |
        private ECryptographyType? GetCryptographyType(string value)
        {
            if (Enum.TryParse<ECryptographyType>(value, true, out var result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Calls only for appsettings.json files to be read
        /// </summary>
        /// <param name="targetDirectory"></param>
        private void ProcessDirectory(string targetDirectory, ECryptographyType cryptographyType)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory, _fileNameToBeProcessed, SearchOption.AllDirectories);

            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName, cryptographyType);
            }
        }

        /// <summary>
        /// Processes files
        /// </summary>
        /// <param name="path"></param>
        private void ProcessFile(string path, ECryptographyType cryptographyType)
        {
            try
            {
                string jsonContent = File.ReadAllText(path);
                JsonNode? jsonNode = JsonNode.Parse(jsonContent);

                if (jsonNode != null)
                {
                    UpdateJsonValues(jsonNode, cryptographyType);

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
        private void UpdateJsonValues(JsonNode node, ECryptographyType cryptographyType)
        {
            if (node is JsonArray jsonArray)
            {
                // Recursively calls this function if value is an array
                foreach (var item in jsonArray)
                {
                    UpdateJsonValues(item!, cryptographyType);
                }
            }
            else if (node is JsonObject jsonObject)
            {
                foreach (var property in jsonObject)
                {
                    if (property.Value is JsonObject || property.Value is JsonArray)
                    {
                        // Recursively calls this function if value is an object or array
                        UpdateJsonValues(property.Value!, cryptographyType);
                    }
                    else if (property.Value is JsonValue value)
                    {
                        // Checks if property is a credential
                        if (IsSensitiveKey(property.Key))
                        {
                            string originalValue = value.ToString();

                            jsonObject[property.Key] = cryptographyType switch
                            {
                                ECryptographyType.Secret => (JsonNode)"<SECRET>",
                                ECryptographyType.Hash => (JsonNode)ComputeHash(originalValue),
                                ECryptographyType.Whitespace => (JsonNode)string.Empty,
                                _ => throw new Exception("No valid cryptography type selected."),
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
