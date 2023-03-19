using Discord;
using Discord.WebSocket;
using Interfaces.Discord.Service;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordPokemonNameBot.Service
{
    public class PrefixService : IPrefixService
    {
        public bool ValidatePokemonSpanMessage(SocketUserMessage message, out Embed? pokemonEmbed)
        {
            pokemonEmbed = null;
            if (message.Author.Id != Constants.PokemonBotAuthorId)
            {
                return false;
            }

            pokemonEmbed= message.Embeds.FirstOrDefault();
            if (pokemonEmbed == null || string.IsNullOrEmpty(pokemonEmbed.Title))
            {
                return false;
            }
            return pokemonEmbed.Title.Equals("A wild pokémon has аppeаred!", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
