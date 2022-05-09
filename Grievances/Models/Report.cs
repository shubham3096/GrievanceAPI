using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class Report
    {
        public string Fromdate { get; set; }
        public string Todate { get; set; }
        public string Dept { get; set; }
        public string Dist { get; set; }
        public string Status { get; set; }
        public string CategoryID { get; set; }
        public string SubCategoryID { get; set; }
        public int? pageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
