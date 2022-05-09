using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class CitizenFeedbackModel
    {
        public string Grievance_ID { get; set; }
        public string Action_Taken_By { get; set; }
        public string Remarks { get; set; }


    }

    public class ReminderFeedbackModel
    {
        public string Grievance_ID { get; set; }
        public string Action_Taken_By { get; set; }
        public string Remarks { get; set; }

        public string Action_Type { get; set; }

    }

    public class DisposeCPModel
    {
        [Required(ErrorMessage ="Registration number is required.")]
        public string Registration_no { get; set; }
        [Required(ErrorMessage = "Remarks are required.")]
        public string Remarks { get; set; }
        [JsonIgnore]
        public long Action_Taken_By { get; set; }
        public string  Actor_name { get; set; }
    }

    public class GetDisposeRemarksModel
    {
        [Required(ErrorMessage = "Registration number is required.")]
        public string Registration_no { get; set; }
        [Required(ErrorMessage = "Dispose id are required.")]
        public long Dispose_id { get; set; }
    }
}
