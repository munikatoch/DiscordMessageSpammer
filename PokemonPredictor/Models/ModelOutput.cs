using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonPredictor.Models
{
    public class ModelOutput
    {
        [ColumnName("Score")]
        public float[] Score;

        [ColumnName("PredictedPokemonLabel")]
        public string PredictedPokemonName;
    }
}
