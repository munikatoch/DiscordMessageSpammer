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

            if(logMessage.Exception != null)
            {
                sb.AppendLine(ExceptionMessageBuilder(logMessage.Exception));
            }
            return sb.ToString();
        }

        public static string ExceptionMessageBuilder(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now:T} : Exception Message : {exception.Message}");
            sb.AppendLine($"{DateTime.Now:T} : Inner Exception : {exception.InnerException}");
            sb.AppendLine($"{DateTime.Now:T} : Exception StackTrace : {exception.StackTrace}");
            return sb.ToString();
        }
    }
}