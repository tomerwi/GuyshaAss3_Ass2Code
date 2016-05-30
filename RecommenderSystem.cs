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
    class RecommenderSystem : Ass2RecommenderSystem
    {
        //public enum PredictionMethod { Pearson, Cosine, Random, BaseModel, Stereotypes };
        public enum RecommendationMethod { Popularity, Pearson, Cosine, BaseModel, Stereotypes, NNPearson, NNCosine, NNBaseModel, NNJaccard, CP, Jaccard };

        //class members here
        private List<string> popularMovies;
        //public Ass2RecommenderSystem RecSys;

        public RecommenderSystem()
        {
            // RecSys = new Ass2RecommenderSystem();
            popularMovies = new List<string>();
        }

        /*public void Load(string sFileName)
        {
            RecSys.Load(sFileName);
        }
        public void Load(string sFileName, double dTrainSetSize)
        {
            RecSys.Load(sFileName, dTrainSetSize);
        }

        public void TrainBaseModel(int cFeatures)
        {
            RecSys.TrainBaseModel(cFeatures);
        }
        public void TrainStereotypes(int cStereotypes)
        {
            RecSys.TrainStereotypes(cStereotypes);
        }

        public double GetRating(string sUID, string sIID)
        {
            return RecSys.GetRating(sUID, sIID);
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
        }*/
        public List<string> Recommend(RecommendationMethod sAlgorithm, string sUserId, int cRecommendations)
        {
            if(sAlgorithm == RecommendationMethod.Popularity)
            {
                return recommendPopularity(sUserId, cRecommendations);
            }
            else if (sAlgorithm.ToString().StartsWith("NN"))
            {

            }
            else if(sAlgorithm == RecommendationMethod.Jaccard)
            {
            }
            else if(sAlgorithm == RecommendationMethod.CP)
            {

            }
            else //prediction
            {
                PredictionMethod predictionMethod = PredictionMethod.Pearson;
                if (sAlgorithm == RecommendationMethod.BaseModel)
                    predictionMethod = PredictionMethod.BaseModel;
                else if (sAlgorithm == RecommendationMethod.Cosine)
                    predictionMethod = PredictionMethod.Cosine;
                else if (sAlgorithm == RecommendationMethod.Stereotypes)
                    predictionMethod = PredictionMethod.Stereotypes;
                return recommendPredictions(sUserId, cRecommendations, predictionMethod);
            }

            return new List<string>(); //!!
        }
        private List<string> recommendPopularity(string sUserId, int cRecommendations)
        {
            List<string> ans = new List<string>();
            if (popularMovies.Count == 0)
                calcPopularity();
            if (popularMovies.Count < cRecommendations)
                return popularMovies;
            //remove users' movies
            for (int i = 0; i < popularMovies.Count && ans.Count <= cRecommendations; i++)
            {
                string movie = popularMovies[i];
                if (!movieToUser[movie].Contains(sUserId))
                    ans.Add(popularMovies[i]);
            }
            return ans;
        }
        private List<string> recommendNN(string sUserId, int cRecommendations)
        {
            throw new NotImplementedException();
        }
        private List<string> recommendPredictions(string sUserId, int cRecommendations, PredictionMethod predictionMethod)
        {
            List<string> ans = new List<string>();

            SortedDictionary<double, List<string>> moviesPredictedRatings = new SortedDictionary<double, List<string>>();
            foreach(string movie in movieToUser.Keys)
            {
                if (movieToUser[movie].Contains(sUserId))
                    continue;
                double predictedRating = PredictRating(predictionMethod, sUserId, movie);
                if (!moviesPredictedRatings.ContainsKey(predictedRating))
                    moviesPredictedRatings.Add(predictedRating, new List<string>());
                moviesPredictedRatings[predictedRating].Add(movie);
            }
            foreach(List<string> movies in moviesPredictedRatings.Values)
            {
                if (ans.Count >= cRecommendations)
                    break;
                ans.AddRange(movies);
            }
            if (ans.Count > cRecommendations)//crop
            {
                ans.RemoveRange(cRecommendations, ans.Count - cRecommendations); //!!
            }

            return ans;
        }

        private void calcPopularity() // func that will be called only once to calc the popularity - will generate a list of all the items that will be orderd accourding to the popularity (high->low)
        {
            SortedDictionary<double, List<string>> moviePopSorted = new SortedDictionary<double, List<string>>();
            foreach (string movie in moviePopularity.Keys)
            {
                double popRate = moviePopularity[movie];
                if (!moviePopSorted.ContainsKey(popRate))
                    moviePopSorted.Add(popRate, new List<string>());
                moviePopSorted[popRate].Add(movie);
            }

            foreach(List<string> movies in moviePopSorted.Values)
            {
                popularMovies.AddRange(movies);
            }
            
        }
        //               length           algorithm              precision/recall   result        ---> i think...
        public Dictionary<int, Dictionary<RecommendationMethod, Dictionary<string, double>>> ComputePrecisionRecall(List<RecommendationMethod> lMethods, List<int> lLengths, int cTrials)
        {
            throw new NotImplementedException();
        }

    }
}
