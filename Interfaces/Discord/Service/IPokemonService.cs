using Discord;

namespace Interfaces.Discord.Service
{
    public interface IPokemonService
    {
        Task<Embed> PredictPokemon(string url);
    }
}
