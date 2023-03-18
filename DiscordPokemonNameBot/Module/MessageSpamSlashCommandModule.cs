﻿using Discord;
using Discord.Interactions;
using Interfaces.Discord.Service;
using Interfaces.Logger;
using Models.Discord.Common;
using Discord.WebSocket;

namespace DiscordPokemonNameBot.Module
{
    public class MessageSpamSlashCommandModule : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly Random _random;
        private readonly IAppLogger _logger;
        private readonly MessageSpam _message;
        private readonly IDiscordService _discordService;
        private readonly IPokemonService _pokemonService;

        public MessageSpamSlashCommandModule(Random random, IAppLogger appLogger, MessageSpam message, IDiscordService discordService, IPokemonService pokemonService)
        {
            _random = random;
            _logger = appLogger;
            _message = message;
            _discordService = discordService;
            _pokemonService = pokemonService;
        }

        [SlashCommand("hello", "Basic bot ping like Hello World")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Ping()
        {
            await RespondAsync("Hello " + Context.User.Mention + ". I am a bot!");
            _logger.CommandUsedLog("hello", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [SlashCommand("delete", "Delete specified number of messages")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteMessages([Summary(description: "Downloads and removes n messages from the current channel with max 99.")] int count)
        {
            _logger.CommandUsedLog("delete", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            if (count < 0)
            {
                await RespondAsync("How can I delete negative number of messages. Please teach me sensei");
                return;
            }
            else if (count == 0)
            {
                await RespondAsync("Wow! We actually successfully deleted 0 message");
                return;
            }
            else if (count > 99)
            {
                await RespondAsync("Deleting top 99 messages as max is 99 only for this command. If you want to delete all message then try deleteall");
            }
            else
            {
                await RespondAsync("Message getting deleted");
            }
            _ = Task.Run(async () =>
            {
                if (Context?.Channel != null && Context.Channel is ITextChannel textChannel)
                {
                    await _discordService.DeleteMessage(textChannel, count);
                }
            });
        }

        [SlashCommand("startspam", "Start Message Spamming")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task StartMessageSpam(
            [Summary(description: "Channel for message spamming")] SocketChannel channel,
            [Summary(description: "Duration in seconds after which message should spam minimum is 5s")] int duration = 0
            )
        {
            _logger.CommandUsedLog("startspam", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
            
            if (duration < 0)
            {
                await RespondAsync("What is this sorcery? You have to teach me sensie how to use negative duration");
                return;
            }
            else if(duration < 5)
            {
                await RespondAsync("Message will spam at 5s per message as this is the minimum");
                duration = 5;
            }
            else
            {
                await RespondAsync("Message spam start");
            }
            _message.IsGenerateRandomDurationEnabled = duration == 0;
            _message.DurationInSeconds = TimeSpan.FromSeconds(duration);
            _message.IsGenerateRandomDurationEnabled = duration == 0;
            _message.DurationInSeconds = TimeSpan.FromSeconds(duration);
            if (!_message.IsSpamMessageEnabled)
            {
                _message.IsSpamMessageEnabled = true;
                _ = Task.Run(async () =>
                {
                    while (_message.IsSpamMessageEnabled)
                    {
                        if (_message.IsGenerateRandomDurationEnabled)
                        {
                            duration = _random.Next(5, 15);
                            _message.DurationInSeconds = TimeSpan.FromSeconds(duration);
                        }
                        await Task.Delay(_message.DurationInSeconds);
                        await _discordService.CreateAndSendSpamMessage(channel.Id);
                    }
                });
            }
            else
            {
                await RespondAsync("Message spam updated");
            }
        }

        [SlashCommand("stopspam", "Stop Message Spamming")]
        public async Task StopMessageSpam()
        {
            _message.IsSpamMessageEnabled = false;
            await RespondAsync("Message spam stopped");
            _logger.CommandUsedLog("stopspam", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [SlashCommand("deleteall", "Delete all messages from the channel")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteAllMessagesFromChannel()
        {
            _logger.CommandUsedLog("deleteall", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            await RespondAsync("Message getting deleted");
            _ = Task.Run(async () =>
            {
                if (Context?.Channel != null && Context.Channel is ITextChannel textChannel)
                {
                    int messageCount = await _discordService.DeleteMessage(textChannel);
                    while (messageCount == 100)
                    {
                        messageCount = await _discordService.DeleteMessage(textChannel);
                    }
                }
            });
        }

        [SlashCommand("detectpokemon", "Add url to detect the pokemon")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task DetectPokemon(string url)
        {
            _logger.CommandUsedLog("detectpokemon", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            Embed pokemonEmbed = await _pokemonService.PredictPokemon(url);
            await RespondAsync("", new[] { pokemonEmbed }, false);
        }
    }
}