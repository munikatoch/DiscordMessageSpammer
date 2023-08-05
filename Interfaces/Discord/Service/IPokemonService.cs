using Models.DAO;
using Models.Discord;

namespace Interfaces.Discord.Service
{
    public interface IPokemonService
    {
        Task PredictPokemon(string url, ulong? guildId, ulong channelId);

        Task InsertPokemons();
    }
}
