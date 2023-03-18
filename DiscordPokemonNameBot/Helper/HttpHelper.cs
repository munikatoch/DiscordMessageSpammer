using Interfaces.Discord.Helper;
using Interfaces.Logger;
using Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DiscordPokemonNameBot.Helper
{
    public class HttpHelper : IHttpHelper
    {
        private IHttpClientFactory _clientFactory;
        private IAppLogger _appLogger;

        public HttpHelper(IHttpClientFactory clientFactory, IAppLogger appLogger)
        {
            _clientFactory = clientFactory;
            _appLogger = appLogger;
        }

        public async Task<byte[]?> GetImageContent(string url, string type)
        {
            byte[]? content = new byte[0];

            if (ValidateAndParseUrl(url, out Uri? uri))
            {
                using (HttpClient client = _clientFactory.CreateClient(type))
                {
                    using (HttpResponseMessage response = await client.GetAsync(uri))
                    {
                        if(response.IsSuccessStatusCode) 
                        {
                            content = await response.Content.ReadAsByteArrayAsync();
                        }
                        else
                        {
                            string message = await CreateHttpUnsuccessLogMessage(response);
                            _appLogger.FileLogger("Http/Unsuccess", message);
                        }
                    }
                }
            }
            return content;
        }

        public async Task FormUrlEcodedContentPostRequest<T>(string url, T request, string type, Dictionary<string, string>? header = null) where T : notnull
        {
            if(ValidateAndParseUrl(url, out Uri? uri)) 
            {
                Dictionary<string, string?> contentKeyValuePair = request.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(request)?.ToString());
                FormUrlEncodedContent content = new FormUrlEncodedContent(contentKeyValuePair);
                using (HttpClient client = _clientFactory.CreateClient(type))
                {
                    if (header != null && header.Count > 0)
                    {
                        AddOrUpdateHeader(client, header);
                    }
                    using (HttpResponseMessage response = await client.PostAsync(uri, content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            string message = await CreateHttpUnsuccessLogMessage(response);
                            _appLogger.FileLogger("Http/Unsuccess", message);
                        }
                    }
                }
            }
        }

        public async Task<T?> GetRequest<T>(string url, string type) where T : notnull
        {
            T? result;
            if(ValidateAndParseUrl(url, out Uri? uri))
            {
                using (HttpClient client = _clientFactory.CreateClient(type))
                {
                    using (HttpResponseMessage response = await client.GetAsync(uri))
                    {
                        if (response.IsSuccessStatusCode) 
                        {
                            string data = await response.Content.ReadAsStringAsync();
                            result = JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions()
                            {
                                PropertyNameCaseInsensitive = false
                            });
                            return result;
                        }
                        else
                        {
                            string message = await CreateHttpUnsuccessLogMessage(response);
                            _appLogger.FileLogger("Http/Unsuccess", message);
                        }
                    }
                }
            }
            return default(T);
        }

        private void AddOrUpdateHeader(HttpClient client, Dictionary<string, string> header)
        {
            foreach (KeyValuePair<string, string> keyValuePair in header)
            {
                if (client.DefaultRequestHeaders.Contains(keyValuePair.Key))
                {
                    client.DefaultRequestHeaders.Remove(keyValuePair.Key);
                }
                client.DefaultRequestHeaders.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        private bool ValidateAndParseUrl(string url, out Uri? uri)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out uri) && uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private async Task<string> CreateHttpUnsuccessLogMessage(HttpResponseMessage response)
        {
            string httpResponse = await response.Content.ReadAsStringAsync();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Status Code: {response.StatusCode}");

            if(response.RequestMessage?.RequestUri?.AbsolutePath != null)
            {
                sb.AppendLine($"Url: {response.RequestMessage?.RequestUri?.AbsolutePath}");
            }
            sb.AppendLine($"Response: {httpResponse}");
            sb.AppendLine(Constants.EOFMarkup);
            return sb.ToString();
        }
    }
}
