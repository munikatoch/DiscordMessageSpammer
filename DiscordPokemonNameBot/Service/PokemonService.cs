using Discord;
using Interfaces.Discord.Helper;
using Interfaces.Discord.Service;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Models;
using Models.Discord;
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

        public async Task<PokemonPrediction> PredictPokemon(string url)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Error occured while predicting the pokemon. Please check the input";
            PokemonPrediction predictedPokemon = new PokemonPrediction()
            {
                PokemonEmbed = embed.Build()
            };
            byte[]? imageContent = await _httpHelper.GetImageContent(url, HttpClientType.Pokemon.ToString());
            if (imageContent != null && imageContent.Length > 0)
            {
                PredictionEngine<ModelInput, ModelOutput> predictionEngine = _predictionEnginePool.GetPredictionEngine();
                ModelInput imageToPredict = new ModelInput
                {
                    Image = imageContent
                };
                ModelOutput prediction = predictionEngine.Predict(imageToPredict);
                predictedPokemon = BuildPokemonPredictionModel(embed, prediction);
                return predictedPokemon;
            }
            return predictedPokemon;
        }

        private PokemonPrediction BuildPokemonPredictionModel(EmbedBuilder embed, ModelOutput prediction)
        {
            PokemonPrediction predictedPokemon = new PokemonPrediction();
            string[] pokemonTrait = prediction.PredictedPokemonLabel.Split('|');

            embed.Title = "Spawned Pokemon Name !!!";
            embed.Description = $"Pokemon prediction";
            embed.AddField("Prediction Accuracy: ", prediction.Score.Max() * 100);
            embed.AddField("Pokemon Name: ", pokemonTrait[0]);

            if (pokemonTrait.Length > 1)
            {
                if (pokemonTrait[1].Equals("2")) //Rare Pokemon
                {
                    if(Constants.PokemonRarePingRoleId != 0)
                        predictedPokemon.RoleTag = $"<@&{Constants.PokemonRarePingRoleId}>";
                    predictedPokemon.FollowUpMessageOnRarePing = "Please start the message spam again to start spawning the pokemons again";
                    _messageSpam.IsSpamMessageEnabled = false;
                }
                else if (pokemonTrait[1].Equals("3")) //Shadow Pokemon
                {
                    if(Constants.PokemonShadowPingRoleId != 0)
                        predictedPokemon.RoleTag = $"<@&{Constants.PokemonShadowPingRoleId}>";
                }
            }
            embed.WithCurrentTimestamp();

            predictedPokemon.PokemonEmbed = embed.Build();

            return predictedPokemon;
        }
    }
}
