using Common;
using Discord.WebSocket;
using Interfaces.Logger;
using Models;
using Newtonsoft.Json;
using Serilog;

namespace Logging
{
    public class AppLogger : IAppLogger
    {
        private readonly DiscordShardedClient _client;
        private readonly ILogger _logger;

        public AppLogger(DiscordShardedClient client, ILogger logger) 
        {
            _client = client;
            _logger = logger;
        }

        public void CommandUsedLog(string source, string command, ulong channelId, ulong userId, ulong guildId)
        {
            FileLogger(new { 
                Source =  source,
                CommandUsed = command,
                DiscordGuildId = guildId,
                DiscordChannelId = channelId,
                DiscordUserId = userId,
            });
        }

        public void ConsoleLogger(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss.fff} : " + message);
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

        public Task ExceptionLog(string source, Exception exception)
        {
            _logger.Error(exception, source);
            return Task.CompletedTask;
        }

        public Task FileLogger(object message)
        {
            string jsonLogMessage = JsonConvert.SerializeObject(message);
            _logger.Information(jsonLogMessage);
            return Task.CompletedTask;
        }
    }
}