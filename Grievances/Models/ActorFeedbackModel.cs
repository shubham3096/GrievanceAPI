using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class ActorFeedbackModel
    {
        [Required(ErrorMessage = "Subject is required.")]
        public string subject { get; set; }
        [Required(ErrorMessage = "Description Title is required.")]
        public string Description{ get; set; }


    }
    
}
