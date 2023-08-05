using Common;
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
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAppLogger _appLogger;

        public HttpHelper(IHttpClientFactory clientFactory, IAppLogger appLogger)
        {
            _clientFactory = clientFactory;
            _appLogger = appLogger;
        }

        public async Task<byte[]?> GetImageContent(string url, string type)
        {
            byte[]? content = Array.Empty<byte>();

            if (ValidateAndParseUrl(url, out Uri? uri))
            {
                try
                {
                    using HttpClient client = _clientFactory.CreateClient(type);
                    using HttpResponseMessage response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.Content.Headers.ContentType != null)
                        {
                            string contentType = response.Content.Headers.ContentType.ToString();
                            if (contentType.StartsWith("image"))
                            {
                                content = await response.Content.ReadAsByteArrayAsync();
                            }
                        }
                        else
                        {
                            await _appLogger.FileLogger(response).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await _appLogger.FileLogger(response).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await _appLogger.ExceptionLog($"HttpHelper: GetImageContent {url}", ex).ConfigureAwait(false);
                }
            }
            return content;
        }

        public async Task FormUrlEcodedContentPostRequest<T>(string url, T request, string type, Dictionary<string, string>? header = null) where T : notnull
        {
            if(ValidateAndParseUrl(url, out Uri? uri)) 
            {
                Dictionary<string, string?> contentKeyValuePair = request.GetType().GetProperties().ToDictionary(x => x.Name.ToLower(), x => x.GetValue(request)?.ToString());
                FormUrlEncodedContent content = new FormUrlEncodedContent(contentKeyValuePair);
                try
                {
                    using HttpClient client = _clientFactory.CreateClient(type);
                    if (header != null && header.Count > 0)
                    {
                        AddOrUpdateHeader(client, header);
                    }
                    using HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        await _appLogger.FileLogger(response).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await _appLogger.ExceptionLog($"HttpHelper: FormUrlEcodedContentPostRequest {url}", ex).ConfigureAwait(false);
                }
            }
        }

        public async Task<T?> GetRequest<T>(string url, string type) where T : notnull
        {
            T? result;
            if(ValidateAndParseUrl(url, out Uri? uri))
            {
                try
                {
                    using HttpClient client = _clientFactory.CreateClient(type);
                    using HttpResponseMessage response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();

                        string contentType = string.Empty;

                        if (response.Content.Headers.ContentType?.MediaType != null)
                        {
                            contentType = response.Content.Headers.ContentType.MediaType.ToString();
                        }
                        if (contentType.Equals("application/json"))
                        {
                            result = JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions()
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return result;
                        }
                        else
                        {
                            return (T)Convert.ChangeType(data, typeof(T));
                        }
                    }
                    else
                    {
                        await _appLogger.FileLogger(response).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await _appLogger.ExceptionLog($"HttpHelper: GetRequest {url}", ex).ConfigureAwait(false);
                }
            }
            return default;
        }

        private static void AddOrUpdateHeader(HttpClient client, Dictionary<string, string> header)
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

        private static bool ValidateAndParseUrl(string url, out Uri? uri)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out uri) && uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
