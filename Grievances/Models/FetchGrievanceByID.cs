using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class FetchGrievanceByIDModel
    {
        [Required(ErrorMessage = "Actor ID is required.")]
        public string Grievance_ID { get; set; }
        [Required(ErrorMessage = "Access Flag is required.")]
        public string Access_Flag { get; set; }
    }
}
