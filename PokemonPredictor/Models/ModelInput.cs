using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonPredictor.Models
{
    public class ModelInput
    {
        public byte[] Image;

        public UInt32 LabelAsKey;
    }
}
