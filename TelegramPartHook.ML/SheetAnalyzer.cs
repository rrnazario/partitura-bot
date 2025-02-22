using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using TelegramPartHook.Domain.SeedWork;
using static Microsoft.ML.Transforms.ValueToKeyMappingEstimator;

namespace TelegramPartHook.ML
{
    public class ImageData(string imagePath, string label)
    {
        public readonly string ImagePath = imagePath;
        public readonly string Label = label;
    }

    public class ImagePrediction
    {
        public string? PredictedLabel { get; set; }
        public float[]? Score { get; set; }
    }

    public class InMemoryImageData(byte[] image, string label, string imageFileName)
    {
        public readonly byte[] Image = image;
        public readonly string Label = label;
        public readonly string ImageFileName = imageFileName;
    }

    public enum ImageClassification
    {
        Sheet,
        NoSheet
    }

    public interface ISheetAnalyzer
    {
        (ImageClassification classification, float score) Predict(SheetSearchResult result);
        void GenerateModel();

    }
    public class SheetAnalyzer : ISheetAnalyzer, IDisposable
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<InMemoryImageData, ImagePrediction> _predictionEngine;

        public SheetAnalyzer()
        {
            _mlContext = new MLContext(seed: 1);
            
            var model = _mlContext.Model.Load(Path.Combine(AppContext.BaseDirectory, "results.zip"), out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<InMemoryImageData, ImagePrediction>(model);
        }

        public (ImageClassification classification, float score) Predict(SheetSearchResult result)
        {
            var (fileInfo, bytes) = GetFileContent(result);

            var imageToPredict = new InMemoryImageData(bytes, "sheets", fileInfo.Name);

            var prediction = _predictionEngine.Predict(imageToPredict);

            var maxScore = prediction.Score!.Max();

            return (prediction.PredictedLabel == "sheets" ? ImageClassification.Sheet : ImageClassification.NoSheet, maxScore);
        }

        public void GenerateModel()
        {
            var (trainedModel, trainedDataSet) = TrainModel();

            // Save the model to assets/outputs (You get ML.NET .zip model file and TensorFlow .pb model file)
            _mlContext.Model.Save(trainedModel, trainedDataSet.Schema, "results.zip");
        }

        private (FileInfo fileinfo, byte[] content) GetFileContent(SheetSearchResult result)
        {
            var filename = "";

            if (!result.Exists)
            {
                if (result.Source == Domain.Constants.Enums.FileSource.Dropbox)
                {
                    //await _dropboxHelper.DownloadFileAsync(result);
                    filename = result.LocalPath;
                }
                else
                {
                    //filename = await _systemHelper.DownloadFileAsync(result.Address);
                }
            }
            else filename = result.LocalWhereExists;

            var fileInfo = new FileInfo(filename);
            var bytes = File.ReadAllBytes(filename);
            return (fileInfo, bytes);
        }


        private (EstimatorChain<KeyToValueMappingTransformer> pipeline, IDataView trainDataView) BuildModel()
        {
            // 1. Download the image set and unzip
            var fullImagesetFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "images");

            // 2. Load the initial full image-set into an IDataView and shuffle so it'll be better balanced
            IEnumerable<ImageData> images = LoadImagesFromDirectory(fullImagesetFolderPath);
            IDataView fullImagesDataset = _mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledFullImageFilePathsDataset = _mlContext.Data.ShuffleRows(fullImagesDataset);

            // 3. Load Images with in-memory type within the IDataView and Transform Labels to Keys (Categorical)
            IDataView shuffledFullImagesDataset = _mlContext.Transforms.Conversion.
                    MapValueToKey(outputColumnName: "LabelAsKey", inputColumnName: "Label", keyOrdinality: KeyOrdinality.ByValue)
                .Append(_mlContext.Transforms.LoadRawImageBytes(
                                                outputColumnName: "Image",
                                                imageFolder: fullImagesetFolderPath,
                                                inputColumnName: "ImagePath"))
                .Fit(shuffledFullImageFilePathsDataset)
                .Transform(shuffledFullImageFilePathsDataset);

            // 4. Split the data 80:20 into train and test sets, train and evaluate.
            var trainTestData = _mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
            IDataView trainDataView = trainTestData.TrainSet;
            IDataView testDataView = trainTestData.TestSet;

            // 5. Define the model's training pipeline using DNN default values
            var pipeline = _mlContext.MulticlassClassification.Trainers
                    .ImageClassification(featureColumnName: "Image",
                                            labelColumnName: "LabelAsKey",
                                            validationSet: testDataView)
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel",
                                                                      inputColumnName: "PredictedLabel"));

            return (pipeline, trainDataView);
        }

        private (ITransformer, IDataView) TrainModel()
        {
            var (pipeline, trainDataView) = BuildModel();

            // 4. Train/create the ML model
            return (pipeline.Fit(trainDataView), trainDataView);
        }

        private IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
        {
            var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Select(s => new FileInfo(s));

            var result = new List<ImageData>();
            foreach (var file in files)
                result.Add(new ImageData(file.FullName, file.Directory!.Name));

            return result;
        }

        public void Dispose()
        {
            _predictionEngine?.Dispose();
        }
    }
}