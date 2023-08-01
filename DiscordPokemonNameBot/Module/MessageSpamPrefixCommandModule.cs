using Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interfaces.Discord.Service;
using Interfaces.Logger;
using Models;
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

        public MessageSpamPrefixCommandModule(Random random, IAppLogger appLogger, MessageSpam message, IDiscordService discordService, IPokemonService pokemonService)
        {
            _random = random;
            _logger = appLogger;
            _message = message;
            _discordService = discordService;
            _pokemonService = pokemonService;
        }

        [Command("version")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task BotVersionn()
        {
            await Context.Message.ReplyAsync("Bot version: " + Constants.BotVersion);
            _logger.CommandUsedLog("MessageSpamPrefixCommandModule", "version", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [Command("hello")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Ping()
        {
            await Context.Message.ReplyAsync("Hello " + Context.User.Mention + ". I am a bot and current bot latency is " + Context.Client.Latency + " ms");
            _logger.CommandUsedLog("MessageSpamPrefixCommandModule", "hello", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [Command("delete")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteMessages(int count)
        {
            _logger.CommandUsedLog("MessageSpamPrefixCommandModule", "delete", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

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
            _logger.CommandUsedLog("MessageSpamPrefixCommandModule", "startspam", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            if (duration < 0)
            {
                await Context.Message.ReplyAsync("What is this sorcery? You have to teach me sensie how to use negative duration");
                return;
            }
            else if (duration == 0)
            {
                duration = _random.Next(3, 15);
                await Context.Message.ReplyAsync($"Message will spam at {duration}s per message in channel <#{channel.Id}> as duration was default or 0");
            }
            else if (duration < 3)
            {
                await Context.Message.ReplyAsync($"Message will spam at 3s per message as this is the minimum in channel <#{channel.Id}>");
                duration = 3;
            }
            if (!_message.SpamDetail.ContainsKey(Context.Guild.Id))
            {
                _message.SpamDetail.TryAdd(Context.Guild.Id, new SpamDetail()
                {
                    DiscordChannelId = channel.Id,
                    DurationInSeconds = TimeSpan.FromSeconds(duration),
                    IsSpamMessageEnabled = false
                });
            }
            else
            {
                var oldValue = _message.SpamDetail[Context.Guild.Id];
                var newValue = new SpamDetail()
                {
                    DiscordChannelId = channel.Id,
                    DurationInSeconds = TimeSpan.FromSeconds(duration),
                    IsSpamMessageEnabled = oldValue.IsSpamMessageEnabled
                };
                _message.SpamDetail.TryUpdate(Context.Guild.Id, newValue, oldValue);
            }
            if (!_message.SpamDetail[Context.Guild.Id].IsSpamMessageEnabled)
            {
                await Context.Message.ReplyAsync($"Message spam at {duration}s per message in channel <#{channel.Id}>");
                var oldValue = _message.SpamDetail[Context.Guild.Id];
                var newValue = new SpamDetail()
                {
                    DiscordChannelId = channel.Id,
                    DurationInSeconds = TimeSpan.FromSeconds(duration),
                    IsSpamMessageEnabled = true
                };
                _message.SpamDetail.TryUpdate(Context.Guild.Id, newValue, oldValue);
                _ = Task.Run(async () =>
                {
                    while (_message.SpamDetail[Context.Guild.Id].IsSpamMessageEnabled)
                    {
                        await Task.Delay(_message.SpamDetail[Context.Guild.Id].DurationInSeconds);
                        await _discordService.CreateAndSendSpamMessage(_message.SpamDetail[Context.Guild.Id].DiscordChannelId);
                    }
                });
            }
            else
            {
                await Context.Message.ReplyAsync($"Message spam updated to {duration}s per message in channel <#{channel.Id}>");
            }
        }

        [Command("stopspam")]
        public async Task StopMessageSpam()
        {
            var oldValue = _message.SpamDetail[Context.Guild.Id];
            var newValue = new SpamDetail()
            {
                DiscordChannelId = oldValue.DiscordChannelId,
                DurationInSeconds = oldValue.DurationInSeconds,
                IsSpamMessageEnabled = false
            };
            _message.SpamDetail.TryUpdate(Context.Guild.Id, newValue, oldValue);
            await Context.Message.ReplyAsync("Message spam stopped");
            _logger.CommandUsedLog("MessageSpamPrefixCommandModule", "stopspam", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [Command("deleteall")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteAllMessagesFromChannel()
        {
            _logger.CommandUsedLog("MessageSpamPrefixCommandModule", "deleteall", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

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

        [Command("getlogs")]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task GetDiscordBotLogs()
        {
            _logger.CommandUsedLog("MessageSpamPrefixCommandModule", "getlogs", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            await Context.Message.ReplyAsync("Here are the logs");
            await Task.Run(async () =>
            {
                try
                {
                    FileUtils.CreateDirectoryIfNotExists(Constants.LogZipfolder);
                    FileUtils.CreateZipFileSafely();
                    await Context.Channel.SendFileAsync(Constants.LogZipfile);
                }
                catch(Exception e)
                {
                    await _logger.DiscrodChannelLogger(e.Message, Constants.GuildId, Constants.BotLogsChannel);
                }
                finally
                {
                    FileUtils.DeleteAllFiles(Constants.LogZipfolder);
                }
            }).ConfigureAwait(false);
        }
    }
}
