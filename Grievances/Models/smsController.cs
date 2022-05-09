
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class SmsController
    {
        [Required(ErrorMessage = "Mobile No. is required.")]
        public string mobileno { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        public string message { get; set; }

        public string Template_Id { get; set; }
    }
}