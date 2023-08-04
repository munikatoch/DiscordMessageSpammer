using Common;
using Interfaces.DAO;
using Interfaces.Discord.Helper;
using Interfaces.Discord.Service;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Models;
using Models.DAO;
using Models.Discord.Common;
using Models.MlModelTrainer;

namespace DiscordPokemonNameBot.Service
{
    public class PokemonService : IPokemonService
    {
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEnginePool;
        private readonly MessageSpam _message;
        private readonly IHttpHelper _httpHelper;
        private readonly IPokemonRepository _pokemonRepository;

        public PokemonService(PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool, MessageSpam messageSpam, IHttpHelper httpHelper, IPokemonRepository pokemonRepository)
        {
            _predictionEnginePool = predictionEnginePool;
            _message = messageSpam;
            _httpHelper = httpHelper;
            _pokemonRepository = pokemonRepository;
        }

        public async Task PredictPokemon(string url, ulong? guildId)
        {
            byte[]? imageContent = await _httpHelper.GetImageContent(url, HttpClientType.Pokemon.ToString());
            if (imageContent != null && imageContent.Length > 0)
            {
                PredictionEngine<ModelInput, ModelOutput> predictionEngine = _predictionEnginePool.GetPredictionEngine();
                ModelInput imageToPredict = new ModelInput
                {
                    Image = imageContent
                };
                ModelOutput prediction = predictionEngine.Predict(imageToPredict);
                await BuildPokemonPredictionModel(prediction, guildId);
            }
        }

        public async Task InsertPokemons()
        {
            List<Pokemon> pokemons = new List<Pokemon>();

            string[] directories = Directory.GetDirectories(Constants.MlModelAssestsInputRelativePath);

            foreach (string directory in directories)
            {
                string[] subDirectories = Directory.GetDirectories(directory);
                foreach (string subDirectory in subDirectories)
                {
                    int id = 0;
                    string? pokemonName = FileUtils.GetAllFilesInDirectory(subDirectory).Where(x => int.TryParse(Path.GetFileNameWithoutExtension(x), out id)).Select(x => Directory.GetParent(x)).FirstOrDefault()?.Name.ToLower();

                    int pokemonType = 1;
                    if (subDirectory != null)
                    {
                        string? superParentName = Directory.GetParent(subDirectory)?.Name;
                        if (superParentName != null && superParentName.Equals("Rare", StringComparison.CurrentCultureIgnoreCase))
                        {
                            pokemonType |= (int)PokemonType.Rare;
                        }
                        if (pokemonName != null && pokemonName.StartsWith("shadow"))
                        {
                            pokemonType |= (int)PokemonType.Shadow;
                        }
                        if (pokemonName != null && (pokemonName.Contains("alolan") || pokemonName.Contains("galarian") || pokemonName.Contains("hisuian")))
                        {
                            pokemonType |= (int)PokemonType.Regional;
                        }
                    }

                    if (pokemonName != null)
                    {
                        if (pokemonName.Equals("mime jr", StringComparison.Ordinal))
                        {
                            pokemonName = "mime jr.";
                        }
                        else if (pokemonName.Equals("type null", StringComparison.Ordinal))
                        {
                            pokemonName = "type: null";
                        }
                        var pokemon = new Pokemon()
                        {
                            IsRare = (pokemonType & (int)PokemonType.Rare) > 0,
                            IsRegional = (pokemonType & (int)PokemonType.Regional) > 0,
                            IsShadow = (pokemonType & (int)PokemonType.Shadow) > 0,
                            PokemonId = id,
                            PokemonName = pokemonName
                        };
                        pokemons.Add(pokemon);
                    }
                }
            }

            await _pokemonRepository.InsertPokemonAsync(pokemons);
        }

        private async Task BuildPokemonPredictionModel(ModelOutput prediction, ulong? guildId)
        {
            if(guildId.HasValue)
            {
                int pokemonId = prediction.PredictedPokemonLabel;
                Pokemon? pokemon = await _pokemonRepository.GetPokemonById(pokemonId);
                if (pokemon != null && pokemon.IsRare) //Rare Pokemon
                {
                    var oldValue = _message.SpamDetail[guildId.Value];
                    var newValue = new SpamDetail()
                    {
                        DiscordChannelId = oldValue.DiscordChannelId,
                        DurationInSeconds = oldValue.DurationInSeconds,
                        IsSpamMessageEnabled = false
                    };
                    _message.SpamDetail.TryUpdate(guildId.Value, newValue, oldValue);
                }
            }
        }
    }
}
