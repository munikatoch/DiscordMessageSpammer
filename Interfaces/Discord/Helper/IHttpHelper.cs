using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Discord.Helper
{
    public interface IHttpHelper
    {
        Task<byte[]?> GetImageContent(string url, string type);
        Task FormUrlEcodedContentPostRequest<T>(string url, T request, string type, Dictionary<string, string>? header = null) where T : notnull;
        Task<T?> GetRequest<T>(string url, string type) where T : notnull;
    }
}
