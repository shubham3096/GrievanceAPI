using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class DashBoardModel
    {
        public string Application_Department { get; set; }
        public string Application_District { get; set; }
        public string Roles { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
    public class GetDashBoardModel
    {
        public string Application_Department { get; set; }
        public string Application_District { get; set; }
        public string Roles { get; set; }
    }


    public class NotificationResultModel
    {
        public long id { get; set; }
        public dynamic data  { get; set; }
    }

}
