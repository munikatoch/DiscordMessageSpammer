namespace Models
{
    public class Constants
    {
        #region Bot Version
        public static readonly string BotVersion = "1.5";
        #endregion


        #region Ml Model

        public static readonly string ProjectRootDirectory = GetProjectRootPath();
        public static readonly string MlModelWorkSpaceRelativePath = Path.Combine(ProjectRootDirectory, "PokemonPredictor", "Workspace");
        public static readonly string MlModelAssestsInputRelativePath = Path.Combine(ProjectRootDirectory, "PokemonPredictor", "Assets", "Input");
        public static readonly string MlModelAssestsOutputRelativePath = Path.Combine(ProjectRootDirectory, "PokemonPredictor", "Assets", "Output");
        public static readonly string MlModelOutputFileName = "trainedmodel.zip";
        public static readonly string MlModelFileOutputPath = Path.Combine("Assets", "Output", MlModelOutputFileName);
        public static readonly string MlModelFilePath = Path.Combine(MlModelAssestsOutputRelativePath, MlModelOutputFileName);

        #endregion

        #region URL

        public static readonly string RandomParagraphUrl = "http://metaphorpsum.com/paragraphs/1";
        public static readonly string RandomQuoteUrl = "https://zenquotes.io/api/quote?api=random";
        public static readonly string RandomJokeUrl = "https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw,racist,sexist,explicit";
        public static readonly string DiscordMessageSpamUrl = "https://discord.com/api/v9/channels/{0}/messages";

        #endregion

        #region Logs

        public static readonly string Logfolder = Path.Combine(ProjectRootDirectory, "Log");
        public static readonly string LogZipfolder = Path.Combine(ProjectRootDirectory, "Zip", "logs.zip");
        public static readonly string EOFMarkup = "-------------------------------------------------------------------------------------------------------------------------------------------------";

        #endregion

        #region
        public static readonly ulong PokemonBotAuthorId = 669228505128501258;
        public static readonly ulong PokemonRarePingRoleId =  ulong.TryParse(Environment.GetEnvironmentVariable("PokemonRarePingRoleId"), out ulong rarePingId) ? rarePingId : 1045590165747400777;
        public static readonly ulong PokemonShadowPingRoleId = ulong.TryParse(Environment.GetEnvironmentVariable("PokemonShadowPingRoleId"), out ulong shadowPingId) ? shadowPingId : 1088862418907709530;

        //Below this are all ID of primary server for testing and contact
        public static readonly ulong GuildId = ulong.TryParse(Environment.GetEnvironmentVariable("GuildId"), out ulong guildId) ? guildId : 1037542119319015424;
        public static readonly ulong BotLogsChannel = ulong.TryParse(Environment.GetEnvironmentVariable("BotLatencyChannel"), out ulong latencyChannel) ? latencyChannel : 1085965584891662439;
        public static readonly ulong BotShardConnectedChannel = ulong.TryParse(Environment.GetEnvironmentVariable("BotShardConnectedChannel"), out ulong connectChannel) ? connectChannel : 1085966306853011598;
        public static readonly ulong BotShardDisconnectedChannel = ulong.TryParse(Environment.GetEnvironmentVariable("BotShardDisconnectedChannel"), out ulong disconnectChannel) ? disconnectChannel : 1085966439409778699;

        #endregion

        private static string GetProjectRootPath()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);

            int count = 0;

            while (directory.Parent != null && count < 4)
            {
                directory = directory.Parent;
                count++;
            }
            string root = directory.FullName;
            return root;
        }
    }
}