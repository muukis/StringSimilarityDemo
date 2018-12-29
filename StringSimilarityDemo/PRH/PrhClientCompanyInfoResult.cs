using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringSimilarityDemo.PRH
{
    public class PrhClientCompanyInfoResult
    {
        public string type { get; set; }
        public string version { get; set; }
        public int totalResults { get; set; }
        public int resultsFrom { get; set; }
        public object previousResultsUri { get; set; }
        public string nextResultsUri { get; set; }
        public object exceptionNoticeUri { get; set; }
        public List<PrhCompanyInfo> results { get; set; }
    }
}
