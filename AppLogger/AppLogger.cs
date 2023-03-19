using Common;
using Discord.WebSocket;
using Interfaces.Logger;
using Models;

namespace Logging
{
    public class AppLogger : IAppLogger
    {
        private readonly DiscordShardedClient _client;

        public AppLogger(DiscordShardedClient client) 
        {
            _client = client;
        }

        public void CommandUsedLog(string folder, string command, ulong channelId, ulong userId, ulong guildId)
        {
            string commandLogMessage = $"GuildId: {guildId} || ChannelId: {channelId} || UserId: {userId} || Command Used: {command}";
            FileLogger(folder, commandLogMessage);
        }

        public void ConsoleLogger(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.Now:hh:mm:ss.fff} : " + message);
            Console.ResetColor();
        }

        public Task DiscrodChannelLogger(string message, ulong guildId, ulong channelId)
        {
            _ = Task.Run(async () =>
            {
                var guild = _client.GetGuild(guildId);
                if(guild != null)
                {
                    var channel = guild.GetTextChannel(channelId);
                    if(channel != null) 
                    {
                        await channel.SendMessageAsync($"Message: {message}");
                    }
                }
            });
            return Task.CompletedTask;
        }

        public void ExceptionLog(string folder, Exception exception)
        {
            var filePath = $"{Constants.Logfolder}/Exception/{folder}/{DateTime.Now:MMMM, yyyy}";
            FileUtils.CreateDirectoryIfNotExists(filePath);

            filePath += $"/{DateTime.Now:dddd, MMMM d, yyyy}.txt";
            using (var file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                using (var sw = new StreamWriter(file))
                {
                    string fileContent = LogMessageBuilder.ExceptionMessageBuilder(exception);
                    sw.WriteLine(fileContent);
                }
            }
        }

        public void FileLogger(string folder, string message)
        {
            var filePath = $"{Constants.Logfolder}/{folder}/{DateTime.Now:MMMM, yyyy}";
            FileUtils.CreateDirectoryIfNotExists(filePath);

            filePath += $"/{DateTime.Now:dddd, MMMM d, yyyy}.txt";
            using (var file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                using (var sw = new StreamWriter(file))
                {
                    sw.WriteLine($"{DateTime.Now:T} : Message : {message}");
                    sw.WriteLine(Constants.EOFMarkup);
                }
            }
        }
    }
}