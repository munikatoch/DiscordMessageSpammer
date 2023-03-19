using Discord;
using Discord.WebSocket;
using Interfaces;
using Interfaces.Discord;
using Interfaces.Discord.Handler;
using Interfaces.Discord.Handler.InteractionHandler;
using Interfaces.Discord.Handler.PrefixHandler;

namespace DiscordPokemonNameBot
{
    public class DiscordBot : IDiscordBot
    {
        private readonly DiscordShardedClient _client;
        private readonly IAppConfiguration _appConfiguration;
        private readonly IDiscordClientLogHandler _clientLogHandler;
        private readonly IPrefixHandler _prefixHandler;
        private readonly IInteractionHandler _interactionHandler;

        public DiscordBot(DiscordShardedClient client, IAppConfiguration appConfiguration, IDiscordClientLogHandler clientLogHandler, IPrefixHandler prefixHandler, IInteractionHandler interactionHandler)
        {
            _client = client;
            _appConfiguration = appConfiguration;
            _clientLogHandler = clientLogHandler;
            _prefixHandler = prefixHandler;
            _interactionHandler = interactionHandler;
        }

        public async Task ConnectAndStartBot()
        {
            string discordBotToken = _appConfiguration.GetAppSettingValue("BotToken", string.Empty);

            _clientLogHandler.Initialize();
            await _prefixHandler.InitializeAsync();
            await _interactionHandler.InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, discordBotToken);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}