using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordPokemonNameBot.Helper;
using DiscordPokemonNameBot.Model;
using DiscordPokemonNameBot.Module;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using PokemonPredictor;
using PokemonPredictor.Models;
using System.Text;

namespace DiscordPokemonNameBot.Handler
{
    public class PrefixHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly IServiceProvider _service;
        private readonly Predictor _predictor;
        private readonly HttpHelper _httpHelper;
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEngine;
        private readonly SpamMessage _spamMessage;

        public PrefixHandler(
            DiscordSocketClient client, 
            CommandService command, 
            IServiceProvider service, 
            Predictor predictor, 
            HttpHelper httpHelper,
            SpamMessage message,
            PredictionEnginePool<ModelInput, ModelOutput> predictionEngine)
        {
            _client = client;
            _command = command;
            _service = service;
            _predictor = predictor;
            _httpHelper = httpHelper;
            _spamMessage = message;
            _predictionEngine = predictionEngine;
        }

        public async Task InitializeAsync()
        {
            await _command.AddModuleAsync<MessageSpamPrefixCommandModule>(_service);
            _client.MessageReceived += HandlePrefixCommandAsync;
        }

        [RequireUserPermission(ChannelPermission.SendMessages)]
        private async Task HandlePrefixCommandAsync(SocketMessage socketMessage)
        {
            if (socketMessage == null || socketMessage.Author == null)
            {
                return;
            }
            SocketUserMessage message = socketMessage as SocketUserMessage;
            if (IsRarePokemonSpawn(socketMessage))
            {
                _spamMessage.IsSpamMessageEnabled = false;
            }
            if (IsPokemonSpawnMessage(message))
            {
                _ = Task.Run(async () =>
                {
                    Embed embed = message.Embeds.First();
                    if (embed.Image != null && embed.Image.HasValue)
                    {
                        UrlResponseByteContent response = await _httpHelper.GetUrlContent(embed.Image.Value.Url, HttpClientType.Pokemon.ToString());

                        if(_httpHelper.IsSuccessStatusCode(response.HttpStatusCode))
                        {
                            string pokemonName = PredictPokemon(response.Content);
                            if (string.IsNullOrEmpty(pokemonName))
                            {
                                return;
                            }

                            string[] pokemon = pokemonName.Split(';');
                            if (pokemon.Length > 1 && IsRare(pokemon[1]))
                            {
                                await message.Channel.SendMessageAsync("<@&1045590165747400777>");
                            }
                            await message.ReplyAsync(pokemon[0]);
                        }
                        else if(response.Content != null)
                        {
                            string errorMessage = Encoding.UTF8.GetString(response.Content);
                            await message.ReplyAsync(errorMessage);
                        }
                        
                    }
                });
                return;
            }
            if (message == null || socketMessage.Author.IsBot)
            {
                return;
            }
            int argPos = 0;
            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);
                await _command.ExecuteAsync(context: context, argPos: argPos, services: _service);
            }
        }

        private bool IsRarePokemonSpawn(SocketMessage socketMessage)
        {
            if (socketMessage == null || !socketMessage.Author.IsBot)
            {
                return false;
            }
            return socketMessage.MentionedRoles.FirstOrDefault(x => x.Id == 1045590165747400777) != null;
        }

        private bool IsRare(string pokemonName)
        {
            //This will currently just check for shadow pokemons
            if (pokemonName.Equals("2", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private bool IsPokemonSpawnMessage(SocketUserMessage message)
        {
            if (message == null || !message.Author.IsBot || message.Author.Id != 669228505128501258)
            {
                return false;
            }
            Embed? embed = message.Embeds.FirstOrDefault();
            if (embed == null || string.IsNullOrEmpty(embed.Title))
            {
                return false;
            }
            return embed.Title.Equals("A wild pokémon has аppeаred!", StringComparison.InvariantCultureIgnoreCase);
        }

        private string PredictPokemon(byte[]? content)
        {
            string pokemonName = _predictor.PredictSingle(_predictionEngine.GetPredictionEngine(), content);
            return pokemonName;
        }
    }
}
