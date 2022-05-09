using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class GrievanceHistoryModel
    {
        public int Page_number { get; set; }
        public int Page_size { get; set; }
        public string Search_by { get; set; }
        public string Search_value { get; set; }
        public string Sort_by { get; set; }
        public string Sort_order { get; set; }
        [JsonIgnore]
        public long Citizen_ID { get; set; }
        public DateTime? From_date { get; set; } = null;
        public DateTime? To_date { get; set; } = null;
    }   
}
