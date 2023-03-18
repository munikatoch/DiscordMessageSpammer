using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.Discord.Response
{
    public class RandomQuoteResponse
    {
        [JsonPropertyName("q")]
        public string Quote { get; set; }

        [JsonPropertyName("a")]
        public string Author { get; set; }

        [JsonPropertyName("h")]
        public string HtmlQuote { get; set; }
    }
}
