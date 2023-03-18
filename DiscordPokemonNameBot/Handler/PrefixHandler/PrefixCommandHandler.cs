﻿using Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordPokemonNameBot.Module;
using Interfaces.Discord.Handler.PrefixHandler;
using Interfaces.Discord.Service;
using Interfaces.Logger;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tensorflow.Contexts;

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
            if (message != null)
            {
                if (message.Author.IsBot)
                {
                    if (_prefixService.ValidatePokemonSpanMessage(message, out Embed? embed) && embed != null && embed.Image.HasValue)
                    {
                        Embed pokemonEmbed = await _pokemonService.PredictPokemon(embed.Image.Value.Url);
                        await message.ReplyAsync("", false, pokemonEmbed);
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
                            StringBuilder sb = new StringBuilder();
                            if (result.Error.HasValue)
                            {
                                sb.AppendLine($"Error: {result.Error}");
                            }
                            sb.AppendLine(result.ErrorReason);
                            sb.AppendLine(Constants.EOFMarkup);
                            _logger.FileLogger("PrefixCommand/Unsuccessful", sb.ToString());
                            await message.ReplyAsync(result.ErrorReason);
                        }
                    }
                }
            }

        }

        private Task LogCommandServiceEvent(LogMessage log)
        {
            string message = LogMessageBuilder.DiscordLogMessage(log);
            _logger.FileLogger("CommandService", message);
            return Task.CompletedTask;
        }
    }
}