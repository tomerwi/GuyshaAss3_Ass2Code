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
            SortedDictionary<double, List<string>> sorted = new SortedDictionary<double, List<string>>();
            sorted.Add(-1, new List<string>());
            sorted.Add(0, new List<string>());
            sorted.Add(30, new List<string>());
            sorted.Add(30.1, new List<string>());
            sorted[30.1].Add("1");
            sorted[30.1].Add("2");
            sorted[30].Add("3");
            sorted[0].Add("4");
            sorted[-1].Add("5");
            List<string> l1 = new List<string>();
            List<string> l2 = new List<string>();
            List<double> rev = sorted.Keys.ToList();
            rev.Reverse();
            foreach(double k in rev)
            {
                l1.AddRange(sorted[k]);
                foreach (string s in sorted[k])
                    l2.Add(s);
            }
            Console.WriteLine("l1:");
            foreach (string s in l1)
                Console.WriteLine(s);
            Console.WriteLine("l2:");
            foreach (string s in l2)
                Console.WriteLine(s);
            Assignment3();
        }
    }
}
