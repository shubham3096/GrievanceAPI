using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class OverdueReportModel
    {
        [Required(ErrorMessage = "department ID is required.")]
        public string deptid { get; set; }
        [Required(ErrorMessage = "District ID is required.")]
        public string distid { get; set; }
       
        public string actorid { get; set; }
    }


    public class OverdueReportdeptModel
    {
        [Required(ErrorMessage = "department ID is required.")]
        public string deptid { get; set; }
      
    }

    public class getactorsbydeptidanddistidModel
    {
        [Required(ErrorMessage = "department ID is required.")]
        public string Parm1 { get; set; }
        [Required(ErrorMessage = "District ID is required.")]
        public string Parm2 { get; set; }
        [Required(ErrorMessage = "Actor ID is required.")]
        public string SelectBy { get; set; }
    }
    public class actorresponseModel
    {

        public string response { get; set; }
        public string Actor_ID { get; set; }
        public string Designation_Name { get; set; }
        
    }
    public class actorresponseModelMain
    {

        public string response { get; set; }
        public List<actorresponseModel> data { get; set; }
        public string sys_message { get; set; }
        public string response_code { get; set; }


    }

}
