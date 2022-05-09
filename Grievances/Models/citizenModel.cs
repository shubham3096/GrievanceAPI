using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class citizenModel
    {
        public int? Employee_ID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string First_Name { get; set; }

        public string Middle_Name { get; set; }

        public string Email_ID { get; set; } = null;
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Father name is required.")]
        public string Father_Name { get; set; }

        [Required(ErrorMessage = "Mother name is required.")]
        public string Mother_Name { get; set; }

        //[Required(ErrorMessage = "Date of birth is required.")]
        public string Date_Of_Birth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
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

        public string Registered_From{ get; set; }
    }

    public class citizenOTPModel
    {
        [Required(ErrorMessage = "Citizen_Ref_ID  is required.")]
        public string Citizen_Ref_ID { get; set; }

        [Required(ErrorMessage = "OTP  is required.")]
        public string OTP { get; set; }

    }
    public class citizenresetpassword
    {
        [Required(ErrorMessage = "User_Name  is required.")]
        public string User_Name { get; set; }

        [Required(ErrorMessage = "Mobile  is required.")]
        public string Mobile { get; set; }

    }
    public class changepasswordModel
    {
        [Required(ErrorMessage = "NewPassword  is required.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "ConfirmPassword  is required.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "OTP  is required.")]
        public string OTP { get; set; }

        [Required(ErrorMessage = "Citizen_ID  is required.")]
        public string Citizen_ID { get; set; }

    }

    public class CitizenLoginModel
    {
        public int? Login_ID { get; set; }
        public string User_Name { get; set; }
        public string User_Password { get; set; }
        public int? Citizen_Ref_ID { get; set; }
        public int? Is_First_Login { get; set; }
        public int? Login_Attempts { get; set; }
        public string Last_Password_1 { get; set; }
        public string Last_Password_2 { get; set; }
        public string Last_Password_3 { get; set; }
        public DateTime? Password_Exp { get; set; }
        public string OTP { get; set; }
        public Boolean? OTP_Is_Verified { get; set; }
        public string Status { get; set; }
        public Boolean? Is_Active { get; set; }
        public int? Created_By { get; set; }
        public string OTP_Verified_From { get; set; }
    }

    public class SMSModel
    {
         

        public string mobileno { get; set; }
        public string message { get; set; }
    }
    public class VerifyLoginAccessModel
    {
        [Required(ErrorMessage = "Citizen ID is required.")]
        public int? Citizen_Ref_ID { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        public string OTP { get; set; }
    }
    public class GenerateCitizenOTP
    {
        [Required(ErrorMessage = "User name is required.")]
        public string User_Name { get; set; }

        [Required(ErrorMessage = "Mobile is required.")]
        public string Mobile { get; set; }
    }
    public class CitizenChangePasswordModel
    {
        //[Required(ErrorMessage = "Old Password is required.")]
        //public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Password Mismatch!")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Citizen id is required.")]
        public int? Citizen_ID { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        public string OTP { get; set; }
    }


    public class CitizenRegisterModel
    {

        [Required(ErrorMessage = "First name is required.")]
        public string First_Name { get; set; }

        public string Middle_Name { get; set; }

        public string Email_ID { get; set; } = null;
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Father name is required.")]
        public string Father_Name { get; set; }

        [Required(ErrorMessage = "Mother name is required.")]
        public string Mother_Name { get; set; }

        public DateTime? Date_Of_Birth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
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

        public string State_Ref_ID { get; set; }
        
        public string District_Ref_ID { get; set; }

        public Int32 Tehsil_Ref_ID { get; set; }

        public Int32 Village_ID { get; set; }

        public string Village_Name { get; set; }

        public Int32 Municipality_ID { get; set; }

        public int? Location_Ref_ID { get; set; }

        public int? Pincode { get; set; }
        public string Aadhaar_Ref_ID { get; set; }
    }

}
