namespace Models
{
    public class Constants
    {
        #region Ml Model

        public readonly static string ProjectRootDirectory = GetProjectRootPath();
        public readonly static string MlModelWorkSpaceRelativePath = Path.Combine(ProjectRootDirectory, "PokemonPredictor", "Workspace");
        public readonly static string MlModelAssestsInputRelativePath = Path.Combine(ProjectRootDirectory, "PokemonPredictor", "Assets", "Input");
        public readonly static string MlModelAssestsOutputRelativePath = Path.Combine(ProjectRootDirectory, "PokemonPredictor", "Assets", "Output");
        public readonly static string MlModelOutputFileName = "trainedmodel.zip";
        public readonly static string MlModelFileOutputPath = Path.Combine("Assets", "Output", MlModelOutputFileName);
        public readonly static string MlModelFilePath = Path.Combine(MlModelAssestsOutputRelativePath, MlModelOutputFileName);

        #endregion

        #region URL

        public readonly static string RandomParagraphUrl = "http://metaphorpsum.com/paragraphs/1";
        public readonly static string RandomQuoteUrl = "https://zenquotes.io/api/quote?api=random";
        public readonly static string RandomJokeUrl = "https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw,racist,sexist,explicit";
        public readonly static string DiscordMessageSpamUrl = "https://discord.com/api/v9/channels/{0}/messages";

        #endregion

        #region Logs

        public readonly static string Logfolder = Path.Combine(ProjectRootDirectory, "Log");
        public readonly static string EOFMarkup = "-------------------------------------------------------------------------------------------------------------------------------------------------";

        #endregion

        #region
        public readonly static ulong PokemonBotAuthorId = 669228505128501258;
        public readonly static ulong PokemonRarePingRoleId = 1045590165747400777;
        public readonly static ulong PokemonShadowPingRoleId = 1088862418907709530;

        //Below this are all ID of primary server for testing and contact
        public readonly static ulong GuildId = 1037542119319015424;
        public readonly static ulong BotLatencyChannel = 1085965584891662439;
        public readonly static ulong BotGuildJoinChannel = 1085965692546854974;
        public readonly static ulong BotShardConnectedChannel = 1085966306853011598;
        public readonly static ulong BotShardDisconnectedChannel = 1085966439409778699;

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