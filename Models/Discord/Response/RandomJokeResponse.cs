using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.Discord.Response
{
    public class RandomJokeResponse
    {
        public bool Error { get; set; }
        public string Category { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JokeType Type { get; set; }
        public string Setup { get; set; }
        public string Delivery { get; set; }
        public string Joke { get; set; }
        public Flag Flags { get; set; }
        public int Id { get; set; }
        public bool Safe { get; set; }
        public string Lang { get; set; }
    }

    public enum JokeType 
    {
        TwoPart,
        Single
    }

    public class Flag
    {
        public bool Nsfw { get; set; }
        public bool Religious { get; set; }
        public bool Political { get; set; }
        public bool Racist { get; set; }
        public bool Sexist { get; set; }
        public bool Explicit { get; set; }
    }
}
