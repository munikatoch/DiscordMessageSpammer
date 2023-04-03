using Discord;
using Discord.WebSocket;
using Interfaces;
using Interfaces.Discord;
using Interfaces.Discord.Handler;
using Interfaces.Discord.Handler.InteractionHandler;
using Interfaces.Discord.Handler.PrefixHandler;
using Interfaces.Logger;
using Logging;

namespace DiscordPokemonNameBot
{
    public class DiscordBot : IDiscordBot
    {
        private readonly DiscordShardedClient _client;
        private readonly IAppConfiguration _appConfiguration;
        private readonly IDiscordClientLogHandler _clientLogHandler;
        private readonly IPrefixHandler _prefixHandler;
        private readonly IInteractionHandler _interactionHandler;
        private readonly IAppLogger _appLogger;

        public DiscordBot(DiscordShardedClient client, IAppConfiguration appConfiguration, IDiscordClientLogHandler clientLogHandler, IPrefixHandler prefixHandler, IInteractionHandler interactionHandler, IAppLogger appLogger)
        {
            _client = client;
            _appConfiguration = appConfiguration;
            _clientLogHandler = clientLogHandler;
            _prefixHandler = prefixHandler;
            _interactionHandler = interactionHandler;
            _appLogger = appLogger;
        }

        public async Task ConnectAndStartBot()
        {
            string discordBotToken = _appConfiguration.GetValue("DiscordBotToken", string.Empty);

            _clientLogHandler.Initialize();
            await _prefixHandler.InitializeAsync();
            await _interactionHandler.InitializeAsync();

            _appLogger.ConsoleLogger("Starting bot", ConsoleColor.Green);

            await _client.LoginAsync(TokenType.Bot, discordBotToken);
            await _client.StartAsync();

            _appLogger.ConsoleLogger("Bot online", ConsoleColor.Green);

            await Task.Delay(Timeout.Infinite);
        }
    }
}