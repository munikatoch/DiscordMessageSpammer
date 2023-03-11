using DiscordPokemonNameBot.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DiscordPokemonNameBot.Helper
{
    public class HttpHelper
    {
        private IHttpClientFactory _clientFactory;

        public HttpHelper(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<string> GetRequestAsString(string uri, string type)
        {
            try
            {
                HttpClient client = _clientFactory.CreateClient(type);
                HttpResponseMessage response = await client.GetAsync(uri);
                string data = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return data;
                }
                else
                {
                    Console.WriteLine(response.StatusCode + "\nData: " + data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return string.Empty;
        }

        public async Task<UrlResponseByteContent> GetUrlContent(string url, string type)
        {
            byte[]? content = new byte[0];
            HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest;
            try
            {
                HttpClient client = _clientFactory.CreateClient(type);
                HttpResponseMessage response = await client.GetAsync(url);
                httpStatusCode = response.StatusCode;
                content = await response.Content.ReadAsByteArrayAsync();
                response.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new UrlResponseByteContent()
            {
                Content = content,
                HttpStatusCode = httpStatusCode
            };
        }

        public async Task PostRequestWithAuthToken(string discordURI, DiscordMessageRequest spamMessageContent, string type)
        {
            try
            {
                Dictionary<string, string> contentKeyValuePair = new Dictionary<string, string>();
                contentKeyValuePair.Add("content", spamMessageContent.Content);
                FormUrlEncodedContent content = new FormUrlEncodedContent(contentKeyValuePair);
                HttpClient client = _clientFactory.CreateClient(type);
                HttpResponseMessage response = await client.PostAsync(discordURI, content);
                if (!response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(response.StatusCode + "\nData: " + data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool IsSuccessStatusCode(HttpStatusCode httpStatusCode)
        {
            return ((int) httpStatusCode >= 200) && ((int) httpStatusCode <= 299);
        }
    }
}
