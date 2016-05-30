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
        public enum RecommendationMethod { Popularity, Pearson, Cosine, BaseModel, Stereotypes, NNPearson, NNCosine, NNBaseModel, NNJaccard, CP, Jaccard };

        //class members here
        private List<string> popularMovies;

        public RecommenderSystem()
        {
            popularMovies = new List<string>();
        }

       
        public List<string> Recommend(RecommendationMethod sAlgorithm, string sUserId, int cRecommendations)
        {
            if(sAlgorithm == RecommendationMethod.Popularity)
            {
                return recommendPopularity(sUserId, cRecommendations);
            }
            else if (sAlgorithm.ToString().StartsWith("NN"))
            {
                return recommendNN(sAlgorithm, sUserId, cRecommendations);
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
        private List<string> recommendNN(RecommendationMethod sAlgorithm, string sUserId, int cRecommendations)
        {
            List<string> ans = new List<string>();
            Dictionary<string, double> sumWmovies = new Dictionary<string, double>();
            Dictionary<string, double> similarUsers = calcSimilarUsers(sAlgorithm, sUserId);
            foreach(string movie in movieToUser.Keys)
            {
                if (movieToUser[movie].Contains(sUserId))
                    continue;
                foreach(string user in similarUsers.Keys)
                {
                    if(movieToUser[movie].Contains(user))
                    {
                        if (!sumWmovies.ContainsKey(movie))
                            sumWmovies.Add(movie, 0);
                        sumWmovies[movie] += similarUsers[user];
                    }
                }
            }
            //think if there's a better way to do it
            SortedDictionary<double, List<string>> movieWSorted = new SortedDictionary<double, List<string>>();
            foreach(string movie in sumWmovies.Keys)
            {
                double w = sumWmovies[movie];
                if (!movieWSorted.ContainsKey(w))
                    movieWSorted.Add(w, new List<string>());
                movieWSorted[w].Add(movie);
            }
            foreach(List<string> movies in movieWSorted.Values)
            {
                foreach(string movie in movies)
                {
                    if (ans.Count >= cRecommendations)
                        break;
                    ans.Add(movie);
                }
                if (ans.Count >= cRecommendations)
                    break;
            }
            return ans;
        }
        private Dictionary<string,double> calcSimilarUsers(RecommendationMethod sAlgorithm, string sUserId)
        {
            SortedDictionary<double, List<string>> wToUser = new SortedDictionary<double, List<string>>();
            List<int> usedLocations = new List<int>();
            double sumOfW = 0;
            int numOfUsers = 0;
            Random r = new Random();
            while (numOfUsers < 500) //not going over all the users to save time
            {
                if (numOfUsers >= 20 && (sumOfW / numOfUsers) > 0.8) //!!
                    break;
                int location = (int) ((m_ratings.Count - 1) * r.NextDouble());
                if (usedLocations.Contains(location))
                    continue;
                string user = m_ratings.Keys.ToList()[location];
                if (user.Equals(sUserId))
                    continue;
                double w = 0;
                if (sAlgorithm == RecommendationMethod.NNPearson)
                    w = calcWPearson(sUserId, user, "");
                else if (sAlgorithm == RecommendationMethod.NNCosine)
                    w = calcWCosine(sUserId, user, "");
                //TODO: base model, jaccard

                if (w>=0.5)//!!
                {
                    sumOfW += w;
                    numOfUsers++;
                    if (!wToUser.ContainsKey(w))
                        wToUser.Add(w, new List<string>());
                    wToUser[w].Add(user);
                }
            }
            Dictionary<string, double> similarUsers = new Dictionary<string, double>();
            //take only top 20
            foreach(double w in wToUser.Keys)
            {
                List<string> users = wToUser[w];
                foreach(string user in users)
                {
                    if (!similarUsers.ContainsKey(user))
                        similarUsers.Add(user, w);
                    if (similarUsers.Count > 20)
                        break;
                }
                if (similarUsers.Count > 20)
                    break;
            }
            
            return similarUsers;
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
