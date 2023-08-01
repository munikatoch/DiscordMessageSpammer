using Interfaces.DAO;
using Interfaces.Logger;
using Models.DAO;
using MongoDB.Driver;

namespace Repository
{
    public class PokemonRepository : IPokemonRepository
    {
        private readonly IMongoCollection<Pokemon> _collection;
        private readonly IAppLogger _logger;

        public PokemonRepository(MongoClient client, IAppLogger logger)
        {
            _collection = client.GetDatabase("DiscordBot").GetCollection<Pokemon>("Pokemon");
            _logger = logger;
        }

        public async Task<Pokemon> GetPokemonById(int pokemonId)
        {
            FilterDefinition<Pokemon> filter = Builders<Pokemon>.Filter.Eq(r => r.PokemonId, pokemonId);
            Pokemon result = await _collection.Find(filter).FirstOrDefaultAsync();
            return result;
        }

        public async Task InsertPokemonAsync(List<Pokemon> pokemon)
        {
            try
            {
                int n = pokemon.Count / 100;
                if(pokemon.Count % 100 != 0)
                {
                    n++;
                }
                for(int i = 0; i < n; i++)
                {
                    List<Pokemon> pokemons = pokemon.Skip(i * 100).Take(100).ToList();
                    await _collection.InsertManyAsync(pokemons);
                }
            }
            catch(Exception ex)
            {
                await _logger.ExceptionLog("PokemonRepository", ex).ConfigureAwait(false);
            }
        }
    }
}