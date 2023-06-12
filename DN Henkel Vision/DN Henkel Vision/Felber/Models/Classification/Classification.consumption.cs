﻿// This file was auto-generated by ML.NET Model Builder.
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using DN_Henkel_Vision.Memory;

namespace DN_Henkel_Vision
{
    public partial class Classification
    {
        /// <summary>
        /// model input class for Classification.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [LoadColumn(0)]
            [ColumnName(@"col0")]
            public string Col0 { get; set; }

            [LoadColumn(1)]
            [ColumnName(@"col1")]
            public string Col1 { get; set; }

            [LoadColumn(2)]
            [ColumnName(@"col2")]
            public string Col2 { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for Classification.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"col0")]
            public float[] Col0 { get; set; }

            [ColumnName(@"col1")]
            public float[] Col1 { get; set; }

            [ColumnName(@"col2")]
            public uint Col2 { get; set; }

            [ColumnName(@"Features")]
            public float[] Features { get; set; }

            [ColumnName(@"PredictedLabel")]
            public string PredictedLabel { get; set; }

            [ColumnName(@"Score")]
            public float[] Score { get; set; }

        }

        #endregion

        private static string MLNetModelPath = Drive.Folder + @"\..\DN Henkel Vision\Felber\Models\Classification\Classification.zip";

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }
    }
}
