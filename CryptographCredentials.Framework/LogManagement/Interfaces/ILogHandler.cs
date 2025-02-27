using System.Text;

namespace CryptographCredentials.Framework.LogManagement.Interfaces
{
    public interface ILogHandler
    {
        StringBuilder FileBuilder(string message);
        Task SaveFileLocallyAsync(string processName);
    }
}
