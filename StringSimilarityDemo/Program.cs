using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using F23.StringSimilarity;
using F23.StringSimilarity.Interfaces;
using StringSimilarityDemo.Common;
using StringSimilarityDemo.PRH;
using StringSimilarityDemo.StringSimilarity;

namespace StringSimilarityDemo
{
    class Program
    {
        public const string serializedXmlFileName = "LastPrhCompanyInfoSearchResult.xml";
        public static readonly string serializedXmlPath =
            AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + $@"\{serializedXmlFileName}";

        public static IStringSimilarity stringSimilarity = new JaroWinkler();

        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;
            ConsoleLogger.WriteLine("/************************/", ConsoleColor.Yellow);
            ConsoleLogger.WriteLine($"Starting application {AppDomain.CurrentDomain.FriendlyName}...", ConsoleColor.Yellow, true);

            try
            {
                Parser.Default.ParseArguments<Options>(args).WithParsed(MainWorker);
            }
            catch (Exception e)
            {
                ConsoleLogger.Error(e);
            }
            finally
            {
                ConsoleLogger.Normal($"Application finished. (Execution time: {(DateTime.Now - startTime).TotalSeconds:N}s)", true);
                ConsoleLogger.SetColor(ConsoleLogger.OriginalColor);
            }
        }

        private static void MainWorker(Options options)
        {
            ConsoleLogger.Options = options;
            var allCompanyInfos = LoadCompanyInfos(options);

            if (allCompanyInfos.Count == 0)
            {
                return;
            }

            var allCompanyInfoSimilarities = FindCompanyInfoSimilarities(options, allCompanyInfos);

            ShowSimilarityResults(options, allCompanyInfoSimilarities);
        }

        private static void ShowSimilarityResults(Options options, List<StringSimilarityCompanyInfo> allCompanyInfoSimilarities)
        {
            var similarityResults =
                allCompanyInfoSimilarities.Where(o => o.Similarity >= options.CompanyNameStringSimilarityTreshold).ToList();

            if (similarityResults.Count == 0)
            {
                ConsoleLogger.Warning($"No similarities found with {options.CompanyNameStringSimilarityTreshold:F} treshold.", true);

                var closestSimilarity = allCompanyInfoSimilarities.OrderByDescending(o => o.Similarity).First();
                ConsoleLogger.Normal($"Closest similarity with company {closestSimilarity.CompanyInfo.name} " +
                                     $"({closestSimilarity.CompanyInfo.businessId}) " +
                                     $"was with similarity value {closestSimilarity.Similarity:P}.", true);
            }
            else
            {
                ConsoleLogger.Success($"Found {similarityResults.Count} similarities:", true);

                var topSimilarities = similarityResults
                    .OrderByDescending(o => o.Similarity)
                    .Take(options.ResultSetSize)
                    .ToList();

                for (int i = 0; i < topSimilarities.Count; i++)
                {
                    ConsoleLogger.Success($"{i + 1}. {topSimilarities[i].CompanyInfo.name} " +
                                          $"({topSimilarities[i].CompanyInfo.businessId}): " +
                                          $"{topSimilarities[i].Similarity:P}", true);
                }
            }
        }

        private static List<StringSimilarityCompanyInfo> FindCompanyInfoSimilarities(Options options, List<PrhCompanyInfo> allCompanyInfos)
        {
            ConsoleLogger.Write($"Finding similarities with search word \"{options.FindCompanyName}\"... ", ConsoleLogger.NormalColor, true);

            var similarityService = new CompanyInfoSimilarityService(options, Program.stringSimilarity);
            var allCompanyInfoSimilarities = similarityService.FindSimilarities(allCompanyInfos);

            ConsoleLogger.Success(" FINISHED!");

            return allCompanyInfoSimilarities;
        }

        private static List<PrhCompanyInfo> LoadCompanyInfos(Options options)
        {
            var allCompanyInfos = options.LoadCompanyInfo
                ? LoadCompanyInfosFromPrh(options)
                : LoadCompanyInfosFromFile();

            ConsoleLogger.Success($" FINISHED! (Loaded {allCompanyInfos.Count} companies)");

            if (options.Merge)
            {
                allCompanyInfos = MergeSerializedFiles();
            }

            return allCompanyInfos;
        }

        private static List<PrhCompanyInfo> MergeSerializedFiles()
        {
            ConsoleLogger.Write("Merging all serialized files...", ConsoleLogger.NormalColor, true);
            var searchPattern = serializedXmlFileName.Substring(0, serializedXmlFileName.Length - 3) + "*";
            var objectXmlSerializer = new ObjectXmlSerializer();

            var backUps = objectXmlSerializer.DeserializeFromFiles<List<PrhCompanyInfo>>(
                AppDomain.CurrentDomain.BaseDirectory, searchPattern, File.Delete);

            if (backUps == null)
            {
                return null;
            }

            var allCompanyInfos = backUps.SelectMany(o => o).ToList();
            ConsoleLogger.Success($" FINISHED! (Loaded {allCompanyInfos.Count} companies)");

            ConsoleLogger.Write("Serializing merged file to one file...", ConsoleLogger.NormalColor, true);
            objectXmlSerializer.SerializeToFile(allCompanyInfos, serializedXmlPath);
            ConsoleLogger.Success(" FINISHED!");

            return allCompanyInfos;
        }

        private static List<PrhCompanyInfo> LoadCompanyInfosFromFile()
        {
            if (!File.Exists(serializedXmlPath))
            {
                throw new FileNotFoundException("Load all company infos before running the app.");
            }

            ConsoleLogger.Write("Loading company infos from serialized file...", ConsoleLogger.NormalColor, true);

            var objectXmlSerializer = new ObjectXmlSerializer();
            var allCompanyInfos = objectXmlSerializer.DeserializeFromFile<List<PrhCompanyInfo>>(serializedXmlPath);
            return allCompanyInfos;
        }

        private static List<PrhCompanyInfo> LoadCompanyInfosFromPrh(Options options)
        {
            options.ValidateCompanyLoad();
            var prhClient = new PrhClient();

            ConsoleLogger.Write("Loading company infos from PRH", ConsoleLogger.NormalColor, true);
            var allCompanyInfos = prhClient.LoadAllCompanyInfo(options, true);

            if (File.Exists(serializedXmlPath))
            {
                try
                {
                    string serializedXmlBackupPath =
                        $"{serializedXmlPath.Substring(0, serializedXmlPath.Length - 4)}.Backup.{DateTime.Now:yyyyMMddHHmmssff}.xml";

                    File.Move(serializedXmlPath, serializedXmlBackupPath);

                    ConsoleLogger.Write(".", ConsoleLogger.SuccessColor);
                }
                catch (Exception)
                {
                    ConsoleLogger.Write("!", ConsoleLogger.ErrorColor);
                }
            }

            var objectXmlSerializer = new ObjectXmlSerializer();
            objectXmlSerializer.SerializeToFile(allCompanyInfos, serializedXmlPath);

            return allCompanyInfos;
        }
    }
}
