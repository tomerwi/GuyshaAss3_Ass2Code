using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        private Dictionary<string, Dictionary<string, int>> uiAndujDic;

        public RecommenderSystem()
        {
            popularMovies = new List<string>();
            uiAndujDic = new Dictionary<string, Dictionary<string, int>>();
        }
        public void Load(string sFileName)
        {
            try
            {
                using (FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader r = new StreamReader(fs, Encoding.UTF8))
                    {
                        parseRatings(r);
                        calcAvgs();
                        calcRAI();
                        calcuiAndujDic();
                        calcPopularity();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't load file");
            }
        }

        //new E2
        public void Load(string sFileName, double dTrainSetSize)
        {
            try
            {
                using (FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader r = new StreamReader(fs, Encoding.UTF8))
                    {
                        parseRatings(r);
                        splitToTrainAndTest(dTrainSetSize);
                        mue = computeMue(); //it computes the mue only on the train
                        calcAvgs(); //it computes the avrage on m_ratings 
                        calcRAI();
                        calcuiAndujDic();
                        calcPopularity();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't load file");
            }
        }

        public HashSet<string> GetTestUsers()
        {
            return new HashSet<string>(m_ratings_test.Keys.ToList());
        }

        public HashSet<string> GetTestUserItems(string sUserId)
        {
            if(m_ratings_test.ContainsKey(sUserId))
                return new HashSet<string>(m_ratings_test[sUserId].Keys.ToList());
            return new HashSet<string>();
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
                return recommendCP(sUserId, cRecommendations, true);
            }
            else if(sAlgorithm == RecommendationMethod.CP)
            {
                return recommendCP(sUserId, cRecommendations , false);
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

        }
        private List<string> recommendPopularity(string sUserId, int cRecommendations)
        {
            List<string> ans = new List<string>();
            //if (popularMovies.Count == 0)
              //  calcPopularity();
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

        private List<string> recommendCP(string sUserId, int cRecommendations , bool jaccard)
        {
            SortedDictionary<double, List<string>> pi2Sorted = new SortedDictionary<double, List<string>>();
            //if (uiAndujDic.Count == 0)
              //  calcuiAndujDic();
            foreach(string i2 in movieToUser.Keys)
            {
                if (movieToUser[i2].Contains(sUserId))
                    continue;
                double maxw = 0;
                foreach(string i1 in m_ratings[sUserId].Keys)
                {
                    int uianduj = uiAndujDic[i1][i2];
                    if (uianduj < 10)
                        continue;
                    int ui = movieToUser[i1].Count;
                    int denominator = ui;
                    if (jaccard)
                        denominator += (movieToUser[i2].Count - uianduj);
                    double wi2 = (double)uianduj / (double)denominator;
                    if (wi2 > maxw)
                        maxw = wi2;
                }
                if (!pi2Sorted.ContainsKey(maxw))
                    pi2Sorted.Add(maxw, new List<string>());
                pi2Sorted[maxw].Add(i2);
            }
            //take only top cRec
            List<string> ans = new List<string>();
            List<double> rev = pi2Sorted.Keys.ToList();
            rev.Reverse();
            foreach (double d in rev)
            {
                foreach(string movie in pi2Sorted[d])
                {
                    ans.Add(movie);
                    if (ans.Count >= cRecommendations)
                        break;
                }
                if (ans.Count >= cRecommendations)
                    break;
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
            List<double> rev = movieWSorted.Keys.ToList();
            rev.Reverse();
            foreach(double d in rev)
            {
                foreach(string movie in movieWSorted[d])
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
        private List<string> recommendPredictions(string sUserId, int cRecommendations, PredictionMethod predictionMethod)
        {
            List<string> ans = new List<string>();
            SortedDictionary<double, List<string>> moviesPredictedRatings = new SortedDictionary<double, List<string>>();
            foreach (string movie in movieToUser.Keys)
            {
                if (movieToUser[movie].Contains(sUserId))
                    continue;
                double predictedRating = PredictRating(predictionMethod, sUserId, movie);
                if (!moviesPredictedRatings.ContainsKey(predictedRating))
                    moviesPredictedRatings.Add(predictedRating, new List<string>());
                moviesPredictedRatings[predictedRating].Add(movie);
            }
            List<double> rev = moviesPredictedRatings.Keys.ToList();
            rev.Reverse();
            foreach (double d in rev)
            {
                if (ans.Count >= cRecommendations)
                    break;
                ans.AddRange(moviesPredictedRatings[d]);
            }
            if (ans.Count > cRecommendations)//crop
            {
                ans.RemoveRange(cRecommendations, ans.Count - cRecommendations); //!!
            }

            return ans;
        }

        //calc funcs 

        private double calcJaccardSimilarity(string userID, string userID2) //TODO
        {
            int numerator = 0;
            foreach(string movie in m_ratings[userID2].Keys)
            {
                if (m_ratings[userID].ContainsKey(movie))
                    numerator++;
            }
            int denominator = m_ratings[userID].Keys.Count + m_ratings[userID2].Keys.Count - numerator;
            return ((double)numerator / (double)denominator);
        }

        private double calcBaseModelSimilarity(string userID, string userID2) //TODO
        {
            throw new NotImplementedException();
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
                if (numOfUsers >= 20 && (sumOfW / (double)numOfUsers) > 0.8) //!!
                    break;
                int location = (int) ((m_ratings.Count - 1) * r.NextDouble());
                if (usedLocations.Contains(location))
                    continue;
                string user = m_ratings.Keys.ToList()[location];
                if (user.Equals(sUserId))
                    continue;
                double w = 0;
                if (sAlgorithm == RecommendationMethod.NNCosine)
                    w = calcWCosine(sUserId, user, "");
                else if (sAlgorithm == RecommendationMethod.NNJaccard)
                    w = calcJaccardSimilarity(sUserId, user);
                else if (sAlgorithm == RecommendationMethod.BaseModel)
                    w = calcBaseModelSimilarity(sUserId, user);
                else
                    w = calcWPearson(sUserId, user, "");

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
            List<double> rev = wToUser.Keys.ToList();
            rev.Reverse();
            foreach (double w in rev)
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
   

        private void calcuiAndujDic()
        {
            foreach(string imovie in movieToUser.Keys)
            {
                uiAndujDic.Add(imovie, new Dictionary<string, int>());
                foreach(string jmovie in movieToUser.Keys)
                {
                    if (imovie.Equals(jmovie))
                        continue;
                    int uianduj;
                    if (uiAndujDic.ContainsKey(jmovie))
                    {
                        if(uiAndujDic[jmovie].TryGetValue(imovie,out uianduj))
                        {
                            uiAndujDic[imovie].Add(jmovie, uianduj);
                            continue;
                        }
                    }
                    uianduj = 0;
                    foreach(string user in movieToUser[jmovie])
                    {
                        if (movieToUser[imovie].Contains(user))
                            uianduj++;
                    }
                    uiAndujDic[imovie].Add(jmovie, uianduj);
                    if (uiAndujDic.ContainsKey(jmovie))
                        uiAndujDic[jmovie].Add(imovie, uianduj);
                }
            }
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
            List<double> rev = moviePopSorted.Keys.ToList();
            rev.Reverse();
            foreach(double d in rev)
            {
                popularMovies.AddRange(moviePopSorted[d]);
            }
            
        }
        //               length           algorithm              precision/recall   result        ---> i think...
        public Dictionary<int, Dictionary<RecommendationMethod, Dictionary<string, double>>> ComputePrecisionRecall(List<RecommendationMethod> lMethods, List<int> lLengths, int cTrials)
        {
            Dictionary<int, Dictionary<RecommendationMethod, Dictionary<string, double>>> ans = new Dictionary<int, Dictionary<RecommendationMethod, Dictionary<string, double>>>();
            int max = lLengths.Max();
            Dictionary<string, Dictionary<RecommendationMethod, List<string>>> recommendations = new Dictionary<string, Dictionary<RecommendationMethod, List<string>>>();
            foreach (string user in m_ratings_test.Keys)
            {
                foreach(RecommendationMethod method in lMethods)
                {

                }
            }



            return ans;
        }

    }
}
