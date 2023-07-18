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

        private async Task LogInteractionServiceEvent(LogMessage log)
        {
            await _logger.FileLogger(log).ConfigureAwait(false);
        }

        private async Task SlashCommandExecutedEvent(SocketSlashCommand command)
        {
            ShardedInteractionContext context = new ShardedInteractionContext(_client, command);
            IResult result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            if (!result.IsSuccess)
            {
                await _logger.FileLogger(result).ConfigureAwait(false);
                await command.RespondAsync(result.ErrorReason);
            }
        }
    }
}
