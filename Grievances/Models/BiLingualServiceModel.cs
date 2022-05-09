
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class BiLingualServiceModel
    {
        [Required(ErrorMessage = "Please provide service ID.")]
        public int ServiceID { get; set; }

        [Required(ErrorMessage = "Please provide language code.")]
        public string Language_Code { get; set; }

        public string ServiceType { get; set; }
    }
    public class BilingualLibraryItem
    {
        public string Label_ID { get; set; }
        public int Service_ID { get; set; }
        public string Label_Eng { get; set; }
        public string Label_Pb { get; set; }
        public string Service_Type { get; set; }
    }
    public class BilingualResponse
    {
        public string Label_ID { get; set; }
        public string Label_Value { get; set; }
    }

    public class BilingualRequestModel
    {
        [Required(ErrorMessage = "Please provide service ID.")]
        public int[] ServiceID { get; set; }

        [Required(ErrorMessage = "Please provide language code.")]
        public string Language_Code { get; set; }

        public string[] ServiceType { get; set; }
    }
}
