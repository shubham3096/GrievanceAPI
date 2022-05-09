using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class ChangepasswordModel
    {
        [Required(ErrorMessage = "OldPassword is required.")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "NewPassword Title is required.")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "ConfirmPassword Title is required.")]
        public string ConfirmPassword { get; set; }
      
        public string Actor_id { get; set; }

    }
    public class ChangeusernameModel
    {
        [Required(ErrorMessage = "newusername is required.")]
        public string User_Name { get; set; }
      
        public string Actor_ID { get; set; }

        public string Employee_ID { get; set; }

        public string Password { get; set; }



    }
    public class checkusernameModel
    {
        [Required(ErrorMessage = "newusername is required.")]
        public string UserName { get; set; }
        



    }

}
