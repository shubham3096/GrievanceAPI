using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class AadharAuth
    {
        public string Aadhaar { get; set; }
        public string OTPRequestResponse { get; set; }
        public string rescode { get; set; }
       
    }
    public class AadharcardAuth
    {
        public string Aadhaar { get; set; }
        public string OTP { get; set; }
        public string txn { get; set; }
        public Int64 aadhaarrefid { get; set; }
        public string OTPAuthResponse { get; set; }
        public string rescode { get; set; }
    }

        public class AadharcardAuthDemo
        {
            public string Aadhaar { get; set; }
            public string OTP { get; set; }
            public string txn { get; set; }
            public Int64 aadhaarrefid { get; set; }
            public string OTPAuthResponse { get; set; }
            public string rescode { get; set; }
            public string Name { get; set; }
            public string YOB { get; set; }

            public string Gender { get; set; }

        }
}
