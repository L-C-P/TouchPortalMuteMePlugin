using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace TPMuteMe.Util
{
    /// <summary>
    /// Extensions for logger.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Convert object to string and log the serialized content.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">Object to serialize.</param>
        /// <param name="memberName">The member name ob the caller.</param>
        public static void LogObjectAsJson(this ILogger logger, Object message, [CallerMemberName] String memberName = "")
        {
            String json = JsonSerializer.Serialize(message);
            logger.LogInformation($"{memberName}: {json}");
        }
    }
}
