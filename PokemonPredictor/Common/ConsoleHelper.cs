﻿using Microsoft.ML;
using Microsoft.ML.Data;

namespace PokemonPredictor.Common
{
    public class ConsoleHelper
    {
        public static void PrintMultiClassClassificationMetrics(string name, MulticlassClassificationMetrics metrics)
        {
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*    Metrics for {name} multi-class classification model   ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"    AccuracyMacro = {metrics.MacroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    AccuracyMicro = {metrics.MicroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");

            int i = 0;
            foreach (var classLogLoss in metrics.PerClassLogLoss)
            {
                i++;
                Console.WriteLine($"    LogLoss for class {i} = {classLogLoss:0.####}, the closer to 0, the better");
            }
            Console.WriteLine($"************************************************************");
        }

        public static void PrintSchema(DataViewSchema schema)
        {
            foreach (var column in schema)
            {
                Console.WriteLine($"Name: {column.Name} of Type: {column.Type}");
            }
        }

        public static void PrintPreview(DataDebuggerPreview dataDebuggerPreview)
        {
            foreach (var row in dataDebuggerPreview.RowView)
            {
                foreach (var element in row.Values)
                {
                    Console.WriteLine($"Key: {element.Key}, Value: {element.Value}");
                }
                Console.WriteLine();
            }
        }
    }
}
