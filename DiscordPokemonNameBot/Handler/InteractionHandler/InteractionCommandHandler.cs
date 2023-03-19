using Common;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordPokemonNameBot.Module;
using Interfaces.Discord.Handler.InteractionHandler;
using Interfaces.Logger;
using Models;
using System.Text;

namespace DiscordPokemonNameBot.Handler.InteractionHandler
{
    public class InteractionCommandHandler : IInteractionHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppLogger _logger;

        public InteractionCommandHandler(DiscordShardedClient client, InteractionService interactionService, IServiceProvider serviceProvider, IAppLogger logger)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await _interactionService.AddModuleAsync<MessageSpamSlashCommandModule>(_serviceProvider);

            _interactionService.Log += LogInteractionServiceEvent;
            _client.SlashCommandExecuted += SlashCommandExecutedEvent;
        }

        private Task LogInteractionServiceEvent(LogMessage log)
        {
            string message = LogMessageBuilder.DiscordLogMessage(log);
            _logger.FileLogger("InteractionService", message);
            return Task.CompletedTask;
        }

        private async Task SlashCommandExecutedEvent(SocketSlashCommand command)
        {
            ShardedInteractionContext context = new ShardedInteractionContext(_client, command);
            IResult result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            if (!result.IsSuccess)
            {
                StringBuilder sb = new StringBuilder();
                if (result.Error.HasValue)
                {
                    sb.AppendLine($"Error: {result.Error}");
                }
                sb.AppendLine(result.ErrorReason);
                sb.AppendLine(Constants.EOFMarkup);
                _logger.FileLogger("InteractionCommand/Unsuccessful", sb.ToString());
                await command.RespondAsync(result.ErrorReason);
            }
        }
    }
}
