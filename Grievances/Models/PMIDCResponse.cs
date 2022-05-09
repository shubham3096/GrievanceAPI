using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class ResponseInfo
    {
        public string apiId { get; set; }
        public string ver { get; set; }
        public object ts { get; set; }
        public string resMsgId { get; set; }
        public string msgId { get; set; }
        public string status { get; set; }
    }

    public class CitizenDetl
    {
        public object id { get; set; }
        public object uuid { get; set; }
        public string name { get; set; }
        public string mobileNumber { get; set; }
        public object aadhaarNumber { get; set; }
        public object pan { get; set; }
        public object emailId { get; set; }
        public object userName { get; set; }
        public object password { get; set; }
        public object active { get; set; }
        public object type { get; set; }
        public object gender { get; set; }
        public object tenantId { get; set; }
        public object permanentAddress { get; set; }
        public object roles { get; set; }
    }

    public class AuditDetails
    {
        public string createdBy { get; set; }
        public string lastModifiedBy { get; set; }
        public long createdTime { get; set; }
        public long lastModifiedTime { get; set; }
    }

    public class AddressDetails
    {
        public string uuid { get; set; }
        public string houseNoAndStreetName { get; set; }
        public string mohalla { get; set; }
        public string city { get; set; }
        public string landmark { get; set; }
        public string tenantId { get; set; }
        public AuditDetails auditDetails { get; set; }
    }

    public class Services
    {
        public CitizenDetl citizen { get; set; }
        public string tenantId { get; set; }
        public string serviceCode { get; set; }
        public string serviceRequestId { get; set; }
        public string description { get; set; }
        public string addressId { get; set; }
        public string accountId { get; set; }
        public string phone { get; set; }
        public AddressDetails addressDetail { get; set; }
        public bool active { get; set; }
        public string status { get; set; }
        public AuditDetails auditDetails { get; set; }
    }

    public class Action
    {
        public string uuid { get; set; }
        public string tenantId { get; set; }
        public string by { get; set; }
        public long when { get; set; }
        public string businessKey { get; set; }
        public string action { get; set; }
        public string status { get; set; }
    }

    public class ActionHistory
    {
        public List<Action> actions { get; set; }
    }

    public class PMIDCResponse
    {
        public ResponseInfo ResponseInfo { get; set; }
        public List<Services> services { get; set; }
        public List<ActionHistory> actionHistory { get; set; }
    }


}
