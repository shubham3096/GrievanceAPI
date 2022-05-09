using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class EAapiModel
    {
        [Required(ErrorMessage = "SelectBy is required.")]
        public string SelectBy { get; set; }

        public string subdept { get; set; }
        public string deptid { get; set; }
    }

    public class TownByDeptModel
    {
        public long deptId { get; set; }
        public long districtId { get; set; }
    }

    public class GetServiceRequestModel
    {
        public string Service_ID { get; set; }
        public long Department_ID { get; set; }
        public int Page_number { get; set; }
        public int Page_size { get; set; }
        public string Search_by { get; set; }
        public string Search_value { get; set; }
        public string Sort_by{ get; set; }
        public string Sort_order{ get; set; }
    }

    public class GetImportantLinksModel
    {
        public int Page_number { get; set; }
        public int Page_size { get; set; }
    }
}
