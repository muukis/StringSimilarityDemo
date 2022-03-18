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
        public const string SerializedXmlFileName = "LastPrhCompanyInfoSearchResult.xml";
        public static readonly string SerializedXmlPath =
            AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + $@"\{SerializedXmlFileName}";

        public static readonly IStringSimilarity StringSimilarity = new JaroWinkler();

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

        private static void ShowSimilarityResults(Options options, List<StringSimilarityObject<PrhCompanyInfo>> allCompanyInfoSimilarities)
        {
            var similarityResults =
                allCompanyInfoSimilarities.Where(o => o.Similarity >= options.CompanyNameStringSimilarityTreshold).ToList();

            if (similarityResults.Count == 0)
            {
                ConsoleLogger.Warning($"No similarities found with {options.CompanyNameStringSimilarityTreshold:F} treshold.", true);

                var closestSimilarity = allCompanyInfoSimilarities.OrderByDescending(o => o.Similarity).First();
                ConsoleLogger.Normal($"Closest similarity with company {closestSimilarity.Object.name} " +
                                     $"({closestSimilarity.Object.businessId}) " +
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
                    ConsoleLogger.Success($"{i + 1}. {topSimilarities[i].Object.name} " +
                                          $"({topSimilarities[i].Object.businessId}): " +
                                          $"{topSimilarities[i].Similarity:P}", false);
                }
            }
        }

        private static List<StringSimilarityObject<PrhCompanyInfo>> FindCompanyInfoSimilarities(Options options, List<PrhCompanyInfo> allCompanyInfos)
        {
            ConsoleLogger.Write($"Finding similarities with search word \"{options.FindCompanyName}\"... ", ConsoleLogger.NormalColor, true);

            var similarityService = new ObjectSimilarityService(Program.StringSimilarity);
            var allCompanyInfoSimilarities = similarityService.FindSimilarities(allCompanyInfos, options.FindCompanyName, o => o.name);

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
            var searchPattern = SerializedXmlFileName.Substring(0, SerializedXmlFileName.Length - 3) + "*";
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
            objectXmlSerializer.SerializeToFile(allCompanyInfos, SerializedXmlPath);
            ConsoleLogger.Success(" FINISHED!");

            return allCompanyInfos;
        }

        private static List<PrhCompanyInfo> LoadCompanyInfosFromFile()
        {
            if (!File.Exists(SerializedXmlPath))
            {
                throw new FileNotFoundException("Load all company infos before running the app.");
            }

            ConsoleLogger.Write("Loading company infos from serialized file...", ConsoleLogger.NormalColor, true);

            var objectXmlSerializer = new ObjectXmlSerializer();
            var allCompanyInfos = objectXmlSerializer.DeserializeFromFile<List<PrhCompanyInfo>>(SerializedXmlPath);
            return allCompanyInfos;
        }

        private static List<PrhCompanyInfo> LoadCompanyInfosFromPrh(Options options)
        {
            options.ValidateCompanyLoad();
            var prhClient = new PrhClient();

            ConsoleLogger.Write("Loading company infos from PRH", ConsoleLogger.NormalColor, true);
            var allCompanyInfos = prhClient.LoadAllCompanyInfo(options, true);

            if (File.Exists(SerializedXmlPath))
            {
                try
                {
                    string serializedXmlBackupPath =
                        $"{SerializedXmlPath.Substring(0, SerializedXmlPath.Length - 4)}.Backup.{DateTime.Now:yyyyMMddHHmmssff}.xml";

                    File.Move(SerializedXmlPath, serializedXmlBackupPath);

                    ConsoleLogger.Write(".", ConsoleLogger.SuccessColor);
                }
                catch (Exception)
                {
                    ConsoleLogger.Write("!", ConsoleLogger.ErrorColor);
                }
            }

            var objectXmlSerializer = new ObjectXmlSerializer();
            objectXmlSerializer.SerializeToFile(allCompanyInfos, SerializedXmlPath);

            return allCompanyInfos;
        }
    }
}
