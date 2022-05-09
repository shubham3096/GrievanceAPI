using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class AuthModel
    {
        [Required(ErrorMessage = "user_name is required.")]
        public string user_name { get; set; }

        [Required(ErrorMessage = "password is required.")]
        public string password { get; set; }

    }

    public class SSOAuthModel
    {
        [Required(ErrorMessage = "SSOLogin is required.")]
        public string Token { get; set; }
    }

    public class OTPRequestModel
    {
        public long Actor_Id { get; set; }
        [Required(ErrorMessage = "Email Id is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mobile number is required")]
        public long Mobile_No { get; set; }
        public int Email_OTP { get; set; }
        public int Mobile_OTP { get; set; }
    }

    public class VerifyOTPModel
    {
        public long Actor_Id { get; set; }
        [Required(ErrorMessage = " OTP type is required")]
        public string OTP_Type { get; set; }
        [Required(ErrorMessage = " OTP value is required")]
        public int OTP_Value { get; set; }
    }
}
