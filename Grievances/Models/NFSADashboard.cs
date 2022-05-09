using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class NFSADashboardModel
    {
        public string State { get; set; }
        public string Token { get; set; }
        public string AuthString { get; set; }
        public string DateOfData { get; set; }
        public List<PGRMSDataModel> PGRMSData { get; set; }
    }

    public class PGRMSDataModel
    {
        public string District { get; set; }
        public string GrievanceTypeCode { get; set; }
        public string ModeofGrievanceCode { get; set; }
        public string PlatformofGrievanceCode { get; set; }
        public string CountsPendingOneWeek { get; set; }
        public string CountsPendingTwoWeek { get; set; }
        public string CountsPendingThreeWeek { get; set; }
        public string CountsPendingMoreThanThreeWeek { get; set; }
        public string CountsResolvedOneWeek { get; set; }
        public string CountsResolvedTwoWeek { get; set; }
        public string CountsResolvedThreeWeek { get; set; }
        public string CountsResolvedMoreThanThreeWeek { get; set; }
    }

    public class NFSAResponseModel
    {
        public string State { get; set; }
        public string Token { get; set; }
        public string AckCode { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string DateOfData { get; set; }
    }
        
}
