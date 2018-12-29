using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using StringSimilarityDemo.Common;
using StringSimilarityDemo.PRH;

namespace StringSimilarityDemo
{
    class Program
    {
        public static readonly string serializedXmlPath =
            AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + @"\LastPrhCompanyInfoSearchResult.xml";

        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;

            try
            {
                ConsoleLogger.Normal($"Starting application {AppDomain.CurrentDomain.FriendlyName}...");

                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(options =>
                    {
                        List<PrhCompanyInfo> allCompanyInfos;
                        var objectXmlSerializer = new ObjectXmlSerializer();

                        if (options.LoadCompanyInfo)
                        {
                            options.ValidateCompanyLoad();
                            var prhClient = new PrhClient();

                            ConsoleLogger.Write("Loading company infos from PRH", ConsoleLogger.NormalColor);
                            allCompanyInfos = prhClient.LoadAllCompanyInfo(options, true);

                            if (File.Exists(serializedXmlPath))
                            {
                                try
                                {
                                    File.Move(serializedXmlPath,
                                        $"{serializedXmlPath.Substring(0, serializedXmlPath.Length - 4)}.Backup.{DateTime.Now:yyyyMMddHHmmssff}.xml");

                                    ConsoleLogger.Write(".", ConsoleLogger.SuccessColor);
                                }
                                catch (Exception)
                                {
                                    ConsoleLogger.Write("!", ConsoleLogger.ErrorColor);
                                }
                            }

                            objectXmlSerializer.SerializeToFile(allCompanyInfos, serializedXmlPath);
                        }
                        else
                        {
                            if (!File.Exists(serializedXmlPath))
                            {
                                throw new FileNotFoundException("Load all company infos before running the app.");
                            }

                            ConsoleLogger.Write("Loading company infos from serialized file...", ConsoleLogger.NormalColor);
                            allCompanyInfos = objectXmlSerializer.DeserializeFromFile<List<PrhCompanyInfo>>(serializedXmlPath);
                        }

                        ConsoleLogger.Normal($" FINISHED! ({allCompanyInfos.Count} companies)");
                    });
            }
            catch (Exception e)
            {
                ConsoleLogger.Error(e);
            }
            finally
            {
                ConsoleLogger.Normal($"Application finished. (Execution time: {(DateTime.Now - startTime).TotalSeconds:N})");
                ConsoleLogger.SetColor(ConsoleLogger.OriginalColor);
            }
        }
    }
}
