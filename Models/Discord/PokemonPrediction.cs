using Discord;

namespace Models.Discord
{
    public class PokemonPrediction
    {
        public Embed? PokemonEmbed { get; set; }
        public string? RoleTag { get; set; }
        public string? FollowUpMessageOnRarePing { get; set; }
    }
}
