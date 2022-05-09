using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Models
{
    public class Email
    {
        [EmailAddress(ErrorMessage = "Invalid Email!")]
        [Required(ErrorMessage = "Email-ID is required.")]
        public string emailId { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public string subject { get; set; }

        [Required(ErrorMessage = "Body is Invalid.")]
        public string body { get; set; }
    }

    public class Emails
    {
        [Required(ErrorMessage = "Body is required.")]
        public string body { get; set; }

        public List<EmailData> emails { get; set; }
    }

    public class EmailData
    {
        [Required(ErrorMessage = "Email is required.")]
        public string emailId { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public string subject { get; set; }
    }

    public class TokenData
    {
        public string Token { get; set; }
    }
}
