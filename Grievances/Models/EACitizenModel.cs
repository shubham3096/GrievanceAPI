using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class EACitizenModel
    {
        public int? Employee_ID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string First_Name { get; set; }

        public string Middle_Name { get; set; }

        [EmailAddress(ErrorMessage = "Please enter valid email")]
        public string Email_ID { get; set; } = null;
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Father name is required.")]
        public string Father_Name { get; set; }

        [Required(ErrorMessage = "Mother name is required.")]
        public string Mother_Name { get; set; }

        public string Date_Of_Birth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [RegularExpression("M|F|O", ErrorMessage = "The Gender must be either 'Male' or 'Female' or 'Others' only.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Phone no. is required.")]
        public string Phone_No { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        public string Marital_Status { get; set; }

        public string Profile_Picture_Dms_Ref_Id { get; set; }

        [Required(ErrorMessage = "Address line is required.")]
        public string Address_Line_1 { get; set; }

        public string Address_Type { get; set; }

        public string Address_Line_2 { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public string State_Ref_ID { get; set; }

        [Required(ErrorMessage = "District is required.")]
        public string District_Ref_ID { get; set; }

        public Int32 Tehsil_Ref_ID { get; set; }

        public Int32 Village_ID { get; set; }

        public string Village_Name { get; set; }

        public Int32 Municipality_ID { get; set; }

        public int? Location_Ref_ID { get; set; }

        [Required(ErrorMessage = "Pin Code is required.")]
        public int? Pincode { get; set; }
        [Required(ErrorMessage = "Aadhaar_Ref_ID is required.")]
        public string Aadhaar_Ref_ID { get; set; }
        public string Registered_From { get; set; }
    }
}
