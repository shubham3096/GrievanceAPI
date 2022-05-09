using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class SewaKendraServiceModel
    {
        public int Page_number { get; set; }
        public int Page_size { get; set; }
        public int dept_id { get; set; }
    }


    public class GetSewaKendraDepartmentModel
    {
        public int selectBy { get; set; }
        public int department_id { get; set; }
        public int Page_number { get; set; }
        public int Page_size { get; set; }
    }
    public class GetSewaKendraModel
    {
        public int district_id { get; set; }
        public int address_id { get; set; }
        public int Page_number { get; set; }
        public int Page_size { get; set; }
    }
}
