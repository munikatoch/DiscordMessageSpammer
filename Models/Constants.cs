namespace Models
{
    public class Constants
    {
        #region Bot Version
        public static readonly string BotVersion = "1.7";
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
        public static readonly string RandomUselessFactUrl = "https://uselessfacts.jsph.pl/api/v2/facts/random?language=en";
        public static readonly string RandomActivityUrl = "https://www.boredapi.com/api/activity/";

        #endregion

        #region Logs

        public static readonly string Logfolder = Path.Combine(ProjectRootDirectory, "Log");
        public static readonly string LogZipfolder = Path.Combine(ProjectRootDirectory, "Zip");
        public static readonly string LogZipfile = Path.Combine(LogZipfolder, "logs.zip");
        public static readonly string Logfile = Path.Combine(Logfolder, "log.txt");

        #endregion

        #region
        public static readonly ulong PokemonBotAuthorId = 669228505128501258;
        public static readonly ulong GuildId = 1084476576843976754;
        public static readonly ulong BotLogsChannel = 1098890782930387004;
        public static readonly ulong BotShardConnectedChannel = 1098890782930387004;
        public static readonly ulong BotShardDisconnectedChannel = 1098890782930387004;

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