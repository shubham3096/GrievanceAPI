using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class FetchNodalOfficerAPIModel
    {
        
        public int  Department_id { get; set; }
        public int Location_ID { get; set; }
    }
    public class FetchNodalOfficerNameAPIModel
    {

        public string Actor_ID { get; set; }
        public string State_Actor_ID { get; set; }
        public string ADCGR_Actor_ID { get; set; }

        public string ACG_ID { get; set; }

    }
}
