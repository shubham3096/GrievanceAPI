using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class PMIDCRequestInfo
    {
        public RequestInfo RequestInfo { get; set; }
    }

    public class RequestInfo
    {
        public string apiId { get; set; }
        public string ver { get; set; }
        public string ts { get; set; }
        public string action { get; set; }
        public string did { get; set; }
        public string key { get; set; }
        public string msgId { get; set; }
        public string authToken { get; set; }
    }

    public class Citizen
    {
        [Required(ErrorMessage = "Citizen Name is required")]
        public string name { get; set; }
        [Required(ErrorMessage = "Citizen Phone number is required")]

        public string mobileNumber { get; set; }
    }

    public class AddressDetail
    {
        [Required(ErrorMessage = "City is required")]
        public string city { get; set; }
        public string mohalla { get; set; }
        public string houseNoAndStreetName { get; set; }
        public string landmark { get; set; }
    }

    public class PMIDCService
    {
        public Citizen citizen { get; set; }
        [Required(ErrorMessage = "Service Code required")]

        public string serviceCode { get; set; }
        [Required(ErrorMessage = "Address Details required")]

        public AddressDetail addressDetail { get; set; }

        public string description { get; set; }
        [Required(ErrorMessage = "Tenant id is required")]

        public string tenantId { get; set; }
        [Required(ErrorMessage = "Phone numbere required")]

        public string phone { get; set; }
        [Required(ErrorMessage = "Source required")]

        public string source { get; set; }

        public Dictionary<string,string> attributes { get; set; }
    }

    public class PmidcModel
    {
        public RequestInfo RequestInfo { get; set; }
        public List<PMIDCService> services { get; set; }
    }



    public class PmidcLoginModel
    {
        [Required(ErrorMessage ="User Name is required")]
        public string username { get; set; }
        [Required(ErrorMessage = "User Name is required")]
        public string password { get; set; }
        [Required(ErrorMessage ="Grant Type is required")]
        public string grant_type { get; set; }
        [Required(ErrorMessage ="Scope is required")]
        public string scope { get; set; }
        [Required(ErrorMessage = "Tenant id is required")]

        public string tenantId { get; set; }
        [Required(ErrorMessage = "User Type is required")]

        public string userType { get; set; }
    }

    public class PMIDCComplainModel
    {
        [Required(ErrorMessage = "Service request id is required")]
        public string serviceRequestId { get; set; }
        [Required(ErrorMessage = "Tenant id is required")]

        public string tenantId { get; set; }
        [Required(ErrorMessage = "Auth Token is required")]

        [DisplayName("RequestInfo") ]
        public RequestInfo RequestInfo { get; set; }

    }


    public class PMIDCStatus
    {
        public string Complaint_Id { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
    }


}
