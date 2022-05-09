using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class GetGrievanceTrail
    {
        [Required(ErrorMessage = "Grievance ID is required.")]
        public string Grievance_ID { get; set; }
    }
}
