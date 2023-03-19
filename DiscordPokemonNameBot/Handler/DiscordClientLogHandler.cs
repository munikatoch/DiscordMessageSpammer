using Common;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordPokemonNameBot.Configuration;
using Interfaces;
using Interfaces.Discord.Handler;
using Interfaces.Logger;
using Models;
using System.Text;

namespace DiscordPokemonNameBot.Handler
{
    public class DiscordClientLogHandler : IDiscordClientLogHandler
    {
        private readonly IAppLogger _logger;
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;
        private readonly IAppConfiguration _appConfiguration;

        public DiscordClientLogHandler(IAppLogger logger, DiscordShardedClient client, InteractionService interactionService, IAppConfiguration appConfiguration)
        {
            _logger = logger;
            _client = client;
            _interactionService = interactionService;
            _appConfiguration = appConfiguration;
        }

        public void Initialize()
        {
            _client.Log += LogEvent;
            _client.ShardReady += ShardReadyEvent;
            _client.ShardConnected += ShardConnectedEvent;
            _client.ShardDisconnected += ShardDisconnectedEvent;
            _client.ShardLatencyUpdated += ShardLatencyUpdatedEvent;
            _client.JoinedGuild += JoinedGuildEvent;
        }

        private async Task JoinedGuildEvent(SocketGuild guild)
        {
            string message = $"New server joined ID: {guild.Id}, Name: {guild.Name}, Total Members: {guild.MemberCount}";
            await _logger.DiscrodChannelLogger(message, Constants.GuildId, Constants.BotGuildJoinChannel);
        }

        private async Task ShardLatencyUpdatedEvent(int oldPing, int updatedPing, DiscordSocketClient client)
        {
            string message = $"Shard latency updated from {oldPing}ms to {updatedPing}ms";
            await _logger.DiscrodChannelLogger(message, Constants.GuildId, Constants.BotLatencyChannel);
        }

        private async Task ShardDisconnectedEvent(Exception exception, DiscordSocketClient client)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine($"ShardId: {client.ShardId} is Connected, Latency: {client.Latency}ms");
            message.AppendLine(LogMessageBuilder.ExceptionMessageBuilder(exception));
            await _logger.DiscrodChannelLogger(message.ToString(), Constants.GuildId, Constants.BotShardDisconnectedChannel);
        }

        private async Task ShardConnectedEvent(DiscordSocketClient client)
        {
            string message = $"ShardId: {client.ShardId} is Connected, Latency: {client.Latency}ms";
            await _logger.DiscrodChannelLogger(message, Constants.GuildId, Constants.BotShardConnectedChannel);
        }

        private async Task ShardReadyEvent(DiscordSocketClient client)
        {
#if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync(Constants.GuildId);
#else
            await _interactionService.RegisterCommandsGloballyAsync();
#endif
            string message = $"ShardId: {client.ShardId} is Ready, Latency: {client.Latency}ms";
            _logger.ConsoleLogger(message, ConsoleColor.Green);
        }

        private Task LogEvent(LogMessage log)
        {
            string message = LogMessageBuilder.DiscordLogMessage(log);
            _logger.FileLogger("discord", message);
            return Task.CompletedTask;
        }
    }
}
