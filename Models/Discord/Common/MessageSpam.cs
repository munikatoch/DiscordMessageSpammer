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
        public ConcurrentDictionary<ulong, SpamDetail> SpamDetail { get; set; }

        public MessageSpam()
        {
            SpamDetail = new ConcurrentDictionary<ulong, SpamDetail>();
            
            SpamDetail myServer = new SpamDetail()
            {
                DiscordChannelId = 0,
                DurationInSeconds = TimeSpan.Zero,
                IsSpamMessageEnabled = false,
                PokemonSpawnChannel = new List<ulong> { 1080102636575014923, 1079972351015403553, 1136956960164548711 }
            };

            SpamDetail liliServer = new SpamDetail()
            {
                DiscordChannelId = 0,
                DurationInSeconds = TimeSpan.Zero,
                IsSpamMessageEnabled = false,
                PokemonSpawnChannel = new List<ulong> { 1098869965735608390, 1098870021964431360, 1098870061789356092 }
            };

            SpamDetail botServer = new SpamDetail()
            {
                DiscordChannelId = 0,
                DurationInSeconds = TimeSpan.Zero,
                IsSpamMessageEnabled = false,
                PokemonSpawnChannel = new List<ulong> { 1136909102782619718, 1136909128632119356, 1136962632356737144 }
            };

            SpamDetail.TryAdd(1136906515056447559, botServer);
            SpamDetail.TryAdd(1037542119319015424, myServer);
            SpamDetail.TryAdd(1084476576843976754, liliServer);
        }

        
    }

    public class SpamDetail : IEqualityComparer<SpamDetail>
    {
        public bool IsSpamMessageEnabled { get; set; }
        public ulong DiscordChannelId { get; set; }
        public List<ulong> PokemonSpawnChannel { get; set; } = new List<ulong>();
        public TimeSpan DurationInSeconds { get; set; }

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
