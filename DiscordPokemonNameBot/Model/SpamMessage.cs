using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordPokemonNameBot.Model
{
    public class SpamMessage
    {
        public string Message { get; set; }
        public bool IsSpamMessageEnabled { get; set; }
        public bool IsGenerateRandomDurationEnabled { get; set; }
        public TimeSpan DurationInSeconds  { get; set; }
    }
}
