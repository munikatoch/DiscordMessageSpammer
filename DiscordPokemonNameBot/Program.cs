using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordPokemonNameBot.Handler;
using DiscordPokemonNameBot.Helper;
using DiscordPokemonNameBot.Model;
using DiscordPokemonNameBot.Module;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;
using PokemonPredictor;
using PokemonPredictor.Models;
using System;
using System.Configuration;
using Microsoft.Extensions.ML;
using System.Reflection.Metadata.Ecma335;
using System.Net.WebSockets;
using System.Net;
using PokemonPredictor.Common;
using System.Collections;
using System.Text;

namespace DiscordPokemonNameBot
{
    internal class Program
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string ModelZipFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../PokemonPredictor", @"Assets\Output", "trainedmodel.zip"));

        public Program()
        {
            IServiceCollection serviceCollection = CreateProvider();
            TrainModel(serviceCollection.BuildServiceProvider());
            serviceCollection.AddPredictionEnginePool<ModelInput, ModelOutput>().FromFile(ModelZipFolder);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        static void Main(string[] args)
            => new Program().RunAsync(args).GetAwaiter().GetResult();

        static IServiceCollection CreateProvider()
        {
            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.MessageContent |
                                 GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true,
            };
            IServiceCollection collection = new ServiceCollection();
            collection.AddSingleton(config)
                .AddSingleton<PublicApi>()
                .AddSingleton<DiscordShardedClient>()
                .AddSingleton<InteractionService>()
                .AddSingleton<CommandService>()
                .AddSingleton<SlashCommandHandler>()
                .AddSingleton<PrefixHandler>()
                .AddSingleton<MessageSpamSlashCommandModule>()
                .AddSingleton<MessageSpamPrefixCommandModule>()
                .AddSingleton<SpamMessage>()
                .AddSingleton<Predictor>()
                .AddSingleton<HttpHelper>()
                .AddScoped<Random>()
                .AddScoped(x => new MLContext(seed: 1));

            collection.AddHttpClient(HttpClientType.Pokemon.ToString());
            collection.AddHttpClient(HttpClientType.RandomBeer.ToString());
            collection.AddHttpClient(HttpClientType.RandomParagraph.ToString());
            collection.AddHttpClient(HttpClientType.Discord.ToString(), p =>
            {
                p.DefaultRequestHeaders.Add("authorization", ConfigurationManager.AppSettings["DiscordUserAuthToken"]);
            });
            return collection;
        }

        public async Task RunAsync(string[] args)
        {
            await StartDiscordBot();
        }

        private async Task StartDiscordBot()
        {
            DiscordShardedClient client = _serviceProvider.GetRequiredService<DiscordShardedClient>();

            InteractionService slashCommand = _serviceProvider.GetRequiredService<InteractionService>();
            await _serviceProvider.GetRequiredService<SlashCommandHandler>().InitializeAsync();

            CommandService prefixCommand = _serviceProvider.GetRequiredService<CommandService>();
            await _serviceProvider.GetRequiredService<PrefixHandler>().InitializeAsync();

            client.Log += CreateClientLogging;
            slashCommand.Log += CreateSlashCommandLogging;
            prefixCommand.Log += CreatePrefixCommandLogging;

            client.ShardReady += async (x) =>
            {
                var didParse = UInt64.TryParse(ConfigurationManager.AppSettings["GuildId"], out ulong guildId);
                if (didParse)
                {
                    await slashCommand.RegisterCommandsGloballyAsync(true);
                    //await slashCommand.RegisterCommandsToGuildAsync(guildId);
                }
                
                Console.WriteLine("Bot online and Ready!");
            };

            await client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["BotToken"]);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task CreatePrefixCommandLogging(LogMessage arg)
        {
            Console.WriteLine("Prefix Command logging");
            LogMessageModel logMessage = CreateLogMessageModel(arg.Source, arg.Message, arg.Exception.StackTrace, arg.Exception.GetType().Name);
            ConsoleHelper.PrintLogMessage(logMessage);
        }

        private async Task CreateSlashCommandLogging(LogMessage arg)
        {
            Console.WriteLine("Slash Command logging");

            if(arg.Exception is InvalidOperationException)
            {
                InvalidOperationException exception = (InvalidOperationException)arg.Exception;
            }

            LogMessageModel logMessage = CreateLogMessageModel(arg.Source, arg.Message, arg.Exception.StackTrace, arg.Exception.GetType().Name);
            ConsoleHelper.PrintLogMessage(logMessage);
        }

        private async Task CreateClientLogging(LogMessage arg)
        {
            Console.WriteLine("Client logging");
            string result = string.Empty;
            if (arg.Exception is WebException)
            {
                WebException exception = (WebException)arg.Exception;
                WebResponse? response = exception.Response;
                if (response != null)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            else if(arg.Exception is GatewayReconnectException)
            {
                GatewayReconnectException exception = (GatewayReconnectException)arg.Exception;
                result = GetGatewayReconnectExceptionResult(exception.Data);
            }
            else
            {
                result = arg.Exception?.Message;
            }
            LogMessageModel logMessage = CreateLogMessageModel(arg.Source, arg.Message, arg.Exception?.StackTrace, arg.Exception?.GetType().Name, result);
            ConsoleHelper.PrintLogMessage(logMessage);
        }

        private void TrainModel(ServiceProvider serviceProvider)
        {
            Predictor predictor = serviceProvider.GetRequiredService<Predictor>();
            bool.TryParse(ConfigurationManager.AppSettings["TrainModelAgain"], out bool trainModelAgain);
            bool.TryParse(ConfigurationManager.AppSettings["RemoveFilesAndTrainModelAgain"], out bool removeFilesAndTrainModelAgain);
            predictor.TrainPokemonModel(removeFilesAndTrainModelAgain, trainModelAgain);
        }

        private LogMessageModel CreateLogMessageModel(string source, string message, string? stacktrace, string? exceptionType, string content = "")
        {
            LogMessageModel logMessage = new LogMessageModel()
            {
                Message = message,
                ResponseContent = content,
                StackTrace = stacktrace,
                Source = source,
                ExceptionType = exceptionType,
            };
            return logMessage;
        }

        private string GetGatewayReconnectExceptionResult(IDictionary data)
        {
            StringBuilder sb = new StringBuilder();
            var enumerator = data.GetEnumerator();
            while (enumerator.MoveNext())
            {
                sb.Append($"Key: {enumerator.Key.ToString()}");
                if (enumerator.Value != null)
                {
                    sb.Append($", Value: {enumerator.Value.ToString()}");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}