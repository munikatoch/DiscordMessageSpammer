using Discord;
using Interfaces;
using Interfaces.Discord.Helper;
using Interfaces.Discord.Service;
using Models;
using Models.Discord.Common;
using Models.Discord.Request;
using Models.Discord.Response;
using Tensorflow.Contexts;

namespace DiscordPokemonNameBot.Service
{
    public class DiscordService : IDiscordService
    {
        private readonly Random _random;
        private readonly IHttpHelper _httpHelper;
        private readonly MessageSpam _message;
        private readonly IAppConfiguration _appConfiguration;

        public DiscordService(Random random, IHttpHelper httpHelper, MessageSpam message, IAppConfiguration appConfiguration)
        {
            _random = random;
            _httpHelper = httpHelper;
            _message = message;
            _appConfiguration = appConfiguration;
        }

        public async Task CreateAndSendSpamMessage(ulong id)
        {
            int type = _random.Next(0, 3);
            switch (type)
            {
                case 0:
                    string? response = await _httpHelper.GetRequest<string>(Constants.RandomParagraphUrl, HttpClientType.RandomParagraph.ToString());
                    _message.Message = "**Paragraph**\n" + response;
                    break;
                case 1:
                    RandomJokeResponse? jokeResponse = await _httpHelper.GetRequest<RandomJokeResponse>(Constants.RandomJokeUrl, HttpClientType.RandomJoke.ToString());
                    if (jokeResponse != null)
                    {
                        if (jokeResponse.Type == JokeType.Single)
                        {
                            _message.Message = jokeResponse.Joke;
                        }
                        else
                        {
                            _message.Message = $"{jokeResponse.Setup}\n{jokeResponse.Delivery}";
                        }
                        _message.Message = "**Joke**\n" + _message.Message;
                    }
                    break;
                case 2:
                    List<RandomQuoteResponse>? quoteResponses = await _httpHelper.GetRequest<List<RandomQuoteResponse>>(Constants.RandomQuoteUrl, HttpClientType.RandomQuote.ToString());
                    if (quoteResponses != null && quoteResponses.Count > 0)
                    {
                        RandomQuoteResponse quoteResponse = quoteResponses.First();
                        _message.Message = $"**Quote**\n{quoteResponse.Quote}\nBy - {quoteResponse.Author}";
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(_message.Message))
            {
                await SendMessage(_message.Message, id);
            }
        }

        public async Task<int> DeleteMessage(ITextChannel textChannel, int count = 99)
        {
            IEnumerable<IMessage> messages = await textChannel.GetMessagesAsync(count + 1).FlattenAsync();
            int messageCount = messages.Count();
            if (messageCount > 0)
                await textChannel.DeleteMessagesAsync(messages);
            return messageCount;
        }

        private async Task SendMessage(string message, ulong id)
        {
            string url = string.Format(Constants.DiscordMessageSpamUrl, id);

            DiscordMessageRequest request = CreateRequest(message);
            Dictionary<string, string> headers = CreateHeadersForDiscrodMessage();
            await _httpHelper.FormUrlEcodedContentPostRequest(url, request, HttpClientType.Discord.ToString(), headers);
        }

        private DiscordMessageRequest CreateRequest(string content)
        {
            return new DiscordMessageRequest()
            {
                Content = content
            };
        }

        private Dictionary<string, string> CreateHeadersForDiscrodMessage()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            var userToken = _appConfiguration.GetAppSettingValue("DiscordUserAuthToken", string.Empty);
            headers.Add("authorization", userToken);
            return headers;
        }
    }
}
