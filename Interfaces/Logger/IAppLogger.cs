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
        void FileLogger(string folder, string message);
        Task DiscrodChannelLogger(string message, ulong guildId, ulong channelId);
        void ExceptionLog(string folder, Exception exception);
        void CommandUsedLog(string folder, string command, ulong channelId, ulong userId, ulong guildId);
    }
}
