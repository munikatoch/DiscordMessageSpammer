using Discord;
using Discord.Interactions;
using Interfaces.Discord.Service;
using Interfaces.Logger;
using Models.Discord.Common;
using Discord.WebSocket;
using Models;
using Common;
using System.Text;

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

        [SlashCommand("version", "Current Bot version")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        public async Task BotVersionn()
        {
            await RespondAsync("Bot version: " + Constants.BotVersion);
            _logger.CommandUsedLog("MessageSpamSlashCommandModule", "version", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [SlashCommand("hello", "Basic bot ping like Hello World")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Ping()
        {
            await RespondAsync("Hello " + Context.User.Mention + ". I am a bot and current bot latency is " + Context.Client.Latency + " ms");
            _logger.CommandUsedLog("MessageSpamSlashCommandModule", "hello", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [SlashCommand("delete", "Delete specified number of messages")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteMessages([Summary(description: "Downloads and removes n messages from the current channel with max 99.")] int count)
        {
            _logger.CommandUsedLog("MessageSpamSlashCommandModule", "delete", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

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
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        public async Task StartMessageSpam(
            [Summary(description: "Channel for message spamming")] SocketChannel channel,
            [Summary(description: "Duration in seconds after which message should spam minimum is 3s")] int duration = 0
            )
        {
            _logger.CommandUsedLog("MessageSpamPrefixCommandModule", "startspam", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            if (duration < 0)
            {
                await RespondAsync("What is this sorcery? You have to teach me sensie how to use negative duration");
                return;
            }
            else if (duration == 0)
            {
                duration = _random.Next(3, 15);
                await RespondAsync($"Message will spam at {duration}s per message in channel <#{channel.Id}> as duration was default or 0");
            }
            else if (duration < 3)
            {
                await RespondAsync($"Message will spam at 3s per message as this is the minimum in channel <#{channel.Id}>");
                duration = 3;
            }
            if (!_message.SpamDetail.ContainsKey(Context.Guild.Id))
            {
                InitializeSpamMessage(Context.Guild.Id, channel.Id, duration);
            }
            if (!_message.SpamDetail[Context.Guild.Id].IsSpamMessageEnabled)
            {
                await RespondAsync($"Message spam at {duration}s per message in channel <#{channel.Id}>");
                var oldValue = _message.SpamDetail[Context.Guild.Id];
                var newValue = new SpamDetail()
                {
                    DiscordChannelId = channel.Id,
                    DurationInSeconds = TimeSpan.FromSeconds(duration),
                    IsSpamMessageEnabled = true,
                    CurrentIndex = 0,
                    PokemonSpawnChannel = oldValue.PokemonSpawnChannel
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
                await RespondAsync($"Message spam updated to {duration}s per message in channel <#{channel.Id}>");
            }
        }

        [SlashCommand("setchannel", "Set pokemon redirect channels")]
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        public async Task SetPokemonSpanChannels(string spawnChannels)
        {
            List<ulong> channelIds = new List<ulong>();

            string[] channels = spawnChannels.Split(' ');

            foreach (string tempChannel in channels)
            {
                string channel = tempChannel.Trim().TrimEnd(',');
                MentionUtils.TryParseChannel(channel, out ulong channelId);
                channelIds.Add(channelId);
            }
            await RespondAsync("Pokemon spawn channels set");

            InitializeSpamMessage(Context.Guild.Id, 0, 0);

            SpamDetail oldValue = _message.SpamDetail[Context.Guild.Id];

            _message.SpamDetail.TryUpdate(Context.Guild.Id, new SpamDetail()
            {
                DurationInSeconds = oldValue.DurationInSeconds,
                IsSpamMessageEnabled = oldValue.IsSpamMessageEnabled,
                PokemonSpawnChannel = channelIds,
                CurrentIndex = oldValue.CurrentIndex,
            }, oldValue);
        }

        [SlashCommand("getchannel", "Set pokemon redirect channels")]
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        public async Task GetPokemonSpanChannels()
        {
            StringBuilder sb = new StringBuilder();

            if (_message.SpamDetail.ContainsKey(Context.Guild.Id))
            {
                SpamDetail oldValue = _message.SpamDetail[Context.Guild.Id];

                if (oldValue.PokemonSpawnChannel.Count > 0)
                {
                    foreach (var channel in oldValue.PokemonSpawnChannel)
                    {
                        sb.Append($"{MentionUtils.MentionChannel(channel)} ");
                    }
                }
            }
            else
            {
                sb.Append("No Channel Found!");
            }
            await RespondAsync("Pokemon spawn channels:" + sb.ToString());
        }

        [SlashCommand("stopspam", "Stop Message Spamming")]
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        public async Task StopMessageSpam()
        {
            var oldValue = _message.SpamDetail[Context.Guild.Id];
            var newValue = new SpamDetail()
            {
                DiscordChannelId = oldValue.DiscordChannelId,
                DurationInSeconds = oldValue.DurationInSeconds,
                IsSpamMessageEnabled = false,
                CurrentIndex = oldValue.CurrentIndex,
                PokemonSpawnChannel = oldValue.PokemonSpawnChannel,
            };
            _message.SpamDetail.TryUpdate(Context.Guild.Id, newValue, oldValue);
            await RespondAsync("Message spam stopped");
            _logger.CommandUsedLog("MessageSpamSlashCommandModule", "stopspam", Context.Channel.Id, Context.User.Id, Context.Guild.Id);
        }

        [SlashCommand("deleteall", "Delete all messages from the channel")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteAllMessagesFromChannel()
        {
            _logger.CommandUsedLog("MessageSpamSlashCommandModule", "deleteall", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

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

        [SlashCommand("getlogs", "Get log files created")]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        public async Task GetDiscordBotLogs()
        {
            _logger.CommandUsedLog("MessageSpamSlashCommandModule", "getlogs", Context.Channel.Id, Context.User.Id, Context.Guild.Id);

            await RespondAsync("Here are the logs");
            await Task.Run(async () =>
            {
                try
                {
                    FileUtils.CreateDirectoryIfNotExists(Constants.LogZipfolder);
                    FileUtils.CreateZipFileSafely();
                    await Context.Channel.SendFileAsync(Constants.LogZipfile);
                }
                catch (Exception e)
                {
                    await _logger.DiscrodChannelLogger(e.Message, Constants.GuildId, Constants.BotLogsChannel);
                }
                finally
                {
                    FileUtils.DeleteAllFiles(Constants.LogZipfolder);
                }

            }).ConfigureAwait(false);
        }

        private void InitializeSpamMessage(ulong id, ulong channelId, int duration)
        {
            _message.SpamDetail.TryAdd(Context.Guild.Id, new SpamDetail()
            {
                DiscordChannelId = channelId,
                DurationInSeconds = TimeSpan.FromSeconds(duration),
                IsSpamMessageEnabled = false,
                CurrentIndex = 0,
                PokemonSpawnChannel = new List<ulong>(),
            });
        }
    }
}
