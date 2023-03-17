using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Vision;
using PokemonPredictor.Common;
using PokemonPredictor.Models;
using System.Diagnostics;
using System.Text;
using static Microsoft.ML.DataOperationsCatalog;

namespace PokemonPredictor
{
    public class Predictor
    {
        private MLContext _mlContext;
        public Predictor(MLContext context)
        {
            _mlContext = context;
        }

        public string PredictSingle(PredictionEngine<ModelInput, ModelOutput> predictionEngine, byte[]? content = null)
        {
            if (content == null || content.Length == 0)
            {
                return string.Empty;
            }
            ModelInput imageToPredict = new ModelInput
            {
                Image = content
            };

            ModelOutput prediction = predictionEngine.Predict(imageToPredict);
            return prediction.PredictedPokemonName;
        }

        public void TrainPokemonModel(bool removeFilesAndTrainModelAgain, bool trainModelAgain)
        {
            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../PokemonPredictor"));
            string workspaceRelativePath = Path.Combine(projectDirectory, "Workspace");
            string assetsInputRelativePath = Path.Combine(projectDirectory, @"Assets\Input");
            string assetsOutputRelativePath = Path.Combine(projectDirectory, @"Assets\Output");

            _mlContext.Log += MlContextLoggig;

            FileUtils.CreateDirectoryIfNotExists(new string[] { workspaceRelativePath, assetsInputRelativePath, assetsOutputRelativePath });

            if (removeFilesAndTrainModelAgain)
            {
                FileUtils.DeleteAllFiles(workspaceRelativePath);
                FileUtils.DeleteAllFiles(assetsOutputRelativePath);
            }

            string assetsOutputFilePath = Path.Combine(assetsOutputRelativePath, "trainedmodel.zip");

            if (!File.Exists(assetsOutputFilePath) || trainModelAgain)
            {
                TrainModel(_mlContext, assetsInputRelativePath, workspaceRelativePath, assetsOutputFilePath);
            }
        }

        private void TrainModel(MLContext mlContext, string assetsInputRelativePath, string workspaceRelativePath, string assetsOutputFilePath)
        {
            IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: assetsInputRelativePath);

            IDataView fullImagesDataset = mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledFullImageFilePathsDataset = mlContext.Data.ShuffleRows(fullImagesDataset);

            IDataView shuffledFullImagesDataset = mlContext.Transforms.Conversion.MapValueToKey("LabelAsKey", "Label")
                .Append(mlContext.Transforms.LoadRawImageBytes(
                                               outputColumnName: "Image",
                                               imageFolder: assetsInputRelativePath,
                                               inputColumnName: "ImagePath"))
               .Fit(shuffledFullImageFilePathsDataset)
               .Transform(shuffledFullImageFilePathsDataset);

            TrainTestData trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.20);

            IDataView trainDataset = trainTestData.TrainSet;
            IDataView testDataset = trainTestData.TestSet;

            ImageClassificationTrainer.Options options = GetImageClassificationTrainderOptions(testDataset, workspaceRelativePath);

            var pipeline = mlContext.MulticlassClassification.Trainers.
                ImageClassification(options)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedPokemonLabel", "PredictedLabel"));

            ITransformer trainedModel = pipeline.Fit(trainDataset);

            //ConsoleHelper.PrintPreview(trainedModel.Preview(testDataset, 10));

            mlContext.Model.Save(trainedModel, trainDataset.Schema, assetsOutputFilePath);

            EvaluateModel(mlContext, testDataset, trainedModel);

