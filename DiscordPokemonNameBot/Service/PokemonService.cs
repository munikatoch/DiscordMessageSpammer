using Discord;
using Interfaces.Discord.Helper;
using Interfaces.Discord.Service;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Models;
using Models.Discord.Common;
using Models.MlModelTrainer;

namespace DiscordPokemonNameBot.Service
{
    public class PokemonService : IPokemonService
    {
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEnginePool;
        private readonly MessageSpam _messageSpam;
        private readonly IHttpHelper _httpHelper;

        public PokemonService(PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool, MessageSpam messageSpam, IHttpHelper httpHelper)
        {
            _predictionEnginePool = predictionEnginePool;
            _messageSpam = messageSpam;
            _httpHelper = httpHelper;
        }

        public async Task<Embed> PredictPokemon(string url)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Error occured while predicting the pokemon!!!";
            byte[]? imageContent = await _httpHelper.GetImageContent(url, HttpClientType.Pokemon.ToString());
            if (imageContent != null && imageContent.Length > 0)
            {
                PredictionEngine<ModelInput, ModelOutput> predictionEngine = _predictionEnginePool.GetPredictionEngine();
                ModelInput imageToPredict = new ModelInput
                {
                    Image = imageContent
                };
                ModelOutput prediction = predictionEngine.Predict(imageToPredict);
                BuildEmbed(embed, prediction);
            }
            return embed.Build();
        }

        private void BuildEmbed(EmbedBuilder embed, ModelOutput prediction)
        {
            string[] pokemonTrait = prediction.PredictedPokemonLabel.Split('|');

            embed.Title = "Spawned Pokemon Name !!!";
            embed.Description = $"Prediction Accuracy: {prediction.Score.Max() * 100}";
            embed.AddField("Pokemon Name", pokemonTrait[0]);

            if (pokemonTrait.Length > 1)
            {
                if (pokemonTrait[1].Equals("2")) //Rare Pokemon
                {
                    embed.AddField("Role Tag", $"<@&{Constants.PokemonRarePingRoleId}>");
                    _messageSpam.IsSpamMessageEnabled = false;
                }
                else if (pokemonTrait[1].Equals("3"))
                {
                    // TO-DO for shadow pokemons
                }
            }
        }
    }
}
