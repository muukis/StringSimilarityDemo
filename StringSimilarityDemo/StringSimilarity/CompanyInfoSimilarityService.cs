using System.Collections.Generic;
using System.Linq;
using F23.StringSimilarity.Interfaces;
using StringSimilarityDemo.Common;
using StringSimilarityDemo.PRH;

namespace StringSimilarityDemo.StringSimilarity
{
    public class CompanyInfoSimilarityService
    {
        private readonly IStringSimilarity stringSimilarity;
        private readonly Options options;

        public CompanyInfoSimilarityService(Options options, IStringSimilarity stringSimilarity)
        {
            this.options = options;
            this.stringSimilarity = stringSimilarity;
        }

        public List<StringSimilarityCompanyInfo> FindSimilarities(List<PrhCompanyInfo> allCompanyInfos)
        {
            if (allCompanyInfos == null)
            {
                ConsoleLogger.Write("!", ConsoleLogger.ErrorColor);
                return null;
            }

            double donePercentage = 0;
            const double nextCheckPercentageHop = 0.05;
            double nextCheckPercentage = 0.05;
            string findCompanyName = this.options.FindCompanyName.ToUpper();

            string lastDonePercentageText = $"{donePercentage:P}";
            ConsoleLogger.Write(lastDonePercentageText);

            var retval = allCompanyInfos.Select((o, i) =>
            {
                var similarity = new StringSimilarityCompanyInfo
                {
                    CompanyInfo = o,
                    Similarity = this.stringSimilarity.Similarity(findCompanyName, o.name ?? string.Empty)
                };

                donePercentage = (double) (i + 1) / allCompanyInfos.Count;

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

            ConsoleLogger.Write(new string('\b', lastDonePercentageText.Length));

            lastDonePercentageText = $"{donePercentage:P}";
            ConsoleLogger.Write(lastDonePercentageText);

            return retval;
        }
    }
}
