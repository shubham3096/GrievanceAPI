using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class emailReportModel
    {

        public string response { get; set; }
        public string Stakeholder_ID { get; set; }    
        public string Stakeholder_Name { get; set; }
        public string Stakeholder_Local_Language { get; set; }
        
      
    }
    public class emailReportModelMain
    {

        public string response { get; set; }
        public List<emailReportModel> data  { get; set; }
        public string sys_message { get; set; }
        public string response_code { get; set; }


    }


    public class emailReportModelResponse
    {

        public string body { get; set; }
        public object emails { get; set; }
       


    }
}
