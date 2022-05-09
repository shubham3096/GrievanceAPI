using GrievanceService.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class CommonCitizenModel
    {
        public Int64 userID { get; set; }
        public Int64 grievanceID { get; set; }

        [Required(ErrorMessage = "User type is required!")]
        public string user_type { get; set; } = "Citizen";
        [Required(ErrorMessage = "Application name is required!")]
        public string application_name { get; set; } = "eSewa";
        [Required(ErrorMessage = "First name is required!")]
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        [Required(ErrorMessage = "First name in punjabi is required!")]
        public string first_name_punjabi { get; set; }
        public string middle_name_punjabi { get; set; }
        public string last_name_punjabi { get; set; }
        [Required(ErrorMessage = "Father first name is required!")]
        public string father_first_name { get; set; }
        [Required(ErrorMessage = "Father first name in punjabi is required!")]
        public string father_first_name_punjabi { get; set; }
        public string father_middle_name { get; set; }
        public string father_middle_name_punjabi { get; set; }
        public string father_last_name { get; set; }
        public string father_last_name_punjabi { get; set; }
        [Required(ErrorMessage = "Mother first name is required!")]
        public string mother_first_name { get; set; }
        [Required(ErrorMessage = "Mother first name in punjabi is required!")]
        public string mother_first_name_punjabi { get; set; }
        public string mother_middle_name { get; set; }
        public string mother_middle_name_punjabi { get; set; }
        public string mother_last_name { get; set; }
        public string mother_last_name_punjabi { get; set; }
        public string gender_punjabi { get; set; }
        public string age { get; set; }
        public string date_Of_Birth { get; set; }
        public string mobile_no { get; set; }
        public string place_of_birth { get; set; }
        public string place_of_birth_punjabi { get; set; }
        [Required(ErrorMessage = "Marital status is required!")]
        public string marital_status { get; set; }
        public string marital_status_punjabi { get; set; }
        public string email_id { get; set; }
        public string is_email_verified { get; set; }
        public string email_verified_on { get; set; }
        public string phone_no { get; set; }
        public string is_mobile_verified { get; set; }
        public string mobile_verified_on { get; set; }
        public string Occupation { get; set; }
        public string Qualification { get; set; }

        [RequiredIf("marital_status", "Married, Widowed", ErrorMessage = "Husband first name is required!")]
        public string husband_first_name { get; set; }
        [RequiredIf("marital_status", "Married, Widowed", ErrorMessage = "Husband first name in punjabi is required!")]
        public string husband_first_name_punjabi { get; set; }
        [RequiredIf("marital_status", "Married, Widowed", ErrorMessage = "Husband midddle name is required!")]
        public string husband_middle_name { get; set; }
        [RequiredIf("marital_status", "Married, Widowed", ErrorMessage = "Husband middle name in punjabi is required!")]
        public string husband_middle_name_punjabi { get; set; }
        [RequiredIf("marital_status", "Married, Widowed", ErrorMessage = "Husband last name is required!")]
        public string husband_last_name { get; set; }
        [RequiredIf("marital_status", "Married, Widowed", ErrorMessage = "Husband last name in punjabi is required!")]
        public string husband_last_name_punjabi { get; set; }

        /*Permanent address*/
        public CommonCitizenAddressModel p_address = new CommonCitizenAddressModel();
        /*Corrospondance address*/
        public CommonCitizenAddressModel c_address = new CommonCitizenAddressModel();
        /*Profile Picture*/
        public CommonCitizenPictureModel picture = new CommonCitizenPictureModel();
    }
    public class CommonCitizenAddressModel
    {
        public Int64 addressID { get; set; }

        [Required(ErrorMessage = "User ID is required!")]
        public Int64 userID { get; set; }
        [Required(ErrorMessage = "Address type is required!")]
        public string address_type { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string address_line_3 { get; set; }
        [Required(ErrorMessage = "Region is required!")]
        public string region { get; set; }
        [Required(ErrorMessage = "State ID is required!")]
        public Int64 stateID { get; set; }
        [Required(ErrorMessage = "District ID is required!")]
        public Int64 districtID { get; set; }
        [Required(ErrorMessage = "Tehsil ID is required!")]
        public Int64 tehsilID { get; set; }
        public Int64 blockID { get; set; } = 0;
        [RequiredIf("region", "Rural", ErrorMessage = "Village ID is required!")]
        public Int64 villageID { get; set; }
        [Required(ErrorMessage = "Pin Code is required!")]
        public string pincode { get; set; }
        [Required(ErrorMessage = "Address in punjabi is required!")]
        public string address_punjabi { get; set; }
    }
    public class CommonCitizenPictureModel
    {
        public Int64 docid { get; set; } = 0;

        [Required(ErrorMessage = "User ID is required!")]
        public Int64 userID { get; set; }
        [Required(ErrorMessage = "doc file is required!")]
        public string doc_file { get; set; }
        [Required(ErrorMessage = "doc type is required!")]
        public string filetype { get; set; }
        public string Isactive { get; set; }
        public string blobFileReference { get; set; }
        public string blobContainer { get; set; }
    }
    public class MediaFile
    {
        public List<InputFile> files { get; set; }
        public string isServiceSame { get; set; }
        public string submissionType { get; set; }
        public string is_document_uploaded { get; set; }
    }
    public class EsewaCitizenProfileModel
    {
        public Int64 CitizenID { get; set; } = 0;
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string Fatherfirstname { get; set; }
        public string Fathermiddlename { get; set; }
        public string Fatherlastname { get; set; }
        public string Motherfirstname { get; set; }
        public string Mothermiddlename { get; set; }
        public string Motherlastname { get; set; }
        public string Gender { get; set; }
        public string Dob { get; set; }
        public string Maritalstatus { get; set; }
        public string Mobileno { get; set; }
        public string email { get; set; }
        public string Husbandfirstname { get; set; }
        public string Husbandmiddlename { get; set; }
        public string Husbandlastname { get; set; }
        public string Firstnamepunjabi { get; set; }
        public string Middlenamepunjabi { get; set; }
        public string Lastnamepunjabi { get; set; }
        public string Fatherfirstnamepunjabi { get; set; }
        public string Fathermiddlenamepunjabi { get; set; }
        public string Fatherlastnamepunjabi { get; set; }
        public string Motherfirstnamepunjabi { get; set; }
        public string Mothermiddlenamepunjabi { get; set; }
        public string Motherlastnamepunjabi { get; set; }
        public string Genderpunjabi { get; set; }
        public string Placeofbirthpunjabi { get; set; }
        public string Maritalstatuspunjabi { get; set; }
        public string Husbandfirstnamepunjabi { get; set; }
        public string Husbandmiddlenamepunjabi { get; set; }
        public string Husbandlastnamepunjabi { get; set; }
        public string Userid { get; set; }
        public int age { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_1pb { get; set; }
        public string address_line_2 { get; set; }
        public string address_line_3 { get; set; }
        public string region { get; set; }
        public Int64 stateID { get; set; }
        public Int64 districtID { get; set; }
        public Int64 tehsilID { get; set; }
        public Int64 blockID { get; set; }
        public Int64 villageID { get; set; }
        public Int64 pincode { get; set; }
        public string address_line_1_c { get; set; }
        public string address_line_1_cpb { get; set; }
        public string address_line_2_c { get; set; }
        public string address_line_3_c { get; set; }
        public string region_c { get; set; }
        public Int64 stateID_c { get; set; }
        public Int64 districtID_c { get; set; }
        public Int64 tehsilID_c { get; set; }
        public Int64 blockID_c { get; set; }
        public Int64 villageID_c { get; set; }
        public Int64 pincode_c { get; set; }
    }

    /*Add model for connect users*/
    public class MediaFileForConnect
    {
        public List<InputFileForConnect> files { get; set; }
        public string isServiceSame { get; set; }
        public string submissionType { get; set; }
        public string is_document_uploaded { get; set; }
    }
    public class InputFileForConnect
    {
        public string filename { get; set; }
        public int? filesize { get; set; }
        public string filetype { get; set; }
        public string doc_ref_id { get; set; }
        public string appId { get; set; }
        public string base64 { get; set; }
        public string doc_id { get; set; }
        public bool IsDigitallySigned { get; set; }
        public string armslicenseId { get; set; }
        public string isMandatory { get; set; }
        public string userid { get; set; }
    }

    public class GrievanceNodalOffcerModel
    {
        public int Page_number { get; set; }
        public int? Page_size { get; set; }
        public int Department_ID { get; set; }
       
    }
}
