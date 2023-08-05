using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordPokemonNameBot.Module;
using Interfaces.Discord.Handler.PrefixHandler;
using Interfaces.Discord.Service;
using Interfaces.Logger;
using Models.DAO;
using Models.Discord;

namespace DiscordPokemonNameBot.Handler.PrefixHandler
{
    public class PrefixCommandHandler : IPrefixHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private readonly IAppLogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPrefixService _prefixService;
        private readonly IPokemonService _pokemonService;

        public PrefixCommandHandler(DiscordShardedClient client, CommandService commandService, IAppLogger logger, IServiceProvider serviceProvider, IPrefixService prefixService, IPokemonService pokemonService)
        {
            _client = client;
            _commandService = commandService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _prefixService = prefixService;
            _pokemonService = pokemonService;
        }

        public async Task InitializeAsync()
        {
            await _commandService.AddModuleAsync<MessageSpamPrefixCommandModule>(_serviceProvider);

            _commandService.Log += LogCommandServiceEvent;
            _client.MessageReceived += MessageReceivedEvent;
        }

        private Task MessageReceivedEvent(SocketMessage socketMessage)
        {
            if (socketMessage is SocketUserMessage message)
            {
                _ = Task.Run(async () =>
                {
                    await HandleCommandAsync(message);
                });
            }
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketUserMessage message)
        {
            try
            {
                if (message != null)
                {
                    if (message.Author.IsBot)
                    {
                        if (_prefixService.ValidatePokemonSpanMessage(message, out Embed? embed) && embed != null && embed.Image.HasValue)
                        {
                            if (message.Channel is SocketGuildChannel)
                            {
                                SocketGuildChannel? channel = message.Channel as SocketGuildChannel;
                                await _pokemonService.PredictPokemon(embed.Image.Value.Url, channel?.Guild.Id, message.Channel.Id);
                            }
                        }
                    }
                    else
                    {
                        int argPos = 0;
                        if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                        {
                            ShardedCommandContext context = new ShardedCommandContext(_client, message);
                            IResult result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
                            if (!result.IsSuccess)
                            {
                                await _logger.FileLogger(result).ConfigureAwait(false);
                                if (!result.ErrorReason.Equals("Unknown command."))
                                {
                                    await message.ReplyAsync(result.ErrorReason);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLog("PrefixCommandHandler.HandleCommandAsync", ex).ConfigureAwait(false);
            }
        }

        private async Task LogCommandServiceEvent(LogMessage log)
        {
            await _logger.FileLogger(log).ConfigureAwait(false);
        }
    }
}
