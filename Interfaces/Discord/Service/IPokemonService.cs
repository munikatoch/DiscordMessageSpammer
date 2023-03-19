using Models.Discord;

namespace Interfaces.Discord.Service
{
    public interface IPokemonService
    {
        Task<PokemonPrediction> PredictPokemon(string url);
    }
}
