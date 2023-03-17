using Discord.Interactions;
using Discord.WebSocket;
using DiscordPokemonNameBot.Module;

namespace DiscordPokemonNameBot.Handler
{
    public class SlashCommandHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _command;
        private readonly IServiceProvider _service;

        public SlashCommandHandler(DiscordShardedClient client, InteractionService command, IServiceProvider service)
        {
            _client = client;
            _command = command;
            _service = service;
        }

        public async Task InitializeAsync()
        {
            await _command.AddModuleAsync<MessageSpamSlashCommandModule>(_service);
            _client.SlashCommandExecuted += HandleSlashInteraction;
        }

        private async Task HandleSlashInteraction(SocketSlashCommand arg)
        {
            try
            {
                ShardedInteractionContext context = new ShardedInteractionContext(_client, arg);
                await _command.ExecuteCommandAsync(context, _service);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
