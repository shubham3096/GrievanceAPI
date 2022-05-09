using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class ProfileModel
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string middlename { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string addressdistrict { get; set; }
        public string addresstehsil { get; set; }
        public string addressvillage { get; set; }
        public string addressmunicipality { get; set; }
      //  public long Citizen_ID { get; set; }
    }
}
