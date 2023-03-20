﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interfaces.Discord.Service;
using Interfaces.Logger;
using Models.Discord;
using Models.Discord.Common;

namespace DiscordPokemonNameBot.Module
{
    public class MessageSpamPrefixCommandModule : ModuleBase<ShardedCommandContext>
    {
        private readonly Random _random;
        private readonly IAppLogger _logger;
        private readonly MessageSpam _message;
        private readonly IDiscordService _discordService;
        private readonly IPokemonService _pokemonService;
        private readonly string _folderName = "PrefixCommand/Successful";

        public MessageSpamPrefixCommandModule(Random random, IAppLogger appLogger, MessageSpam message, IDiscordService discordService, IPokemonService pokemonService)
        {
            _random = random;
            _logger = appLogger;
            _message = message;
            _discordService = discordService;
            _pokemonService = pokemonService;
        }

        [Command("hello")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Ping()
        {
            await Context.Message.ReplyAsync("Hello " + Context.User.Mention + ". I am a bot!");
            _logger.CommandUsedLog(_folderName, "hello", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [Command("delete")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteMessages(int count)
        {
            _logger.CommandUsedLog(_folderName, "delete", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            if (count < 0)
            {
                await Context.Message.ReplyAsync("How can I delete negative number of messages. Please teach me sensei");
                return;
            }
            else if (count == 0)
            {
                await Context.Message.ReplyAsync("Wow! We actually successfully deleted 0 message");
                return;
            }
            else if (count > 99)
            {
                await Context.Message.ReplyAsync("Deleting top 99 messages as max is 99 only for this command. If you want to delete all message then try deleteall");
            }
            else
            {
                await Context.Message.ReplyAsync("Message getting deleted");
            }
            _ = Task.Run(async () =>
            {
                if (Context?.Channel != null && Context.Channel is ITextChannel textChannel)
                {
                    await _discordService.DeleteMessage(textChannel, count);
                }
            });
        }

        [Command("startspam")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task StartMessageSpam(SocketChannel channel, int duration = 0)
        {
            _logger.CommandUsedLog(_folderName, "startspam", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            if (duration < 0)
            {
                await Context.Message.ReplyAsync("What is this sorcery? You have to teach me sensie how to use negative duration");
                return;
            }
            else if (duration == 0)
            {
                await Context.Message.ReplyAsync("Message will spam at a range of 5s to 15s per message as duration was default or 0");
            }
            else if (duration < 5)
            {
                await Context.Message.ReplyAsync("Message will spam at 5s per message as this is the minimum");
                duration = 5;
            }
            else
            {
                await Context.Message.ReplyAsync("Message spam start");
            }
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
                await Context.Message.ReplyAsync("Message spam updated");
            }
        }

        [Command("stopspam")]
        public async Task StopMessageSpam()
        {
            _message.IsSpamMessageEnabled = false;
            await Context.Message.ReplyAsync("Message spam stopped");
            _logger.CommandUsedLog(_folderName, "stopspam", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [Command("deleteall")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteAllMessagesFromChannel()
        {
            _logger.CommandUsedLog(_folderName, "deleteall", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            await Context.Message.ReplyAsync("Message getting deleted");
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

        [Command("detectpokemon")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task DetectPokemon(string url)
        {
            _logger.CommandUsedLog(_folderName, "detectpokemon", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            PokemonPrediction predictedPokemon = await _pokemonService.PredictPokemon(url);
            await Context.Message.ReplyAsync("", false, predictedPokemon.PokemonEmbed);
        }
    }
}
