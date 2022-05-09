using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class AddCitizenModel
    {
        public string First_Name { get; set; }
         public string Middle_Name { get; set; }
        public string Last_Name { get; set; }
        public string Father_Name { get; set; }
        public string Mother_Name { get; set; }
        public string Email_ID { get; set; }
        public string Phone_No { get; set; }
        public string Date_Of_Birth { get; set; }
        public string Gender { get; set; }
        public string pincode { get; set; }
        public string address_Type { get; set; }
        public string address_Line_1 { get; set; }
        public string address_Line_2 { get; set; }
        public string district_ID { get; set; }
        public string tehsil_ID { get; set; }
        public string village_ID { get; set; }
        public string village_Name { get; set; }
        public string Municipality_ID { get; set; }
        public long Operator_ID { get; set; }

    }
    public class UpdateCitizenModel
    {
        public long Citizen_ID { get; set; }
        public string First_Name { get; set; }
        public string Middle_Name { get; set; }
        public string Last_Name { get; set; }
        public string Father_Name { get; set; }
        public string Mother_Name { get; set; }
        public string Email_ID { get; set; }
        public string Phone_No { get; set; }
        public string Date_Of_Birth { get; set; }
        public string Gender { get; set; }
        public string pincode { get; set; }
        public string address_Type { get; set; }
        public string address_Line_1 { get; set; }
        public string address_Line_2 { get; set; }
        public string district_ID { get; set; }
        public string tehsil_ID { get; set; }
        public string village_ID { get; set; }
        public string village_Name { get; set; }
        public string Municipality_ID { get; set; }
        public long Operator_ID { get; set; }

    }
}
