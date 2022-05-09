using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class StakeholderModel
    {
        public int? Stakeholder_ID { get; set; }                public string Stakeholder_Name { get; set; }                public string Stakeholder_Local_Language { get; set; }                public string Stakeholder_Type { get; set; }                public int? P_ID { get; set; }                public string Short_Code { get; set; }        public Boolean? Is_Active { get; set; }
    }
}
