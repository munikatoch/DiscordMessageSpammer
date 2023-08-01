using Interfaces.MlTrainer;
using Interfaces;
using Interfaces.Discord;
using Interfaces.Discord.Service;

namespace DiscordPokemonNameBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Container.Register();

            IAppConfiguration configuration = Container.ResolveDiscordBot<IAppConfiguration>();
            bool isDeleteWorkspaceAndModel = configuration.GetValue("RemoveFilesAndTrainModelAgain", false);
            bool isTrainModelAgain = configuration.GetValue("TrainModelAgain", false);

            IMlModelTrainer modelTrainer = Container.ResolveMlModel<IMlModelTrainer>();
            modelTrainer.TrainerModel(isDeleteWorkspaceAndModel, isTrainModelAgain);

            //IPokemonService pokemonService = Container.ResolveDiscordBot<IPokemonService>();
            //pokemonService.InsertPokemons();

            IDiscordBot discordBot = Container.ResolveDiscordBot<IDiscordBot>();
            discordBot.ConnectAndStartBot().GetAwaiter().GetResult();
        }
    }
}