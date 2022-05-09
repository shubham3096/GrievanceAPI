using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class CPGramModel
    {
        [Required(ErrorMessage = "Grievance_ID is required.")]
        public string Grievance_ID { get; set; }
       
    }

    public class CPGramReturnModel
    {
        [Required(ErrorMessage = "registration_no is required.")]
        public string registration_no { get; set; }

        [Required(ErrorMessage = "action_date is required.")]
        public string action_date { get; set; }

        [Required(ErrorMessage = "Remarks is required.")]
        public string Remarks { get; set; }

        [Required(ErrorMessage = "officername is required.")]
        public string officername { get; set; }

        [Required(ErrorMessage = "officerdesignation is required.")]
        public string officerdesignation { get; set; }

    }

    public class CPGramSearchModel
    {
        [Required(ErrorMessage = "Grievance_ID is required.")]
        public string Grievance_ID { get; set; }
        public string Access_Flag { get; set; } = "B";

    }

}
