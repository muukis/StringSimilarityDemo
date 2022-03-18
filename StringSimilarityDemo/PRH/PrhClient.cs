using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using RestSharp;
using StringSimilarityDemo.Common;

namespace StringSimilarityDemo.PRH
{
    public class PrhClient
    {
        private int _sleepFor429 = 5000;

        public List<PrhCompanyInfo> LoadAllCompanyInfo(Options options, bool ignoreErrors = false)
        {
            var companyInfos = new List<PrhCompanyInfo>();

            try
            {
                int loadLimit = 1000;
                int newCompanyInfosCount;
                int startingIndex = 115000;

                do
                {
                    ConsoleLogger.Write(".", ConsoleLogger.NormalColor);
                    var newCompanyInfos = LoadCompanyInfo(options, startingIndex, loadLimit);
                    startingIndex += loadLimit;
                    newCompanyInfosCount = newCompanyInfos?.Count ?? 0;

                    if (newCompanyInfosCount > 0)
                    {
                        companyInfos.AddRange(newCompanyInfos);
                    }
                } while (newCompanyInfosCount > 0);
            }
            catch (Exception e)
            {
                ConsoleLogger.WriteLine();

                if (!ignoreErrors)
                {
                    throw;
                }

                ConsoleLogger.Write(e, ConsoleLogger.WarningColor);
            }

            return companyInfos;
        }

        public List<PrhCompanyInfo> LoadCompanyInfo(Options options, int startingIndex, int loadLimit)
        {
            if (loadLimit > 1000)
            {
                throw new IndexOutOfRangeException($"Parameter {nameof(loadLimit)} maximum value is 1000.");
            }

            var client = new RestClient("https://avoindata.prh.fi");

            var resource = "bis/v1?totalResults=false" +
                           $"&maxResults={loadLimit}" +
                           $"&resultsFrom={startingIndex}" +
                           $"&companyRegistrationFrom={options.CompanyRegistrationFrom}";

            if (options.CompanyRegistrationTo != null)
            {
                resource += $"&companyRegistrationTo={options.CompanyRegistrationTo}";
            }

            var request = new RestRequest(resource);
            RestResponse<PrhClientCompanyInfoResult> response;

            while(true)
            {
                response = client.ExecuteAsync<PrhClientCompanyInfoResult>(request).GetAwaiter().GetResult();

                if ((int)response.StatusCode != 429) // 429 = Too Many Requests
                {
                    break;
                }

                ConsoleLogger.Write("!", ConsoleLogger.WarningColor);
                Thread.Sleep(_sleepFor429);
                _sleepFor429 += 1000;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException($"{nameof(PrhClient)} REST response returned with status code {response.StatusCode}.");
            }

            return response.Data?.results;
        }
    }
}
