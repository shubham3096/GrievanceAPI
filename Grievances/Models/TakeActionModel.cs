using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class TakeActionModel
    {
        [Required(ErrorMessage = "Grievance_ID is required.")]
        public string Grievance_ID { get; set; }
        
        public string To { get; set; }
        [Required(ErrorMessage = "ActionType is required.")]
        public string Action_Type { get; set; }
        [Required(ErrorMessage = "Remarks is required.")]
        public string Remarks { get; set; }
        
        public string Status_Text { get; set; }
        public int Dms_ID { get; set; }
        public string changeOrigin { get; set; }
       
        public string criteria { get; set; }
        [Required(ErrorMessage = "Action is required.")]
        public string Taction { get; set; }

        public long Category_ID { get; set; }
        public long Sub_Category_ID { get; set; }

    }
}
