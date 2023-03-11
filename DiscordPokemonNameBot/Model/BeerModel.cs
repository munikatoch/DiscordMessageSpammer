using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordPokemonNameBot.Model
{
    public class BeerModel
    {
        public string ID { get; set; }
        public string UID { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public string Style { get; set; }
        public string Hop { get; set; }
        public string Yeast { get; set; }
        public string Malts { get; set; }
        public string Ibu { get; set; }
        public string Alcohol { get; set; }
        public string Blg { get; set; }

        public override string ToString()
        {
            return "------------------------------------------------------------------\n" +
                $"Beer :beer: Info Start\n" +
                $"**Beer Brand**: {Brand}\n" +
                $"**Beer Name**: {Name}\n" +
                $"**Beer Alcohol Percentage**: {Alcohol}\n" +
                $"**Beer Style**: {Style}\n" +
                $"Beer :beer: Info End\n" +
                "------------------------------------------------------------------";
        }
    }
}
