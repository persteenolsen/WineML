﻿using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML.Trainers.FastTree;

namespace WineML
{
    class Program
    {
        private static string[] _features = new[] {
            nameof(WineData.FixedAcidity),
            nameof(WineData.VolatileAcidity),
            nameof(WineData.CitricAcid),
            nameof(WineData.ResidualSugar),
            nameof(WineData.Chlorides),
            nameof(WineData.FreeSulfurDioxide),
            nameof(WineData.TotalSulfurDioxide),
            nameof(WineData.Density),
            nameof(WineData.Ph),
            nameof(WineData.Sulphates),
            nameof(WineData.Alcohol)
        };

        static void Main(string[] args)
        {
            PrintHeader("Tasting wine with ML.NET");

            var mlContext = new MLContext();

            Console.Write("Load training data...");
            var trainingData = LoadData(mlContext, "winequality-white-train.csv");
            Console.WriteLine("DONE!");

            Console.Write("Load validation data...");
            var validationData = LoadData(mlContext, "winequality-white-validate.csv");
            Console.WriteLine("DONE!");

            Console.WriteLine("\r\n");


            Console.WriteLine("**** TRAIN AND EVALUATE MODEL WITH ALL FEATURES *****");

            // Only while testing / learning
            // Note: Both model training and validation are done by this method
            //TrainAndEvaluateTest(mlContext, trainingData, validationData);

            // Note: The Training is done by another method call CreateModel
            TrainAndValidate(mlContext, trainingData, validationData);

            Console.WriteLine("\r\n");

            Console.WriteLine("**** PREDICT QUALITY OF THE SELECTED WINE *****");

            // Only  while testing / learning
            // Note: The model training are done in this method
            /*PredictTest(mlContext, trainingData, new WineData
            {
                FixedAcidity = 7.6F,
                VolatileAcidity = 0.17F,
                CitricAcid = 0.27F,
                ResidualSugar = 4.6F,
                Chlorides = 0.05F,
                FreeSulfurDioxide = 23,
                TotalSulfurDioxide = 98,
                Density = 0.99422F,
                Ph = 3.08F,
                Sulphates = 0.47F,
                Alcohol = 9.5F,
                Quality = 0 // We are gonna predict this. The expected value is 6
            }); 

            */

            // The model training is done by CreateModel
            Predict(mlContext, trainingData, new WineData {
                 FixedAcidity = 7.6F,
                 VolatileAcidity = 0.17F,
                 CitricAcid = 0.27F,
                 ResidualSugar = 4.6F,
                 Chlorides = 0.05F,
                 FreeSulfurDioxide = 23,
                 TotalSulfurDioxide = 98,
                 Density = 0.99422F,
                 Ph = 3.08F,
                 Sulphates = 0.47F,
                 Alcohol = 9.5F,
                 Quality = 0 // We are gonna predict this. The expected value is 6
             }); 

            Console.WriteLine("\r\n");


            Console.WriteLine("**** FIND THE BEST FIT TO FIND A GOOD WINE  *****");
            FindBestFit(mlContext, trainingData, validationData);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static IDataView LoadData(MLContext mlContext, string fileName)
        {
            var dataPath = Path.Combine(Environment.CurrentDirectory, "Data", fileName);
            return mlContext.Data.LoadFromTextFile<WineData>(dataPath, separatorChar: ';', hasHeader: true);
        }

        // Only while testing / learning
        // Testing: Showing the code of training directly - more clear to read but duplicate code !
        /*
        private static void TrainAndEvaluateTest(MLContext mlContext, IDataView trainingData, IDataView validationData)
        {
            Console.Write("Train model - Test...");

            // Note: Instead of calling the method CreateModel
            // Transform colums, train the model with the regression algoritme using Fit
            var model = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(WineData.Quality))
                        .Append(mlContext.Transforms.Concatenate("Features", _features))
                        .Append(mlContext.Regression.Trainers.FastTree())
                        .Fit(trainingData);

            Console.WriteLine("DONE!");


            Console.Write("Evaluate model - Test...");
            var predictions = model.Transform(validationData);
            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");
            Console.WriteLine("DONE!");

            Console.WriteLine($"RSquared Score: {metrics.RSquared:0.##}");
            Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError:#.##}");
        }
        */

        // Only while testing / learning
        // Testing: Showing the code of training directly - more clear to read but duplicate code !
        /*
        private static void PredictTest(MLContext mlContext, IDataView trainingData, WineData wineData)
        {
            Console.Write("Train model - Test...");

            // Note: Instead of calling the method CreateModel
            // Transform colums, train the model with the regression algoritme using Fit
            var model = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(WineData.Quality))
                        .Append(mlContext.Transforms.Concatenate("Features", _features))
                        .Append(mlContext.Regression.Trainers.FastTree())
                        .Fit(trainingData);

            Console.WriteLine("DONE!");

            Console.Write("Predicting quality - Test...");
            var predictionFunction = mlContext.Model.CreatePredictionEngine<WineData, WinePrediction>(model);
            var prediction = predictionFunction.Predict(wineData);
            Console.WriteLine($"{prediction.Quality:0.##}");
        }
        */



        private static void TrainAndValidate(MLContext mlContext, IDataView trainingData, IDataView validationData)
        {
            Console.Write("Train model...");
            var model = CreateModel(mlContext, trainingData, _features);
            Console.WriteLine("DONE!");

            Console.Write("Validate model...");
            var predictions = model.Transform(validationData);
            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");
            Console.WriteLine("DONE!");

            Console.WriteLine($"RSquared Score: {metrics.RSquared:0.##}");
            Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError:#.##}");
        }


        private static void Predict(MLContext mlContext, IDataView trainingData, WineData wineData)
        {
            Console.Write("Train model...");
            var model = CreateModel(mlContext, trainingData, _features);
            Console.WriteLine("DONE!");

            Console.Write("Predicting quality...");
            var predictionFunction = mlContext.Model.CreatePredictionEngine<WineData, WinePrediction>(model);
            var prediction = predictionFunction.Predict(wineData);
            Console.WriteLine($"{prediction.Quality:0.##}");
        }
        


        private static void FindBestFit(MLContext mlContext, IDataView trainingData, IDataView validationData)
        {
            var rSquarePerFeature = new Dictionary<string, double>();
            
            foreach (var feature in _features)
            {
                Console.Write($"Calculate RSquared for {feature}... ");

                var model = CreateModel(mlContext, trainingData, feature);
                var predictions = model.Transform(validationData);
                var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

                rSquarePerFeature.Add(feature, metrics.RSquared);
                Console.WriteLine($"{metrics.RSquared:0.##}");
            }

            var bestFit = rSquarePerFeature.OrderByDescending(f => f.Value).First();
            Console.WriteLine($"Best fit for finding a good wine: {bestFit.Key} with a RSquared of {bestFit.Value:0.##}");
        }

        
        // This method is called by "var model = CreateModel( a,b,c )" from the methods below
        // 1: TrainAndValidate
        // 2: Predict
        // 3: FindBestFit
        private static TransformerChain<RegressionPredictionTransformer<FastTreeRegressionModelParameters>> CreateModel(MLContext mlContext, IDataView trainingData, params string[] features)
        {
            //Console.WriteLine("\r\n");
            //Console.Write(" The methond TransformerChain was called ... ");
            //Console.WriteLine("\r\n");

            return mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(WineData.Quality))
                       .Append(mlContext.Transforms.Concatenate(
                           "Features",
                           features
                       ))
                       .Append(mlContext.Regression.Trainers.FastTree())
                       .Fit(trainingData);
        }

        private static void PrintHeader(string title)
        {
            var originalForegroundColor = Console.ForegroundColor;
            var originalBackgroundColor = Console.BackgroundColor;
            
            var padding = (Console.WindowWidth - title.Length) / 2;

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.BackgroundColor = ConsoleColor.White;

            Console.WriteLine(".".PadLeft(Console.WindowWidth - 1, ' '));
            Console.WriteLine(".".PadLeft(padding, ' ') + title.ToUpper() + "".PadLeft(padding - 1, ' '));
            Console.WriteLine("".PadLeft(Console.WindowWidth - 1, ' '));

            Console.ForegroundColor = originalForegroundColor;
            Console.BackgroundColor = originalBackgroundColor;
        }
    }
}
