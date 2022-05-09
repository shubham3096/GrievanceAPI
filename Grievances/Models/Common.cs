using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class EASSO_Response_Model
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public HttpStatusCode Code { get; set; }
        public object Data { get; set; }
    }
    public class EASSO_Employee_Details
    {
        public string Sso_id { get; set; }
        public string Hrms_id { get; set; }
        public string Email { get; set; }
        public string Mobile_no { get; set; }
        public string First_name { get; set; }
        public string Middle_name { get; set; }
        public string Last_name { get; set; }
        public string Gender { get; set; }
        public bool Is_Active { get; set; }
    }

    public class EASSO_Token_Email
    {       
        public string User_name { get; set; }
       
    }
}
