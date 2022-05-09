using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class GrievanceModel
    {
        [Required(ErrorMessage = "Application Title is required.")]
        public string Application_Title { get; set; }
        [Required(ErrorMessage = "Application Description is required.")]
        public string Application_Description { get; set; }
        [Required(ErrorMessage = "Application District is required.")]
        public int Application_District { get; set; }
        [Required(ErrorMessage = "Application Department is required.")]
        public int Application_Department { get; set; }
        [Required(ErrorMessage = "Application Department Name is required.")]
        public String Application_Department_Name { get; set; }
        public Int64 Citizen_EA_User_ID { get; set; }
        [Required(ErrorMessage = "Citizen Name is required.")]
        public string Citizen_Name { get; set; }

        public string Citizen_Email { get; set; }
        [Required(ErrorMessage = "Citizen Mobile No is required.")]
        public string Citizen_Mobile_No { get; set; }
        [Required(ErrorMessage = "Citizen Address  is required.")]
        public string Citizen_Address { get; set; }
        [Required(ErrorMessage = "Citizen District  is required.")]
        public string Citizen_District { get; set; }
        [Required(ErrorMessage = "Citizen District ID is required.")]
        public int Citizen_District_ID { get; set; }
       
        [Required(ErrorMessage = "Citizen State is required.")]
        public string Citizen_State { get; set; }
        [Required(ErrorMessage = "Citizen State ID is required.")]
        public int Citizen_State_ID { get; set; }
        public string Citizen_Pincode { get; set; }
        [Required(ErrorMessage = "Citizen Type is required.")]
        public string Citizen_Type { get; set; }

        public string Previous_Grievance { get; set; }
        public string Otp { get; set; }
        public bool Is_Otp_Verified { get; set; }
        [Required(ErrorMessage = "AssignedTo is required.")]
        public int Assigned_To { get; set; }
        
        public Int32 Sub_Category_ID { get; set; }
       
        public Int32 Category_ID { get; set; }
        [Required(ErrorMessage = "Flow Type is required.")]
        public string Flow_Type { get; set; }


        public string Citizen_Village { get; set; }

        public string Citizen_Village_ID { get; set; }
        public string Citizen_Tehsil { get; set; }
        public string Citizen_Tehsil_ID { get; set; }
        public string Citizen_Municipality { get; set; }
        public string Citizen_Municipality_ID { get; set; }

        public string Application_District_Name { get; set; }
        public string submittedby { get; set; }

        
        public string System_type { get; set; }

        public string Request_type { get; set; }

        public List<DocumentModel> doc { get; set; }
        // Bunty LocationType
        public string LocationType { get; set; }

        public string CPGrievance { get; set; }
        public string NFSAGrievanceId { get; set; }

        public string Locality_Code{ get; set; }
        public long Town_ID { get; set; }

        public string Service_Code { get; set; }
        public string Town_Name { get; set; }
        public string Locality_Name { get; set; }

    }

    public class DocumentModel
    {
        public Int32 Document_ID { get; set; }
        public Int32 Grievance_ID { get; set; }
        public Int32 File_ID { get; set; }
    }
    public class GrievanceIdModel
    {
        public string Grievance_id { get; set; }
    }
}
