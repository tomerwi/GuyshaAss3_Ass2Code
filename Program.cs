using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//add here other usings that were added to E2

namespace RecommenderSystem
{
    class Program
    {
        static void Assignment3()
        {
            RecommenderSystem rs = new RecommenderSystem();
            rs.Load("ratings.dat", 0.95);
            rs.TrainBaseModel(10);
            rs.TrainStereotypes(10);

            List<string> lRecommendations = rs.Recommend(RecommenderSystem.RecommendationMethod.Pearson, "6", 5);
            Console.Write("Recommended movies for user 6 ");
            foreach (string sMovie in lRecommendations)
                Console.Write(sMovie + ",");
            Console.WriteLine();

            List<RecommenderSystem.RecommendationMethod> lMethods = new List<RecommenderSystem.RecommendationMethod>();
            lMethods.Add(RecommenderSystem.RecommendationMethod.BaseModel);
            lMethods.Add(RecommenderSystem.RecommendationMethod.Pearson);
            lMethods.Add(RecommenderSystem.RecommendationMethod.NNPearson);
            lMethods.Add(RecommenderSystem.RecommendationMethod.Popularity);
            lMethods.Add(RecommenderSystem.RecommendationMethod.Jaccard);

            List<int> lLengths = new List<int>();
            lLengths.Add(1);
            lLengths.Add(3);
            lLengths.Add(5);
            lLengths.Add(10);
            lLengths.Add(20);

            DateTime dtStart = DateTime.Now;
            Dictionary<int, Dictionary<RecommenderSystem.RecommendationMethod, Dictionary<string, double>>> dResults = rs.ComputePrecisionRecall(lMethods, lLengths, 1000);
            Console.WriteLine("Precision-recall scores for all methods are:");
            foreach (int iLength in lLengths)
            {
                foreach (RecommenderSystem.RecommendationMethod sMethod in lMethods)
                {
                    foreach (string sMetric in dResults[iLength][sMethod].Keys)
                    {
                        Console.WriteLine(iLength + "," + sMethod + "," + sMetric + " = " + Math.Round(dResults[iLength][sMethod][sMetric], 4));
                    }
                }
            }
            Console.WriteLine("Execution time was " + Math.Round((DateTime.Now - dtStart).TotalSeconds, 0));
            Console.ReadLine();
        }


        static void Main(string[] args)
        {
            Dictionary<int, List<string>> param = new Dictionary<int, List<string>>();
            param.Add(1, new List<string>());
            param[1].Add("1");
            param[1].Add("2");
            param.Add(2, new List<string>());
            param[2].Add("21");
            Dictionary<int, List<string>> res = test(param);
            Console.WriteLine(param.Count);
            Console.WriteLine(res.Count);
            Assignment3();
        }
        static Dictionary<int, List<string>> test (Dictionary<int, List<string>> param)
        {
            param.Remove(2);
            param[1].Remove("1");
            return new Dictionary<int, List<string>>();
        }
    }
}
