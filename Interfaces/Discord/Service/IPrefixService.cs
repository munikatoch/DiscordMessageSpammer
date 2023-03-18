using Discord;
using Discord.WebSocket;

namespace Interfaces.Discord.Service
{
    public interface IPrefixService
    {
        bool ValidatePokemonSpanMessage(SocketUserMessage message, out Embed? pokemonEmbed);
    }
}
