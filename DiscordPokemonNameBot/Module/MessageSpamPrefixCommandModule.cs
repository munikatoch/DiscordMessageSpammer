using Discord;
using Discord.Commands;
using DiscordPokemonNameBot.Helper;
using DiscordPokemonNameBot.Model;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using PokemonPredictor;
using PokemonPredictor.Models;
using System.Text;

namespace DiscordPokemonNameBot.Module
{
    public class MessageSpamPrefixCommandModule : ModuleBase<SocketCommandContext>
    {
        private readonly PublicApi _api;
        private readonly Random _random;
        private readonly SpamMessage _message;
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEngine;
        private readonly HttpHelper _httpHelper;
        private readonly Predictor _predictor;

        public MessageSpamPrefixCommandModule(
            PublicApi api, 
            Random random, 
            SpamMessage message,
            PredictionEnginePool<ModelInput, ModelOutput> predictionEngine, 
            HttpHelper httpHelper,
            Predictor predictor
            )
        {
            _api = api;
            _random = random;
            _message = message;
            _predictionEngine = predictionEngine;
            _httpHelper = httpHelper;
            _predictor = predictor;
        }

        [Command("hello")]
        public async Task Ping()
        {
            await Context.Message.ReplyAsync("Hello " + this.Context.User.Mention + ". I am a bot!");
        }

        [Command("delete")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteMessages(int count)
        {
            if (count < 0)
            {
                await Context.Message.ReplyAsync("How can I delete negative number of messages. Please teach me sensei");
            }
            else if (count == 0)
            {
                await Context.Message.ReplyAsync("Wow! We actually successfully deleted 0 message");
            }
            else
            {
                await Context.Message.ReplyAsync("Message getting deleted");
                await DeleteNMessage(count);
            }
        }

        [Command("startspam")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task StartMessageSpam(int duration = 0, SpamMessageType type = SpamMessageType.Default, int count = 1
            )
        {
            if (duration < 0)
            {
                await Context.Message.ReplyAsync("What is this sorcery? You have to teach me sensie how to use negative durations");
                return;
            }
            if (duration != 0 && duration < 5)
            {
                duration = 5;
            }
            _message.IsGenerateRandomDurationEnabled = duration == 0;
            _message.DurationInSeconds = TimeSpan.FromSeconds(duration);
            if (!_message.IsSpamMessageEnabled)
            {
                await Context.Message.ReplyAsync("Message spam start");
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
                        await MessageSpammer(type, count);
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
        }

        [Command("deleteall")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteAllMessagesFromChannel()
        {
            await Context.Message.ReplyAsync("Message getting deleted");
            _ = Task.Run(async () =>
            {
                IEnumerable<IMessage> messages = await this.Context.Channel.GetMessagesAsync(100).FlattenAsync();
                while (messages.Count() > 0 && this.Context?.Channel != null)
                {
                    await (this.Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
                    messages = await this.Context.Channel.GetMessagesAsync(100).FlattenAsync();
                }
            });
        }

        [Command("detectpokemon")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task DetectPokemon(string url)
        {
            if(!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !(url.EndsWith(".png") || url.EndsWith(".jpg")))
            {
                await Context.Message.ReplyAsync("Invalid Url Provided");
            }
            UrlResponseByteContent response = await _httpHelper.GetUrlContent(url, HttpClientType.Discord.ToString());

            if (_httpHelper.IsSuccessStatusCode(response.HttpStatusCode))
            {
                string pokemonName = PredictPokemon(response.Content);
                if (string.IsNullOrEmpty(pokemonName))
                {
                    return;
                }

                string[] pokemon = pokemonName.Split(';');
                await Context.Message.ReplyAsync(pokemon[0]);
            }
            else if (response.Content != null)
            {
                string errorMessage = Encoding.UTF8.GetString(response.Content);
                await Context.Message.ReplyAsync(errorMessage);
            }
        }

        private async Task MessageSpammer(SpamMessageType type, int count)
        {
            switch (type)
            {
                case SpamMessageType.Paragraph:
                    _message.Message = await _api.GetRandomParagraphs(count);
                    break;
                case SpamMessageType.Beer:
                    _message.Message = await _api.GetRandomBeers(count);
                    break;
                case SpamMessageType.Default:

                    _message.Message = await _api.GetRandomParagraphs(count);
                    break;
            }
            if (!string.IsNullOrEmpty(_message.Message))
            {
                await _api.SendMessageToDiscord(_message.Message);
            }
        }

        private async Task DeleteNMessage(int count)
        {
            if (count > 99)
            {
                count = 99;
            }
            IEnumerable<IMessage> messages = await this.Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
            if (this.Context?.Channel != null)
            {
                await (this.Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
            }
        }

        private string PredictPokemon(byte[]? content)
        {
            string pokemonName = _predictor.PredictSingle(_predictionEngine.GetPredictionEngine(), content);
            return pokemonName;
        }
    }
}
