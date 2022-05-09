using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class AddofficereModel
    {
        [Required(ErrorMessage = "Actor_ID is required.")]
        public string Actor_ID { get; set; }
        [Required(ErrorMessage = "Parent_Actor_ID Title is required.")]
        public string Parent_Actor_ID { get; set; }
      
    }
}
