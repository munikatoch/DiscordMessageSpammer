using DiscordPokemonNameBot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DiscordPokemonNameBot.Helper
{
    public class PublicApi
    {
        private readonly string _RandomParagraphUri = "http://metaphorpsum.com/sentences/{0}";
        private readonly string _RandomBeersUri = "https://random-data-api.com/api/v2/beers?response_type=json&size={0}";
        private readonly string _discordMessage = "https://discord.com/api/v9/channels/1079086310783995994/messages";

        private HttpHelper _httpHelper;

        public PublicApi(HttpHelper httpHelper) 
        {
            _httpHelper = httpHelper;
        }

        public async Task<string> GetRandomParagraphs(int numberOfParagraphs)
        {
            string uri = string.Format(_RandomParagraphUri, numberOfParagraphs);
            string data = await _httpHelper.GetRequestAsString(uri, HttpClientType.RandomParagraph.ToString());
            return data;
        }

        public async Task<string> GetRandomBeers(int size)
        {
            string uri = string.Format(_RandomBeersUri, size);
            string data = await _httpHelper.GetRequestAsString(uri, HttpClientType.RandomBeer.ToString());
            if (size == 1)
            {
                BeerModel beer = JsonConvert.DeserializeObject<BeerModel>(data);
                return beer.ToString();
            }
            List<BeerModel> beers = JsonConvert.DeserializeObject<List<BeerModel>>(data);
            StringBuilder sb = new StringBuilder();
            beers?.ForEach(x => sb.AppendLine(x.ToString()));
            return sb.ToString();
        }

        public async Task SendMessageToDiscord(string spamMessage)
        {
            DiscordMessageRequest spamMessageContent = new DiscordMessageRequest()
            {
                Content = spamMessage
            };
            await _httpHelper.PostRequestWithAuthToken(_discordMessage, spamMessageContent, HttpClientType.Discord.ToString());
        }
    }
}
