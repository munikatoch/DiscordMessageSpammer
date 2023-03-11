using Discord;
using Discord.Interactions;
using DiscordPokemonNameBot.Helper;
using DiscordPokemonNameBot.Model;
using Microsoft.ML;
using PokemonPredictor.Models;
using PokemonPredictor;
using System.Text;
using Microsoft.Extensions.ML;

namespace DiscordPokemonNameBot.Module
{
    public class MessageSpamSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private PublicApi _api;
        private Random _random;
        private SpamMessage _message;
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEngine;
        private readonly HttpHelper _httpHelper;
        private readonly Predictor _predictor;

        public MessageSpamSlashCommandModule(
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

        [SlashCommand("hello", "Basic bot ping like Hello World")]
        public async Task Ping()
        {
            await RespondAsync("Hello " + this.Context.User.Mention + ". I am a bot!");
        }

        [SlashCommand("delete", "Delete specified number of messages")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteMessages([Summary(description: "Downloads and removes n messages from the current channel with max 100.")] int count)
        {
            if (count < 0)
            {
                await RespondAsync("How can I delete negative number of messages. Please teach me sensei");
            }
            else if (count == 0)
            {
                await RespondAsync("Wow! We actually successfully deleted 0 message");
            }
            else
            {
                await RespondAsync("Message getting deleted");
                await DeleteNMessage(count);
            }
        }

        [SlashCommand("startspam", "Start Message Spamming")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task StartMessageSpam(
            [Summary(description: "Duration in seconds after which message will spam minimum is 5s")] int duration = 0,
            [Summary(description: "Random Message Type")] SpamMessageType type = SpamMessageType.Default,
            [Summary(description: "Random Message count")] UInt16 count = 1
            )
        {
            if (duration < 0)
            {
                await RespondAsync("What is this sorcery? You have to teach me sensie how to use negative durations");
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
                await RespondAsync("Message spam start");
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
                await RespondAsync("Message spam updated");
            }
        }

        [SlashCommand("stopspam", "Stop Message Spamming")]
        public async Task StopMessageSpam()
        {
            _message.IsSpamMessageEnabled = false;
            await RespondAsync("Message spam stopped");
        }

        [SlashCommand("deleteall", "Delete all messages from the channel")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteAllMessagesFromChannel()
        {
            await RespondAsync("Message getting deleted");
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

        [SlashCommand("detectpokemon", "Add url to detect the pokemon")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task DetectPokemon(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !(url.EndsWith(".png") || url.EndsWith(".jpg")))
            {
                await RespondAsync("Invalid Url Provided");
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
                await RespondAsync(pokemon[0]);
            }
            else if (response.Content != null)
            {
                string errorMessage = Encoding.UTF8.GetString(response.Content);
                await RespondAsync(errorMessage);
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
            if(!string.IsNullOrEmpty(_message.Message))
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
