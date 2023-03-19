using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Discord
{
    public class PokemonPrediction
    {
        public Embed? PokemonEmbed { get; set; }
        public string? RoleTag { get; set; }
        public string? FollowUpMessageOnRarePing { get; set; }
    }
}
