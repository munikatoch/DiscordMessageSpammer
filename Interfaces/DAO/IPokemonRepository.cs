using Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.DAO
{
    public interface IPokemonRepository
    {
        Task InsertPokemonAsync(List<Pokemon> pokemon);
        Task<Pokemon?> GetPokemonById(int pokemonId);
    }
}
