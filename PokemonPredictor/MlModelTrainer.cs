using Common;
using Interfaces.Logger;
using Interfaces.MlTrainer;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Vision;
using Models;
using Models.MlModelTrainer;
using PokemonPredictor.Common;
using static Microsoft.ML.DataOperationsCatalog;

namespace PokemonPredictor
{
    public class MlModelTrainer : IMlModelTrainer
    {
        private MLContext _mlContext;
        private IAppLogger _appLoger;

        public MlModelTrainer(MLContext context, IAppLogger appLoger)
        {
            _mlContext = context;
            _appLoger = appLoger;
        }

        public void TrainerModel(bool isDeleteWorkspaceAndModel, bool isModelTrainAgain)
        {
            FileUtils.CreateDirectoryIfNotExists(new string[]
            {
                Constants.MlModelAssestsInputRelativePath,
                Constants.MlModelAssestsOutputRelativePath,
                Constants.MlModelWorkSpaceRelativePath
            });

            _mlContext.Log += MlContextLoggig;

            if (isDeleteWorkspaceAndModel)
            {
                FileUtils.DeleteAllFiles(Constants.MlModelWorkSpaceRelativePath);
                FileUtils.DeleteAllFiles(Constants.MlModelAssestsOutputRelativePath);
            }

            if (!File.Exists(Constants.MlModelFileOutputPath) || isModelTrainAgain)
            {
                TrainModel();
            }
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
            return prediction.PredictedPokemonLabel;
        }

        private void TrainModel()
        {
            IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: Constants.MlModelAssestsInputRelativePath);

            IDataView fullImagesDataset = _mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledFullImageFilePathsDataset = _mlContext.Data.ShuffleRows(fullImagesDataset);

            IDataView shuffledFullImagesDataset = _mlContext.Transforms.Conversion.MapValueToKey("LabelAsKey", "Label")
                .Append(_mlContext.Transforms.LoadRawImageBytes(
                                               outputColumnName: "Image",
                                               imageFolder: Constants.MlModelAssestsInputRelativePath,
                                               inputColumnName: "ImagePath"))
               .Fit(shuffledFullImageFilePathsDataset)
               .Transform(shuffledFullImageFilePathsDataset);

            TrainTestData trainTestData = _mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.20);

            IDataView trainDataset = trainTestData.TrainSet;
            IDataView testDataset = trainTestData.TestSet;

            ImageClassificationTrainer.Options options = GetImageClassificationTrainderOptions(testDataset, Constants.MlModelWorkSpaceRelativePath);

            var pipeline = _mlContext.MulticlassClassification.Trainers.
                ImageClassification(options)
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedPokemonLabel", "PredictedLabel"));

            ITransformer trainedModel = pipeline.Fit(trainDataset);

            _mlContext.Model.Save(trainedModel, trainDataset.Schema, Constants.MlModelFilePath);

            EvaluateModel(_mlContext, testDataset, trainedModel);
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
                MetricsCallback = (metrics) => _appLoger.ConsoleLogger(metrics.ToString()),
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
            string logMessage = $"Source: {e.Source} || Message: {e.Message} || RawMessage: {e.RawMessage}";
            if (sender != null)
            {
                logMessage += $" || Sender: {sender}";
            }
            _appLoger.FileLogger("MlModel", logMessage);
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
                if (label != null)
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
    }
}