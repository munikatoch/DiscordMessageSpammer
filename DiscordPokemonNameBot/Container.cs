using Interfaces.Logger;
using Interfaces.MlTrainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;
using PokemonPredictor;
using Logging;
using Interfaces;
using DiscordPokemonNameBot.Configuration;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Models.MlModelTrainer;
using Microsoft.Extensions.ML;
using Interfaces.Discord;
using Interfaces.Discord.Handler;
using DiscordPokemonNameBot.Handler;
using Interfaces.Discord.Handler.PrefixHandler;
using DiscordPokemonNameBot.Handler.PrefixHandler;
using Interfaces.Discord.Handler.InteractionHandler;
using DiscordPokemonNameBot.Handler.InteractionHandler;
using Interfaces.Discord.Service;
using DiscordPokemonNameBot.Service;
using Models.Discord.Common;
using Interfaces.Discord.Helper;
using DiscordPokemonNameBot.Helper;
using Serilog;
using Serilog.Exceptions;
using Interfaces.DAO;
using Repository;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;

namespace DiscordPokemonNameBot
{
    public static class Container
    {
        private static IServiceProvider? _mlModelServiceProvider;
        public static IServiceProvider MlModelServiceProvider
        {
            get
            {
                if (_mlModelServiceProvider == null)
                {
                    _mlModelServiceProvider = RegisterMlModel();
                }
                return _mlModelServiceProvider;
            }
        }


        private static IServiceProvider? _discordBotServiceProvider;
        public static IServiceProvider DiscordBotServiceProvider
        {
            get
            {
                if (_discordBotServiceProvider == null)
                {
                    _discordBotServiceProvider = RegisterDiscordBot();
                }
                return _discordBotServiceProvider;
            }
        }

        public static void Register()
        {
            _mlModelServiceProvider = RegisterMlModel();
            _discordBotServiceProvider = RegisterDiscordBot();
        }

        public static T ResolveMlModel<T>() where T : notnull
        {
            return MlModelServiceProvider.GetRequiredService<T>();
        }

        public static T ResolveDiscordBot<T>() where T : notnull
        {
            return DiscordBotServiceProvider.GetRequiredService<T>();
        }

        private static IServiceProvider RegisterMlModel()
        {
            IServiceCollection collection = new ServiceCollection();

            collection.AddSingleton<IAppConfiguration, AppConfiguration>();

            collection.AddScoped<IMlModelTrainer, MlModelTrainer>();
            collection.AddScoped(x => new MLContext(seed: 1));
            collection.AddScoped<IAppLogger, AppLogger>();
            collection.AddSingleton<DiscordShardedClient>();

            using var logger = new LoggerConfiguration().WriteTo.File(
                Models.Constants.Logfile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 3)
                .Enrich.WithExceptionDetails()
                .CreateLogger();

            collection.AddSingleton<ILogger>(logger);

            return collection.BuildServiceProvider();
        }

        private static IServiceProvider RegisterDiscordBot()
        {
            IServiceCollection collection = new ServiceCollection();

            #region Configs

            collection.AddSingleton(x => new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.MessageContent |
                                 GatewayIntents.AllUnprivileged,
                LogLevel = LogSeverity.Info,
            });

            collection.AddSingleton(x => new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = Discord.Commands.RunMode.Async,
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Info
            });

            collection.AddSingleton(x => new InteractionServiceConfig()
            {
                DefaultRunMode = Discord.Interactions.RunMode.Async,
                LogLevel = LogSeverity.Info,
            });

            collection.AddSingleton<IAppConfiguration, AppConfiguration>();

            #endregion

            #region Discord Package

            collection.AddSingleton<DiscordShardedClient>();
            collection.AddSingleton<CommandService>();
            collection.AddSingleton<InteractionService>();

            #endregion

            #region Discord Bot

            collection.AddScoped<IDiscordBot, DiscordBot>();
            collection.AddSingleton<IDiscordClientLogHandler, DiscordClientLogHandler>();
            collection.AddScoped<IPrefixHandler, PrefixCommandHandler>();
            collection.AddScoped<IInteractionHandler, InteractionCommandHandler>();
            collection.AddScoped<IDiscordService, DiscordService>();
            collection.AddScoped<IPokemonService, PokemonService>();
            collection.AddScoped<IPrefixService, PrefixService>();
            collection.AddScoped<IHttpHelper, HttpHelper>();
            collection.AddSingleton<Random>();
            collection.AddSingleton<MessageSpam>();

            #endregion

            #region Pokemon Prediction

            collection.AddPredictionEnginePool<ModelInput, ModelOutput>().FromFile(Models.Constants.MlModelFileOutputPath);
            #endregion

            #region Http Client

            collection.AddHttpClient(HttpClientType.RandomParagraph.ToString(), x =>
            {
                x.Timeout = TimeSpan.FromSeconds(3);
            }).AddPolicyHandler(GetRetryPolicy());

            collection.AddHttpClient(HttpClientType.RandomJoke.ToString(), x =>
            {
                x.Timeout = TimeSpan.FromSeconds(3);
            }).AddPolicyHandler(GetRetryPolicy());

            collection.AddHttpClient(HttpClientType.RandomQuote.ToString(), x =>
            {
                x.Timeout = TimeSpan.FromSeconds(3);
            }).AddPolicyHandler(GetRetryPolicy());

            collection.AddHttpClient(HttpClientType.Pokemon.ToString(), x =>
            {
                x.Timeout = TimeSpan.FromSeconds(3);
            }).AddPolicyHandler(GetRetryPolicy());

            collection.AddHttpClient(HttpClientType.Discord.ToString(), x =>
            {
                x.Timeout = TimeSpan.FromSeconds(3);
            }).AddPolicyHandler(GetRetryPolicy());

            collection.AddHttpClient(HttpClientType.RandomUselessFact.ToString(), x =>
            {
                x.Timeout = TimeSpan.FromSeconds(3);
            }).AddPolicyHandler(GetRetryPolicy());

            collection.AddHttpClient(HttpClientType.RandomActivity.ToString(), x =>
            {
                x.Timeout = TimeSpan.FromSeconds(3);
            }).AddPolicyHandler(GetRetryPolicy());

            #endregion

            #region Database

            collection.AddSingleton(x => new MongoClient(x.GetRequiredService<IAppConfiguration>().GetValue("MongoDBConnectionString", string.Empty)));
            collection.AddScoped<IPokemonRepository, PokemonRepository>();

            #endregion

            using var logger = new LoggerConfiguration().WriteTo.File(
                Models.Constants.Logfile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 3)
                .Enrich.WithExceptionDetails()
                .CreateLogger();

            collection.AddSingleton<ILogger>(logger);
            collection.AddScoped<IAppLogger, AppLogger>();
            return collection.BuildServiceProvider();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(3 * retryAttempt));
        }
    }
}
