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
                        calcAdditionalData();
                        calcuiAndujDic();
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
                        if (!SplitTrainTest(m_ratings, dTrainSetSize, out m_train, out m_test))
                            Console.WriteLine("split to train and test failed!");
                        else
                        {
                            m_trainSize = dTrainSetSize;
                            m_mue = computeMue(); //it computes the mue only on the train
                            calcAdditionalData(); //it computes the avrage on m_ratings 
                            calcuiAndujDic();
                        }

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
            return new HashSet<string>(m_test.Keys.ToList());
        }

        public HashSet<string> GetTestUserItems(string sUserId)
        {
            if(m_test.ContainsKey(sUserId))
                return new HashSet<string>(m_test[sUserId]);
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

           return popularMovies.TakeWhile(movie => !m_trainMovieToUser[movie].Contains(sUserId)).Take(cRecommendations).ToList();
        }

        private List<string> recommendCP(string sUserId, int cRecommendations , bool jaccard)
        {
            SortedDictionary<double, List<string>> pi2Sorted = new SortedDictionary<double, List<string>>();
            //if (uiAndujDic.Count == 0)
              //  calcuiAndujDic();
            foreach(string i2 in m_trainMovieToUser.Keys)
            {
                if (m_trainMovieToUser[i2].Contains(sUserId))
                    continue;
                double maxw = 0;
                foreach(string i1 in m_train[sUserId])
                {
                    int uianduj = uiAndujDic[i1][i2];
                    if (uianduj < 10)
                        continue;
                    int ui = m_trainMovieToUser[i1].Count;
                    int denominator = ui;
                    if (jaccard)
                        denominator += (m_trainMovieToUser[i2].Count - uianduj);
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
            Dictionary<string, double> sumWmovies = new Dictionary<string, double>();
            Dictionary<string, double> similarUsers = calcSimilarUsers(sAlgorithm, sUserId);
            foreach(string movie in m_trainMovieToUser.Keys)
            {
                if (m_trainMovieToUser[movie].Contains(sUserId))
                    continue;
                foreach(string user in similarUsers.Keys)
                {
                    if(m_trainMovieToUser[movie].Contains(user))
                    {
                        if (!sumWmovies.ContainsKey(movie))
                            sumWmovies.Add(movie, 0);
                        sumWmovies[movie] += similarUsers[user];
                    }
                }
            }

            List<string> moviesSorted = sumWmovies.Keys.ToList();
            moviesSorted.Sort((a, b) => sumWmovies[a].CompareTo(sumWmovies[b]));
            return moviesSorted.Take(cRecommendations).ToList();

 
        }
        private List<string> recommendPredictions(string sUserId, int cRecommendations, PredictionMethod predictionMethod)
        {
            //List<string> ans = new List<string>();
            //SortedDictionary<double, List<string>> moviesPredictedRatings = new SortedDictionary<double, List<string>>();
            Dictionary<string, double> moviesPredictedRatings = new Dictionary<string, double>();
            foreach (string movie in m_trainMovieToUser.Keys)
            {
                if (m_trainMovieToUser[movie].Contains(sUserId))
                    continue;
                double predictedRating = PredictRating(predictionMethod, sUserId, movie);
                moviesPredictedRatings.Add(movie, predictedRating);
               /* if (!moviesPredictedRatings.ContainsKey(predictedRating))
                    moviesPredictedRatings.Add(predictedRating, new List<string>());
                moviesPredictedRatings[predictedRating].Add(movie);*/
            }
            List<string> moviesSorted = moviesPredictedRatings.Keys.ToList();
            moviesSorted.Sort((a, b) => moviesPredictedRatings[a].CompareTo(moviesPredictedRatings[b]));
            return moviesSorted.Take(cRecommendations).ToList();  
        
        }

        //calc funcs 

        private double calcJaccardSimilarity(string userID, string userID2) 
        {
            int numerator = 0;
            foreach(string movie in m_train[userID2])
            {
                if (m_train[userID].Contains(movie))
                    numerator++;
            }
            int denominator = m_train[userID].Count + m_train[userID2].Count - numerator;
            return ((double)numerator / (double)denominator);
        }

        private double calcBaseModelSimilarity(string userID, string userID2) //TODO
        {
            throw new NotImplementedException();
        }

        private Dictionary<string,double> calcSimilarUsers(RecommendationMethod sAlgorithm, string sUserId)
        {
            Dictionary<string, double> userW = new Dictionary<string, double>();
            List<int> usedLocations = new List<int>();
            double sumOfW = 0;
            int numOfUsers = 0;
            Random r = new Random();
            while (numOfUsers < 500) //not going over all the users to save time
            {
                if (numOfUsers >= 20 && (sumOfW / (double)numOfUsers) > 0.8) //!!
                    break;
                int location = (int) ((m_train.Count - 1) * r.NextDouble());
                if (usedLocations.Contains(location))
                    continue;
                string user = m_train.Keys.ToList()[location];
                if (user.Equals(sUserId) /*|| userW.ContainsKey(user)*/)
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
                    userW.Add(user, w);

                }
            }

            return userW.OrderBy(x => x.Value).Take(20).ToDictionary(x => x.Key, x => x.Value);
        
        }
   

        private void calcuiAndujDic() //improve
        {
            foreach(string imovie in m_trainMovieToUser.Keys)
            {
                uiAndujDic.Add(imovie, new Dictionary<string, int>());
                foreach(string jmovie in m_trainMovieToUser.Keys)
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
                    foreach(string user in m_trainMovieToUser[jmovie])
                    {
                        if (m_trainMovieToUser[imovie].Contains(user))
                            uianduj++;
                    }
                    uiAndujDic[imovie].Add(jmovie, uianduj);
                    if (uiAndujDic.ContainsKey(jmovie))
                        uiAndujDic[jmovie].Add(imovie, uianduj);
                }
            }
        }

    
        public Dictionary<int, Dictionary<string, Dictionary<RecommendationMethod, double>>> ComputePrecisionRecall(List<RecommendationMethod> lMethods, List<int> lLengths, int cTrials)
        {
            Dictionary<int, Dictionary<string, Dictionary<RecommendationMethod, double>>> ans = new Dictionary<int, Dictionary<string, Dictionary<RecommendationMethod, double>>>();
            lLengths.ForEach(l =>
            {
                ans.Add(l, new Dictionary<string, Dictionary<RecommendationMethod, double>>());
                ans[l].Add("precision", new Dictionary<RecommendationMethod, double>());
                ans[l].Add("recall", new Dictionary<RecommendationMethod, double>());
            });
            int max = lLengths.Max();
            foreach (string user in m_test.Keys)
            {
                foreach (RecommendationMethod method in lMethods)
                {
                    List<string> reccomendation = Recommend(method, user, max);
                    lLengths.ForEach(len => 
                    {
                        if (!ans[len]["precision"].ContainsKey(method))
                            ans[len]["precision"].Add(method, 0);
                        if (!ans[len]["recall"].ContainsKey(method))
                            ans[len]["recall"].Add(method, 0);
                        double prec = 0;
                        double rec = 0;
                        calcPrecisionRecall(user, reccomendation.Take(len).ToList(), out prec, out rec);
                        ans[len]["precision"][method] += prec;
                        ans[len]["recall"][method] += rec;
                    });

                }
            }
            lLengths.ForEach(l => 
            {
                lMethods.ForEach(m =>
                {
                    ans[l]["precision"][m] = (ans[l]["precision"][m] / m_test.Keys.Count);
                    ans[l]["recall"][m] = (ans[l]["recall"][m] / m_test.Keys.Count);
                });
            });
             

            return ans;
        }

        private void calcPrecisionRecall(string user, List<string> recommendations, out double dPrecision, out double dRecall)
        {
            int tp = 0;
            recommendations.ForEach(movie => 
            {
                if (m_test[user].Contains(movie))
                    tp++;
            });
            dPrecision = tp / (double)recommendations.Count;
            dRecall = tp /(double) m_test[user].Count;
        }

    }
}
