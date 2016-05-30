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
            List<string> ans = new List<string>();

            if(sAlgorithm == RecommendationMethod.Popularity)
            {
                if (popularMovies.Count == 0)
                    calcPopularity();
                if (popularMovies.Count < cRecommendations)
                    return popularMovies;
                //remove users' movies
                for(int i =0; i<popularMovies.Count && ans.Count<=cRecommendations; i++)
                {
                    string movie = popularMovies[i];
                    if (!movieToUser[movie].Contains(sUserId))
                        ans.Add(popularMovies[i]);
                }            
            }

            return ans;
        }
        private void calcPopularity() // func that will be called only once to calc the popularity - will generate a list of all the items that will be orderd accourding to the popularity (high->low)
        {
            SortedDictionary<double, List<string>> mps = new SortedDictionary<double, List<string>>();
            foreach (string movie in moviePopularity.Keys)
            {
                double popRate = moviePopularity[movie];
                if (!mps.ContainsKey(popRate))
                    mps.Add(popRate, new List<string>());
                mps[popRate].Add(movie);
            }

            foreach(List<string> movies in mps.Values)
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
