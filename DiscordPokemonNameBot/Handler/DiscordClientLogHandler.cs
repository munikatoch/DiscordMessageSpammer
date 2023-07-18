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

        public DiscordClientLogHandler(IAppLogger logger, DiscordShardedClient client, InteractionService interactionService)
        {
            _logger = logger;
            _client = client;
            _interactionService = interactionService;
        }

        public void Initialize()
        {
            _client.Log += LogEvent;
            _client.ShardReady += ShardReadyEvent;
            _client.ShardConnected += ShardConnectedEvent;
            _client.ShardDisconnected += ShardDisconnectedEvent;
            _client.ShardLatencyUpdated += ShardLatencyUpdatedEvent;
        }

        private async Task ShardLatencyUpdatedEvent(int oldPing, int updatedPing, DiscordSocketClient client)
        {
            if(updatedPing < 500) 
            {
                return;
            }
            string message = $"Shard latency updated from {oldPing}ms to {updatedPing}ms";
            await _logger.DiscrodChannelLogger(message, Constants.GuildId, Constants.BotLogsChannel);
        }

        private async Task ShardDisconnectedEvent(Exception exception, DiscordSocketClient client)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine($"ShardId: {client.ShardId} is Connected, Latency: {client.Latency}ms");
            await _logger.DiscrodChannelLogger(message.ToString(), Constants.GuildId, Constants.BotShardDisconnectedChannel).ConfigureAwait(false);
            await _logger.ExceptionLog("DiscordClientLogHandler", exception).ConfigureAwait(false);
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
            if(log.Exception != null) 
            {
                _logger.ExceptionLog("DiscordClientLogHandler", log.Exception).ConfigureAwait(false);
            }
            else
            {
                _logger.FileLogger(log).ConfigureAwait(false);
            }
            return Task.CompletedTask;
        }
    }
}
