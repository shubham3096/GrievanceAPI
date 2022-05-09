using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class MasterModel
    {
        [Required(ErrorMessage = "Select by is required.")]
        public string SelectBy { get; set; }

        public string Parm1 { get; set; }

        public string Parm2 { get; set; }

        public string Parm3 { get; set; }
    }
}
