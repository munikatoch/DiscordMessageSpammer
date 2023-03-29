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
                try
                {
                    using (HttpClient client = _clientFactory.CreateClient(type))
                    {
                        using (HttpResponseMessage response = await client.GetAsync(uri))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                if(response.Content.Headers.ContentType != null)
                                {
                                    string contentType = response.Content.Headers.ContentType.ToString();
                                    if (contentType.StartsWith("image")) 
                                    {
                                        content = await response.Content.ReadAsByteArrayAsync();
                                    }
                                }
                                else
                                {
                                    string message = $"{url} is not a image url";
                                    _appLogger.FileLogger("Http/Unsuccess", message);
                                }
                            }
                            else
                            {
                                string message = await LogMessageBuilder.CreateHttpUnsuccessLogMessage(response);
                                _appLogger.FileLogger("Http/Unsuccess", message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _appLogger.ExceptionLog("Http", ex);
                }
            }
            return content;
        }

        public async Task FormUrlEcodedContentPostRequest<T>(string url, T request, string type, Dictionary<string, string>? header = null) where T : notnull
        {
            if(ValidateAndParseUrl(url, out Uri? uri)) 
            {
                Dictionary<string, string?> contentKeyValuePair = request.GetType().GetProperties().ToDictionary(x => x.Name.ToLower(), x => x.GetValue(request)?.ToString());
                FormUrlEncodedContent content = new FormUrlEncodedContent(contentKeyValuePair); //content
                try
                {
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
                                string message = await LogMessageBuilder.CreateHttpUnsuccessLogMessage(response);
                                _appLogger.FileLogger("Http/Unsuccess", message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _appLogger.ExceptionLog("Http", ex);
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
                    using (HttpClient client = _clientFactory.CreateClient(type))
                    {
                        using (HttpResponseMessage response = await client.GetAsync(uri))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string data = await response.Content.ReadAsStringAsync();


                                if (TypeUtil.IsJson(data))
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
                                string message = await LogMessageBuilder.CreateHttpUnsuccessLogMessage(response);
                                _appLogger.FileLogger("Http/Unsuccess", message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _appLogger.ExceptionLog("Http", ex);
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
    }
}
