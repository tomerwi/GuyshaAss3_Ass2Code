using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using RecommenderSystem.Infrastructure;
//using RecommenderSystem.MovieLens;
//using RecommenderSystem.Algorithms;
//using RecommenderSystem.EvaluationMetrics;

namespace RecommenderSystem
{
    class RecommenderSystem
    {
        public enum PredictionMethod { Pearson, Cosine, Random, BaseModel, Stereotypes };
        public enum RecommendationMethod { Popularity, Pearson, Cosine, BaseModel, Stereotypes, NNPearson, NNCosine, NNBaseModel, NNJaccard, CP, Jaccard };

        //class members here
        public Ass2RecommenderSystem RecSys;

        public RecommenderSystem()
        {
            RecSys = new Ass2RecommenderSystem();
        }

        public void Load(string sFileName)
        {
            throw new NotImplementedException();
        }
        public void Load(string sFileName, double dTrainSetSize)
        {
            throw new NotImplementedException();
        }

        public void TrainBaseModel(int cFeatures)
        {
            throw new NotImplementedException();
        }
        public void TrainStereotypes(int cStereotypes)
        {
            throw new NotImplementedException();
        }

        public double GetRating(string sUID, string sIID)
        {
            throw new NotImplementedException();
        }

        public double PredictRating(PredictionMethod m, string sUID, string sIID)
        {
            throw new NotImplementedException();
        }

        public Dictionary<PredictionMethod, double> ComputeMAE(List<PredictionMethod> lMethods, int cTrials)
        {
            throw new NotImplementedException();
        }

        public Dictionary<PredictionMethod, double> ComputeRMSE(List<PredictionMethod> lMethods, int cTrials)
        {
            throw new NotImplementedException();
        }
        public List<string> Recommend(RecommendationMethod sAlgorithm, string sUserId, int cRecommendations)
        {
            throw new NotImplementedException();
        }
        private void calcPopularity() // func that will be called only once to calc the popularity - will generate a list of all the items that will be orderd accourding to the popularity (high->low)
        {

        }
        //               length           algorithm              precision/recall   result        ---> i think...
        public Dictionary<int, Dictionary<RecommendationMethod, Dictionary<string, double>>> ComputePrecisionRecall(List<RecommendationMethod> lMethods, List<int> lLengths, int cTrials)
        {
            throw new NotImplementedException();
        }

    }
}
