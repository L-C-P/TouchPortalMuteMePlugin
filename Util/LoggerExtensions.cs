using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace TPMuteMe.Util
{
    public static class LoggerExtensions
    {
        public static void LogObjectAsJson(this ILogger logger, Object message, [CallerMemberName] String memberName = "")
        {
            if (message == null)
            {
                return;
            }

            String json = JsonSerializer.Serialize(message);
            logger.LogInformation($"{memberName}: {json}");
        }
    }
}
