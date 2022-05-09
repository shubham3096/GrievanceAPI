using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class CategoriesModel
    {
        [Required(ErrorMessage = "Stakeholder_ID is required.")]
        public string Stakeholder_ID { get; set; }
        [Required(ErrorMessage = "Category_Label is required.")]
        public string Category_Label { get; set; }
        [Required(ErrorMessage = "Category_Label_ll is required.")]
        public string Category_Label_ll { get; set; }
        public string  Parent_ID { get; set; }

        public string Is_Active { get; set; }



    }
    public class getCategoriesModel
    {
        [Required(ErrorMessage = "Category_ID is required.")]
        public string Stakeholder_ID { get; set; }

        public string Onboard { get; set; }


    }
    public class deptstatusModel
    {
        [Required(ErrorMessage = "Category_ID is required.")]
        public string Stakeholder_ID { get; set; }
        [Required(ErrorMessage = "Active_Force is required.")]
        public string Active_Force { get; set; }

        [Required(ErrorMessage = "Sstatus is required.")]
        public string Status { get; set; }



    }
    public class getCategoriesModellist
    {
        [Required(ErrorMessage = "Category_ID is required.")]
        public string Category_ID { get; set; }
     }

    public class getactorbyrole
    {
        [Required(ErrorMessage = "Role_ID is required.")]
        public string Role_ID { get; set; }
    }

    public class editCategoriesModel
    {
       
        [Required(ErrorMessage = "Category_Label is required.")]
        public string Category_Label { get; set; }
        [Required(ErrorMessage = "Category_Label_ll is required.")]
        public string Category_Label_ll { get; set; }
        [Required(ErrorMessage = "Category_ID is required.")]
        public string Category_ID { get; set; }        
        public string Is_Active { get; set; }
        public string Parent_ID { get; set; }




    }
    public class getoffice
    {
        [Required(ErrorMessage = "SelectBy is required.")]
        public string SelectBy { get; set; }
        [Required(ErrorMessage = "Parm1 is required.")]
        public string Parm1 { get; set; }
        [Required(ErrorMessage = "Parm2 is required.")]
        public string Parm2 { get; set; }

    }
}
