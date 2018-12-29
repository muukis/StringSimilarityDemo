using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CommandLine;

namespace StringSimilarityDemo.Common
{
    public class Options
    {
        [Option('v', "verbose", Required = false, Default = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('l', "load", Required = false, Default = false, HelpText = "Reload all company infos to current directory.")]
        public bool LoadCompanyInfo { get; set; }

        [Option('f', "find", Required = true, HelpText = "Find company by name.")]
        public string FindCompanyName { get; set; }

        [Option('t', "treshold", Required = false, Default = 0.8, HelpText = "Find company by name string similarity treshold.")]
        public double CompanyNameStringSimilarityTreshold { get; set; }

        [Option('s', "size", Required = false, Default = 10, HelpText = "Find company by name string similarity result set maximum size.")]
        public int ResultSetSize { get; set; }

        [Option('r', "registrationFrom", Required = false, Default = "2010-01-01", HelpText = "Find companies limited to registration from date.")]
        public string CompanyRegistrationFrom { get; set; }

        [Option('e', "registrationTo", Required = false, Default = null, HelpText = "Find companies limited registration to date.")]
        public string CompanyRegistrationTo { get; set; }

        public void ValidateCompanyLoad()
        {
            if (!LoadCompanyInfo)
            {
                return;
            }

            var errorList = new List<string>();

            if (CompanyNameStringSimilarityTreshold < 0)
            {
                errorList.Add("String similarity treshold minimum value is 0.");
            }

            if (CompanyNameStringSimilarityTreshold > 1)
            {
                errorList.Add("String similarity treshold maximum value is 1.");
            }

            if (ResultSetSize < 1)
            {
                errorList.Add("Result set maximum size minimum value is 1.");
            }

            const string dateTimePattern = @"^\d{4}-\d{2}-\d{2}$";

            if (!Regex.IsMatch(CompanyRegistrationFrom, dateTimePattern) ||
                !DateTime.TryParseExact(CompanyRegistrationFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime temp1))
            {
                errorList.Add("Invalid registration from date. (Must be in format 'yyyy-MM-dd')");
            }

            if (CompanyRegistrationTo != null &&
                (!Regex.IsMatch(CompanyRegistrationTo, dateTimePattern) ||
                 !DateTime.TryParseExact(CompanyRegistrationTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime temp2)))
            {
                errorList.Add("Invalid registration to date. (Must be in format 'yyyy-MM-dd')");
            }

            if (errorList.Count > 0)
            {
                throw new ApplicationException($"Invalid options:{Environment.NewLine}{string.Join(Environment.NewLine, errorList)}");
            }
        }
    }
}
