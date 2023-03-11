using System.Net;

namespace DiscordPokemonNameBot.Model
{
    public class UrlResponseByteContent
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public byte[]? Content { get; set; }
    }
}
