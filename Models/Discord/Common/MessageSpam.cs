using Discord.Rest;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Discord.Common
{
    public class MessageSpam
    {
        public string? Message { get; set; }
        public ConcurrentDictionary<ulong, SpamDetail> SpamDetail { get; set; } = new ConcurrentDictionary<ulong, SpamDetail>();
    }

    public class SpamDetail : IEqualityComparer<SpamDetail>
    {
        public bool IsSpamMessageEnabled { get; set; }
        public ulong DiscordChannelId { get; set; }
        public List<ulong> PokemonSpawnChannel { get; set; } = new List<ulong>();
        public TimeSpan DurationInSeconds { get; set; }
        public int CurrentIndex { get; set; }

        public bool Equals(SpamDetail? oldDetail, SpamDetail? newDetail)
        {
            if(oldDetail == null && newDetail == null)
            {
                return true;
            }
            else if((oldDetail != null && newDetail == null) || (oldDetail == null && newDetail != null))
            {
                return false;
            }
            return oldDetail.IsSpamMessageEnabled == newDetail.IsSpamMessageEnabled
                && oldDetail.DurationInSeconds == newDetail.DurationInSeconds
                && oldDetail.PokemonSpawnChannel.SequenceEqual(newDetail.PokemonSpawnChannel);
        }

        public int GetHashCode([DisallowNull] SpamDetail detail)
        {
            return detail.PokemonSpawnChannel.GetHashCode() + detail.DurationInSeconds.GetHashCode();
        }
    }
}
