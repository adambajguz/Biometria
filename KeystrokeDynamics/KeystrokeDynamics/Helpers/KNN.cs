using System;
using System.Collections.Generic;

namespace KeystrokeDynamics.Helpers
{
    public class KNN
    {
        public int K { get; set; }

        public Func<long, long[], long> DistanceFunction;

        public KNN(int k, Func<long, long[], long> distanceFunction)
        {
            K = k;
            DistanceFunction = distanceFunction;
        }

        public int Execute(Dictionary<char, long> measure, List<Storage.Entities.User> measures)
        {
            foreach (var item in measures)
            {
                foreach (var ii in measure)
                {
                    try
                    {
                        result += Math.Abs(measure[ii.Key] - item.KeystrokeVector[ii.Key]);
                    }
                    catch (Exception)
                    {

                    }
                }
                classifiers.Add(new Classifier(result, item.Id));
                result = 0;
            }

            classifiers.Sort(delegate (Classifier x, Classifier y)
            {
                return (int)(x.Result - y.Result);
            });

            return classifiers[0].UserId;

        }
        private long result = 0;
        private static int k = 1; //k param
        private List<Classifier> classifiers = new List<Classifier>();
    }


    public class Classifier
    {
        public long Result { get; set; }
        public int UserId { get; set; }

        public Classifier(long result, int userId)
        {
            Result = result;
            UserId = userId;
        }
    }
}
