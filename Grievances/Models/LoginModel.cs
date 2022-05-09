using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string password { get; set; }
    }
    public class ActorLogSessionModel
    {
        public string Token { get; set; }

        public string IP { get; set; }

        public string User_Agent { get; set; }

        public Int64? Login_ID { get; set; }
    }
    public class CitizenChangePassModel
    {
        [Required(ErrorMessage = "Old Password is required.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Password Mismatch!")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Citizen id is required.")]
        public int? Citizen_ID { get; set; }
    }
}
