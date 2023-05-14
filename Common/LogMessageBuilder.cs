using Discord;
using System.Text;

namespace Common
{
    public class LogMessageBuilder
    {
        public static string DiscordLogMessage(LogMessage logMessage)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{DateTime.Now:T} : Log source: {logMessage.Source}");
            sb.AppendLine($"{DateTime.Now:T} : Log message: {logMessage.Message}");

            string? exceptionType = logMessage.Exception?.GetType().Name;
            if (!string.IsNullOrEmpty(exceptionType))
                sb.AppendLine($"{DateTime.Now:T} : Log excpetion type: {exceptionType}");

            if (logMessage.Exception != null)
            {
                sb.AppendLine(ExceptionMessageBuilder(logMessage.Exception));
            }
            return sb.ToString();
        }

        public static string TimeStatisticsMessageBuilder(string logMessageStart, long timeInMilliseconds, long timeInTicks)
        {
            return $"{logMessageStart} {timeInMilliseconds} ms | {timeInTicks} ticks";
        }

        public static string ExceptionMessageBuilder(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now:T} : Exception Message : {exception.Message}");
            sb.AppendLine($"{DateTime.Now:T} : Inner Exception : {exception.InnerException}");
            sb.AppendLine($"{DateTime.Now:T} : Exception StackTrace : {exception.StackTrace}");
            return sb.ToString();
        }

        public static async Task<string> CreateHttpUnsuccessLogMessage(HttpResponseMessage response)
        {
            string httpResponse = await response.Content.ReadAsStringAsync();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Status Code: {response.StatusCode}");

            if (response.RequestMessage?.RequestUri?.AbsolutePath != null)
            {
                sb.AppendLine($"Url: {response.RequestMessage?.RequestUri?.AbsolutePath}");
            }
            sb.AppendLine($"Response: {httpResponse}");
            return sb.ToString();
        }
    }
}