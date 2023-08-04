using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Discord.Service
{
    public interface IDiscordService
    {
        Task CreateAndSendSpamMessage(ulong id);
        Task<int> DeleteMessage(ITextChannel textChannel, int count = 99);
        Task SendRedirectSpawnMessage(ulong channelId, ulong spamChannelId);
    }
}
