﻿using System;
using SiaNet;
using SiaNet.Data;
using SiaNet.Initializers;
using SiaNet.Layers;
using TensorSharp;
using TensorSharp.Expression;

namespace MNIST
{
    class Program
    {
        static void Main(string[] args)
        {
            Global.UseGpu();

            Tensor x = Tensor.FromArray(Global.Device, new float[] { 3, 4, 5, 7, 2 });
            
            TOps.Diag(x).Print();

            string datasetFolder = @"C:\dataset\MNIST";

            var ((trainX, trainY), (valX, valY)) = MNISTParser.LoadDataSet(datasetFolder, trainCount: 6000, testCount: 1000, flatten: true);
            Console.WriteLine("Train and Test data loaded");
            DataFrameIter trainIter = new DataFrameIter(trainX, trainY);
            DataFrameIter valIter = new DataFrameIter(valX, valY);

            Sequential model = BuildFCModel();

            model.Compile(OptimizerType.Adam, LossType.CategorialCrossEntropy, MetricType.Accuracy);
            Console.WriteLine("Model compiled.. initiating training");

            model.EpochEnd += Model_EpochEnd;

            model.Train(trainIter, 10, 200, valIter);

            Console.ReadLine();
        }

        private static Sequential BuildFCModel()
        {
            Sequential model = new Sequential();
            model.Add(new Dense(dim: 784, activation: ActType.ReLU, kernalInitializer: new Ones()));
            model.Add(new Dense(dim: 10, activation: ActType.Softmax, kernalInitializer: new Ones()));

            return model;
        }

        private static Sequential BuildConvModel()
        {
            Sequential model = new Sequential();
            model.Add(new Conv2D(filters: 30, kernalSize: Tuple.Create<uint, uint>(5, 5), activation: ActType.ReLU));
            model.Add(new MaxPooling2D(poolSize: Tuple.Create<uint, uint>(2, 2)));
            model.Add(new Conv2D(filters: 15, kernalSize: Tuple.Create<uint, uint>(3, 3), activation: ActType.ReLU));
            model.Add(new MaxPooling2D(poolSize: Tuple.Create<uint, uint>(2, 2)));
            model.Add(new Dropout(0.2f));
            model.Add(new Flatten());
            model.Add(new Dense(dim: 128, activation: ActType.ReLU));
            model.Add(new Dense(dim: 50, activation: ActType.ReLU));
            model.Add(new Dense(dim: 10, activation: ActType.Softmax));

            return model;
        }

        private static void Model_EpochEnd(object sender, EpochEndEventArgs e)
        {
            if(e.ValidationLoss > 0)
                Console.WriteLine("Epoch: {0}, Samples/sec: {1}, Loss: {2}, Acc: {3}, Val_Loss: {4}, Val_Acc: {5}, Elapse: {6}", e.Epoch, e.SamplesSeen, e.Loss, e.Metric, e.ValidationLoss, e.ValidationMetric, e.Duration);
            else
                Console.WriteLine("Epoch: {0}, Samples/sec: {1}, Loss: {2}, Acc: {3}, Elapse: {4}", e.Epoch, e.SamplesSeen, e.Loss, e.Metric, e.Duration);
        }
    }
}