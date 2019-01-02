using System;
using System.Collections.Generic;
using System.Linq;
using F23.StringSimilarity.Interfaces;
using StringSimilarityDemo.Common;

namespace StringSimilarityDemo.StringSimilarity
{
    public class ObjectSimilarityService
    {
        private readonly IStringSimilarity stringSimilarity;

        public ObjectSimilarityService(IStringSimilarity stringSimilarity)
        {
            this.stringSimilarity = stringSimilarity;
        }

        public List<StringSimilarityObject<T>> FindSimilarities<T>(List<T> allObjects, string findText, Func<T, string> comparedText)
        {
            if (allObjects == null)
            {
                ConsoleLogger.Write("!", ConsoleLogger.ErrorColor);
                return null;
            }

            double donePercentage = 0;
            const double nextCheckPercentageHop = 0.05;
            double nextCheckPercentage = 0.05;
            string findCompanyName = findText.ToUpper();

            string lastDonePercentageText = $"{donePercentage:P}";
            ConsoleLogger.Write(lastDonePercentageText);

            var startTime = DateTime.Now;

            var retval = allObjects.Select((o, i) =>
            {
                var similarity = new StringSimilarityObject<T>
                {
                    Object = o,
                    Similarity = this.stringSimilarity.Similarity(findCompanyName, comparedText(o)?.ToUpper() ?? string.Empty)
                };

                donePercentage = (double) (i + 1) / allObjects.Count;

                if (donePercentage > nextCheckPercentage)
                {
                    ConsoleLogger.Write(new string('\b', lastDonePercentageText.Length));

                    lastDonePercentageText = $"{donePercentage:P}";
                    ConsoleLogger.Write(lastDonePercentageText);

                    while (nextCheckPercentage < donePercentage)
                    {
                        nextCheckPercentage += nextCheckPercentageHop;
                    }
                }

                return similarity;
            }).ToList();

            var timeConsumed = DateTime.Now - startTime;

            ConsoleLogger.Write(new string('\b', lastDonePercentageText.Length));
            ConsoleLogger.Write($"{donePercentage:P} ");
            ConsoleLogger.Write($"(Time consumed: {timeConsumed.TotalMilliseconds:F}ms " +
                                $"[avg/compare: {(timeConsumed.TotalMilliseconds / allObjects.Count) * 1000:F}ns])", ConsoleColor.Yellow);

            return retval;
        }
    }
}
