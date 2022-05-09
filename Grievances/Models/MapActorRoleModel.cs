using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{ 
    public class ActorRoleMappingModel
    {
        public Int32? Mapping_ID { get; set; } = 0;

        [Required(ErrorMessage = "Actor id is required.")]
        public int? Actor_Id { get; set; }

        [Required(ErrorMessage = "Role id is required.")]
        public int? Role_ID { get; set; }

        //[Required(ErrorMessage = "Location ref id is required.")]
        public int? Location_Ref_ID { get; set; } = 0;

        //[Required(ErrorMessage = "Geo operation level is required.")]
        public string Geo_Operation_Level { get; set; } = null;

        public int? Office_ID { get; set; }

        public Boolean Is_Active { get; set; } = true;
        public int? Stakeholder_ID { get; set; } = 0;
        public int? District_ID { get; set; } = 0;
        public string District_Name { get; set; } = "";
    }
}
