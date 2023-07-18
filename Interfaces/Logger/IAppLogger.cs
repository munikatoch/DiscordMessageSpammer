using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Logger
{
    public interface IAppLogger
    {
        void ConsoleLogger(string message, ConsoleColor color = ConsoleColor.Gray);
        Task DiscrodChannelLogger(string message, ulong guildId, ulong channelId);
        Task ExceptionLog(string source, Exception exception);
        void CommandUsedLog(string source, string command, ulong channelId, ulong userId, ulong guildId);
        Task FileLogger(object message);
    }
}
