using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonPredictor.Models
{
    public class LogMessageModel
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public string ResponseContent { get; set; }
        public string? StackTrace { get; set; }
        public string? ExceptionType { get; set; }
    }
}
