using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class CheckOTP
    {
        [Required(ErrorMessage = "GrievanceID is required.")]
        public string Grievance_ID { get; set; }
        [Required(ErrorMessage = "OTP is required.")]
        public string OTP { get; set; }
    }
    public class ResendOTP
    {
        [Required(ErrorMessage = "GrievanceID is required.")]
        public string Grievance_ID { get; set; }
       
    }
}
