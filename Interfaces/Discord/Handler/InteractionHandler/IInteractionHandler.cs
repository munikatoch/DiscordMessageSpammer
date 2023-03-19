using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Discord.Handler.InteractionHandler
{
    public interface IInteractionHandler
    {
        Task InitializeAsync();
    }
}
