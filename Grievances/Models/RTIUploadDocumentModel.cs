using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{

    public class PagingModel
    {
        public int pageNumber { get; set; }
        public int pageSize{ get; set; }
        public int userId { get; set; }
        public string[] sortBy { get; set; }
        public long? applicationId { get; set; }
    }
    public class RTIUploadDocumentModel
    {
        public string DocPath { get; set; }
        public IFormFile Content { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class ApplicationMaster
    {
        public int applicationId { get; set; }
    }

    public class ApplicantDetails
    {
        public string applicantName { get; set; }
        public string fatherName { get; set; }
        public string dateOfBirth { get; set; }
        public int age { get; set; }
        public string maritalStatus { get; set; }
        public string gender { get; set; }
        public string spouseName { get; set; }
        public long mobileNumber { get; set; }
        public string email { get; set; }
        public string region { get; set; }
        public string houseStreetWardNo { get; set; }
        public object village { get; set; }
        public object subDistrictTehsil { get; set; }
        public int district { get; set; }
        public int pinCode { get; set; }
        public string citizenship { get; set; }
        public bool bpl { get; set; }
        public string rtiRequest { get; set; }
        public string bplCard { get; set; }
        public string supportingDocument { get; set; }
        public string bplCardExtension { get; set; }
        public string supportingDocumentExtension { get; set; }
        
        [JsonIgnore]
        public DateTime createdDate { get; set; }
        public int departmentMaster { get; set; }
        public int publicAuthorityMaster { get; set; }
        public ApplicationMaster applicationMaster { get; set; }
        public int user { get; set; }
        public int state { get; set; }
        public object subdistrictTehsilName { get; set; }
        public object villageName { get; set; }
        public string idProof { get; set; }
        public string idProofDocument { get; set; }
        public string idProofDocumentType { get; set; }
        public long assignedTo { get; set; }
    }


    public class LocationModel
    {
        public int StateId { get; set; }
        public int? DistId { get; set; } = 0;
        public int? TehId { get; set; } = 0;
        public int? VillId { get; set; } = 0;
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class AppealGround
    {
        public int id { get; set; }
    }

    public class CitizenApplicationMaster
    {
        public int id { get; set; }
    }
    public class CitizenAppealModel
    {
        public AppealGround appealGround { get; set; }
        public string appeal { get; set; }
        public string supportingDocument { get; set; }
        public string supportingDocumentExtension { get; set; }
        public CitizenApplicationMaster citizenApplicationMaster { get; set; }
        public int appealType { get; set; }
        public ApplicationMaster applicationMaster { get; set; }
        public Nullable<bool> undertaking { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class RoleModel
    {
        public int user_id { get; set; }
        public int department_id { get; set; }
        public int subdepartment_id { get; set; }
        public int role { get; set; }
        public bool is_active { get; set; }
        public string actorName { get; set; }
        public object actorEmail { get; set; }
        public object actorMobile { get; set; }
        public int actorLevelId { get; set; }
        public int districtId { get; set; }
        public int officeId { get; set; }
        public string districtName { get; set; }
        public string officeName { get; set; }
    }



    public class TransactionModel
    {
        public string purpose1 { get; set; }
        public string purpose2 { get; set; }
        public string purpose3 { get; set; }
        public string purpose4 { get; set; }
        public string purpose5 { get; set; }
        public string success_url { get; set; }
        public string failure_url { get; set; }
        public int citizen_app_id { get; set; }
        public int citizen_app_master_id { get; set; }
        public int application_appeal_status { get; set; }
        public bool ask_for_payment { get; set; }
        public long ask_for_payment_id { get; set; }
        public long appeal_id { get; set; }
        
    }

    public class ReceiptModel
    {
        public long id { get; set; }
        public long fetch_by { get; set; }
    }

    public class ActorListModel
    {
        public int id { get; set; }
        public bool isactive { get; set; }
    }

    public class ActorSearchModel
    {
        public int AdminDept_ID { get; set; }
        public int Stakeholder_ID { get; set; }
        public int Role_ID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public Nullable<bool> is_HeadQuarter { get; set; }
        public string Name { get; set; }
        public int? HRMS_ID { get; set; }
        public string User_Name { get; set; }
        public string Mobile_No { get; set; }
        public string Office_Name { get; set; }
        public string Designation_Name { get; set; }
        public string District_ID { get; set; }
    }

    public class DepartmentOnBoardStatusModel
    {
        public string Search_by { get; set; }
        public Nullable<bool> OnBoard_status { get; set; }
        public long? Dept_id { get; set; }
    }

    public class UpdateOnBoardModel
    {
        public long Dept_id { get; set; }
        public bool Status { get; set; }
        public long P_id{ get; set; }
    }

    public class ActorOfficeModel
    {
        public int departmentId { get; set; }
        public int subdeptId { get; set; }
        public int actorLevelId { get; set; }
        public Nullable<int> districtId { get; set; }
    }

    public class DepartmentOnBoardActiveActorModel
    {
        public List<long> stakeholderIds { get; set; }
        public string roleName { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public long publicAuthorityId { get; set; }
    }

}