            #region For Testing different arch
            /*
            FileUtils.CreateDirectoryIfNotExists(new string[]
            {
                workspaceRelativePath + "\\ResnetV2101",
                workspaceRelativePath + "\\ResnetResnetV250",
                workspaceRelativePath + "\\InceptionV3",
                workspaceRelativePath + "\\MobilenetV2"
            });

            TrainWithResnetV2101(mlContext, workspaceRelativePath + "\\ResnetV2101", testDataset, trainDataset, trainedModelRelativePath);

            TrainWithResnetV250(mlContext, workspaceRelativePath + "\\ResnetResnetV250", testDataset, trainDataset, trainedModelRelativePath);

            TrainWithInceptionV3(mlContext, workspaceRelativePath + "\\InceptionV3", testDataset, trainDataset, trainedModelRelativePath);

            TrainWithMobilenetV2(mlContext, workspaceRelativePath + "\\MobilenetV2", testDataset, trainDataset, trainedModelRelativePath);
            */
            #endregion
        }

        private ImageClassificationTrainer.Options GetImageClassificationTrainderOptions(IDataView testDataset, string workspaceRelativePath)
        {
            return new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                Arch = ImageClassificationTrainer.Architecture.MobilenetV2,
                Epoch = 100,
                BatchSize = 15,
                LearningRate = 0.001f,
                MetricsCallback = (metrics) => Console.WriteLine(metrics),
                ValidationSet = testDataset,
                EarlyStoppingCriteria = null,
                ReuseTrainSetBottleneckCachedValues = true,
                ReuseValidationSetBottleneckCachedValues = true,
                FinalModelPrefix = "Pokemon_",
                WorkspacePath = workspaceRelativePath
            };
        }

        private void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel)
        {
            IDataView predictionsDataView = trainedModel.Transform(testDataset);
            MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName: "LabelAsKey", predictedLabelColumnName: "PredictedPokemonLabel");
            ConsoleHelper.PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);
        }

        private void MlContextLoggig(object? sender, LoggingEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }

        private IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
        {
            IEnumerable<string> files = FileUtils.GetAllFilesInDirectory(folder, new string[] { ".png", ".jpg" });
            foreach (string file in files)
            {
                string? label = Directory.GetParent(file)?.Name;
                string? parentPath = Directory.GetParent(file)?.FullName;
                int pokemonType = 1;
                if (parentPath != null)
                {
                    string? superParentName = Directory.GetParent(parentPath)?.Name;
                    if (superParentName != null && superParentName.Equals("Rare", StringComparison.CurrentCultureIgnoreCase))
                    {
                        pokemonType = 2;
                    }
                    else if (label != null && label.StartsWith("Shadow"))
                    {
                        pokemonType = 3;
                    }
                }
                if(label != null) 
                {
                    if (label.Equals("Mime Jr", StringComparison.InvariantCultureIgnoreCase))
                    {
                        label = "Mime Jr.";
                    }
                    else if (label.Equals("Type Null", StringComparison.InvariantCultureIgnoreCase))
                    {
                        label = "Type: Null";
                    }
                }
                label = string.Concat(label, "|", pokemonType);
                yield return new ImageData()
                {
                    ImagePath = file,
                    Label = label,
                };
            }
        }

        private void TrainWithInceptionV3(MLContext mlContext, string workspaceRelativePath, IDataView testDataset, IDataView trainDataset, string trainedModelRelativePath)
        {
            ImageClassificationTrainer.Options options = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                Arch = ImageClassificationTrainer.Architecture.InceptionV3,
                Epoch = 100,
                BatchSize = 15,
                LearningRate = 0.001f,
                MetricsCallback = (metrics) => Console.WriteLine(metrics),
                ValidationSet = testDataset,
                EarlyStoppingCriteria = null,
                ReuseTrainSetBottleneckCachedValues = true,
                ReuseValidationSetBottleneckCachedValues = true,
                FinalModelPrefix = "Pokemon_",
                WorkspacePath = workspaceRelativePath
            };
            TransformAndFit(mlContext, testDataset, trainDataset, trainedModelRelativePath, options);
        }

        private void TrainWithMobilenetV2(MLContext mlContext, string workspaceRelativePath, IDataView testDataset, IDataView trainDataset, string trainedModelRelativePath)
        {
            ImageClassificationTrainer.Options options = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                Arch = ImageClassificationTrainer.Architecture.MobilenetV2,
                Epoch = 100,
                BatchSize = 15,
                LearningRate = 0.001f,
                MetricsCallback = (metrics) => Console.WriteLine(metrics),
                ValidationSet = testDataset,
                EarlyStoppingCriteria = null,
                ReuseTrainSetBottleneckCachedValues = true,
                ReuseValidationSetBottleneckCachedValues = true,
                FinalModelPrefix = "Pokemon_",
                WorkspacePath = workspaceRelativePath
            };
            TransformAndFit(mlContext, testDataset, trainDataset, trainedModelRelativePath, options);
        }

        private void TrainWithResnetV250(MLContext mlContext, string workspaceRelativePath, IDataView testDataset, IDataView trainDataset, string trainedModelRelativePath)
        {
            ImageClassificationTrainer.Options options = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                Arch = ImageClassificationTrainer.Architecture.ResnetV250,
                Epoch = 100,
                BatchSize = 15,
                LearningRate = 0.001f,
                MetricsCallback = (metrics) => Console.WriteLine(metrics),
                ValidationSet = testDataset,
                EarlyStoppingCriteria = null,
                ReuseTrainSetBottleneckCachedValues = true,
                ReuseValidationSetBottleneckCachedValues = true,
                FinalModelPrefix = "Pokemon_",
                WorkspacePath = workspaceRelativePath
            };
            TransformAndFit(mlContext, testDataset, trainDataset, trainedModelRelativePath, options);
        }

        private void TrainWithResnetV2101(MLContext mlContext, string workspaceRelativePath, IDataView testDataset, IDataView trainDataset, string trainedModelRelativePath)
        {
            ImageClassificationTrainer.Options options = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
                Epoch = 100,
                BatchSize = 15,
                LearningRate = 0.001f,
                MetricsCallback = (metrics) => Console.WriteLine(metrics),
                ValidationSet = testDataset,
                EarlyStoppingCriteria = null,
                ReuseTrainSetBottleneckCachedValues = true,
                ReuseValidationSetBottleneckCachedValues = true,
                FinalModelPrefix = "Pokemon_",
                WorkspacePath = workspaceRelativePath
            };
            TransformAndFit(mlContext, testDataset, trainDataset, trainedModelRelativePath, options);
        }

        private void TransformAndFit(MLContext mlContext, IDataView testDataset, IDataView trainDataset, string trainedModelRelativePath, ImageClassificationTrainer.Options options)
        {
            var pipeline = mlContext.MulticlassClassification.Trainers.
                ImageClassification(options)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedPokemonLabel", "PredictedLabel"));

            Stopwatch watch = Stopwatch.StartNew();

            ITransformer trainedModel = pipeline.Fit(trainDataset);

            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;

            FileUtils.WriteToFile(trainedModelRelativePath + "\\metrices.txt", $"Training with transfer learning with {options.Arch} took: {elapsedMs / 1000} seconds").ConfigureAwait(false);

            Console.WriteLine($"Training with transfer learning took: {elapsedMs / 1000} seconds");

            //ConsoleHelper.PrintPreview(trainedModel.Preview(testDataset, 10));

            string modelFullPathName = $"{trainedModelRelativePath}\\{options.Arch}.model.zip";

            mlContext.Model.Save(trainedModel, trainDataset.Schema, modelFullPathName);

            EvaluateModel(mlContext, testDataset, trainedModel, options.Arch.ToString(), trainedModelRelativePath);
        }

        private void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel, string arch, string trainedModelRelativePath)
        {
            Stopwatch watch = Stopwatch.StartNew();

            IDataView predictionsDataView = trainedModel.Transform(testDataset);

            MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName: "LabelAsKey", predictedLabelColumnName: "PredictedPokemonLabel");
            ConsoleHelper.PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);

            string content = CreateContentToSaveInFile(metrics, arch);

            FileUtils.WriteToFile(trainedModelRelativePath + "\\metrices.txt", content).ConfigureAwait(false);

            watch.Stop();
            long elapsed2Ms = watch.ElapsedMilliseconds;

            Console.WriteLine($"Predicting and Evaluation took: {elapsed2Ms / 1000} seconds");
        }

        private string CreateContentToSaveInFile(MulticlassClassificationMetrics metrics, string arch)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"************************************************************");
            sb.AppendLine($"*    Metrics for {arch} multi-class classification model   ");
            sb.AppendLine($"*-----------------------------------------------------------");
            sb.AppendLine($"    AccuracyMacro = {metrics.MacroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            sb.AppendLine($"    AccuracyMicro = {metrics.MicroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            sb.AppendLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");
            return sb.ToString();
        }
    }
}