using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class NFSAtoPGRSAddNewGrievanceModel
    {
        [Required(ErrorMessage = "State code is required.")]
        public string State { get; set; }
        public string Token { get; set; }
        [Required(ErrorMessage = "AuthString is required.")]
        public string AuthString { get; set; }
        public GrievanceDataModel GrievanceData { get; set; }
        public List<AttachmentDetails> AttachmentDetails { get; set; }
    }
    public class GrievanceDataModel
    {
        [Required(ErrorMessage = "NFSAGrievanceId is required.")]
        public string NFSAGrievanceId { get; set; }
        [Required(ErrorMessage = "GrievanceDate is required.")]
        public string GrievanceDate { get; set; }
        [Required(ErrorMessage = "Grievance Type is required.")]
        public string GrievanceTypeCode { get; set; }
        [Required(ErrorMessage = "TentativeCompletionDate is required.")]
        public string TentativeCompletionDate { get; set; }
        [Required(ErrorMessage = "ModeofGrievanceCode is required.")]
        public string ModeofGrievanceCode { get; set; }
        [Required(ErrorMessage = "PlatformofGrievanceCode is required.")]
        public string PlatformofGrievanceCode { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Mobile is required.")]
        public string Mobile { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }        
        public string RCNo { get; set; }        
        public string FPSNo { get; set; }        
        public string FPSName { get; set; }
        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }
        [Required(ErrorMessage = "District is required.")]
        public string District { get; set; }
        public string Tahsil { get; set; }        
        public string FPSState { get; set; }
        public string FPSDistrict { get; set; }
        public string AadharNumber { get; set; }
        [Required(ErrorMessage = "GrievanceDetails is required.")]
        public string GrievanceDetails { get; set; }
        [Required(ErrorMessage = "GrievanceStatus is required.")]
        public string GrievanceStatus { get; set; }
        
        public string AtachementName { get; set; }
        
        public string AtachementFileType { get; set; }
        
        public string Base64Data { get; set; }

    }
    public class NFSAtoPGRSResponseModel
    {
        [JsonProperty(PropertyName = "State")]
        public string State { get; set; }
        [JsonProperty(PropertyName = "Token")]
        public string Token { get; set; }
        [JsonProperty(PropertyName = "AckCode")]
        public string AckCode { get; set; }
        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "Remarks")]
        public string Remarks { get; set; }
        [JsonProperty(PropertyName = "ServiceDate")]
        public string ServiceDate { get; set; }
        [JsonProperty(PropertyName = "ResponseDetails")]
        public List<ResponseDetailsModel> ResponseDetails { get; set; }

    }
    public class ResponseDetailsModel
    {
        [JsonProperty(PropertyName = "StateGrievanceId")]
        public string StateGrievanceId { get; set; }
        [JsonProperty(PropertyName = "GrievanceDate")]
        public string GrievanceDate { get; set; }
        [JsonProperty(PropertyName = "NFSAGrievanceId")]
        public string NFSAGrievanceId { get; set; }
        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "Remarks")]
        public string Remarks { get; set; }
    }
    public class AttachmentDetails
    {
        [Required(ErrorMessage = "AtachementName is required.")]
        public string AttachmentName { get; set; }
        [Required(ErrorMessage = "AtachementFileType (.pdf/.doc/.docx/.jpeg/.jpg/.png) is required.")]
        public string AttachmentFileType { get; set; }
        [Required(ErrorMessage = "Base64Data is required.")]
        public string Base64Data { get; set; }
    }
}
