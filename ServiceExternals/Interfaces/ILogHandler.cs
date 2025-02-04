using System.Text;

namespace ServiceExternals.Interfaces
{
    public interface ILogHandler
    {
        StringBuilder FileBuilder(string message);
        Task SaveFileLocallyAsync(string processName, string fileContent);
    }
}
