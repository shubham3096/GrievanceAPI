using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("api/commoncitizen")]
    public class CommonCitizenController : Controller
    {
        #region Controller Properties
        private ServiceResponseModelRows _objResponseRows = new ServiceResponseModelRows();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        CommonFunctions APICall;
        DataTable _dt = new DataTable();
        DataSet _ds = new DataSet();
        List<SqlParameter> _parm = new List<SqlParameter>();
        string eSewaURL = string.Empty;
        string eSewaCreateCitizenProfileAPI = string.Empty;
        string eSewaUploadProfilePictureAPI = string.Empty;
        DocumentController _DocumentController;
        #endregion

        public CommonCitizenController(IConfiguration configuration, IHostingEnvironment env)
        {
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this.eSewaURL = this._configuration["eSewa:DevelopmentURL"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.eSewaURL = this._configuration["eSewa:StagingURL"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.eSewaURL = this._configuration["eSewa:ProductionURL"];
            }
            this.eSewaCreateCitizenProfileAPI = this._configuration["eSewa:CreateCitizenProfile"];
            this.eSewaUploadProfilePictureAPI = this._configuration["eSewa:UploadProfilePicture"];
        }

        #region Save Citizen Detail
        [HttpPost("CreateCitizenDetail")]
        public ServiceResponseModel CreateCitizenDetail([FromBody] CommonCitizenModel InputModel)
        {
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            #region DB PARAMETERS
            List<SqlParameter> Parameters = new List<SqlParameter>();
            int? Citizen_Ref_ID = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Citizen_Ref_ID"));
            /*Profile parameters*/
            Parameters.Add(new SqlParameter("userID", InputModel.userID));
            Parameters.Add(new SqlParameter("grievanceID", Citizen_Ref_ID));
            Parameters.Add(new SqlParameter("user_type", "Citizen"));
            Parameters.Add(new SqlParameter("application_name", InputModel.application_name));
            Parameters.Add(new SqlParameter("user_name", InputModel.phone_no));
            Parameters.Add(new SqlParameter("first_name", InputModel.first_name));
            Parameters.Add(new SqlParameter("first_name_punjabi", InputModel.first_name_punjabi));
            Parameters.Add(new SqlParameter("middle_name", InputModel.middle_name));
            Parameters.Add(new SqlParameter("middle_name_punjabi", InputModel.middle_name_punjabi));
            Parameters.Add(new SqlParameter("last_name", InputModel.last_name));
            Parameters.Add(new SqlParameter("last_name_punjabi", InputModel.last_name_punjabi));
            Parameters.Add(new SqlParameter("father_first_name", InputModel.father_first_name));
            Parameters.Add(new SqlParameter("father_first_name_punjabi", InputModel.father_first_name_punjabi));
            Parameters.Add(new SqlParameter("father_middle_name", InputModel.father_middle_name));
            Parameters.Add(new SqlParameter("father_middle_name_punjabi", InputModel.father_middle_name_punjabi));
            Parameters.Add(new SqlParameter("father_last_name", InputModel.father_last_name));
            Parameters.Add(new SqlParameter("father_last_name_punjabi", InputModel.father_last_name_punjabi));
            Parameters.Add(new SqlParameter("mother_first_name", InputModel.mother_first_name));
            Parameters.Add(new SqlParameter("mother_first_name_punjabi", InputModel.mother_first_name_punjabi));
            Parameters.Add(new SqlParameter("mother_middle_name", InputModel.mother_middle_name));
            Parameters.Add(new SqlParameter("mother_middle_name_punjabi", InputModel.mother_middle_name_punjabi));
            Parameters.Add(new SqlParameter("mother_last_name", InputModel.mother_last_name));
            Parameters.Add(new SqlParameter("mother_last_name_punjabi", InputModel.mother_last_name_punjabi));
            Parameters.Add(new SqlParameter("gender_punjabi", InputModel.gender_punjabi));
            Parameters.Add(new SqlParameter("age", InputModel.age));
            Parameters.Add(new SqlParameter("date_Of_Birth", Convert.ToDateTime(InputModel.date_Of_Birth).ToString("yyyy-MM-dd")));
            Parameters.Add(new SqlParameter("place_of_birth", InputModel.place_of_birth));
            Parameters.Add(new SqlParameter("place_of_birth_punjabi", InputModel.place_of_birth_punjabi));
            Parameters.Add(new SqlParameter("marital_status", InputModel.marital_status));
            Parameters.Add(new SqlParameter("marital_status_punjabi", InputModel.marital_status_punjabi));
            Parameters.Add(new SqlParameter("email_id", InputModel.email_id));
            Parameters.Add(new SqlParameter("is_email_verified", InputModel.is_email_verified));
            Parameters.Add(new SqlParameter("phone_no", InputModel.phone_no));
            Parameters.Add(new SqlParameter("is_mobile_verified", InputModel.is_mobile_verified));
            Parameters.Add(new SqlParameter("Occupation", InputModel.Occupation));
            Parameters.Add(new SqlParameter("Qualification", InputModel.Qualification));
            Parameters.Add(new SqlParameter("husband_first_name", InputModel.husband_first_name));
            Parameters.Add(new SqlParameter("husband_first_name_punjabi", InputModel.husband_first_name_punjabi));
            Parameters.Add(new SqlParameter("husband_middle_name", InputModel.husband_middle_name));
            Parameters.Add(new SqlParameter("husband_middle_name_punjabi", InputModel.husband_middle_name_punjabi));
            Parameters.Add(new SqlParameter("husband_last_name", InputModel.husband_last_name));
            Parameters.Add(new SqlParameter("husband_last_name_punjabi", InputModel.husband_last_name_punjabi));
            /*Permanent address parameters*/
            Parameters.Add(new SqlParameter("p_address_type", "P"));
            Parameters.Add(new SqlParameter("p_address_line_1", InputModel.p_address.address_line_1));
            Parameters.Add(new SqlParameter("p_address_line_2", InputModel.p_address.address_line_2));
            Parameters.Add(new SqlParameter("p_address_line_3", InputModel.p_address.address_line_3));
            Parameters.Add(new SqlParameter("p_region", InputModel.p_address.region));
            Parameters.Add(new SqlParameter("p_stateID", InputModel.p_address.stateID));
            Parameters.Add(new SqlParameter("p_districtID", InputModel.p_address.districtID));
            Parameters.Add(new SqlParameter("p_tehsilID", InputModel.p_address.tehsilID));
            Parameters.Add(new SqlParameter("p_blockID", InputModel.p_address.blockID));
            Parameters.Add(new SqlParameter("p_villageID", InputModel.p_address.villageID));
            Parameters.Add(new SqlParameter("p_pincode", InputModel.p_address.pincode));
            Parameters.Add(new SqlParameter("p_address_punjabi", InputModel.p_address.address_punjabi));
            /*Corrospondance address parameters*/
            Parameters.Add(new SqlParameter("c_address_type", "C"));
            Parameters.Add(new SqlParameter("c_address_line_1", InputModel.c_address.address_line_1));
            Parameters.Add(new SqlParameter("c_address_line_2", InputModel.c_address.address_line_2));
            Parameters.Add(new SqlParameter("c_address_line_3", InputModel.c_address.address_line_3));
            Parameters.Add(new SqlParameter("c_region", InputModel.c_address.region));
            Parameters.Add(new SqlParameter("c_stateID", InputModel.c_address.stateID));
            Parameters.Add(new SqlParameter("c_districtID", InputModel.c_address.districtID));
            Parameters.Add(new SqlParameter("c_tehsilID", InputModel.c_address.tehsilID));
            Parameters.Add(new SqlParameter("c_blockID", InputModel.c_address.blockID));
            Parameters.Add(new SqlParameter("c_villageID", InputModel.c_address.villageID));
            Parameters.Add(new SqlParameter("c_pincode", InputModel.c_address.pincode));
            Parameters.Add(new SqlParameter("c_address_punjabi", InputModel.c_address.address_punjabi));

            Parameters.Add(new SqlParameter("created_by", Citizen_Ref_ID));
            #endregion

            #region DB CALL & BUSINESS LOGIC
            _dt = _MSSQLGateway.ExecuteProcedure("INSERT_MD_COMMON_CITIZEN_PROFILE", Parameters);

            if (_objHelper.checkDBResponse(_dt))
            {
                if (Convert.ToInt32(Convert.ToString(_dt.Rows[0]["response"])) > 0)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(_dt.Rows[0]["response"]));
                    _objResponse.sys_message = Convert.ToString(_dt.Rows[0]["sys_message"].ToString());
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = Convert.ToString(_dt.Rows[0]["sys_message"].ToString());
                }
            }

            #endregion

            return _objResponse;
        }
        #endregion

        #region Get Citizen Detail
        [HttpGet("GetCitizenDetail")]
        public ServiceResponseModel GetCitizenDetail()
        {
            int? Citizen_Ref_ID = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Citizen_Ref_ID"));

            #region DB PARAMETERS
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("grievanceID", Citizen_Ref_ID));
            #endregion

            #region DB CALL & BUSINESS LOGIC
            _ds = _MSSQLGateway.ExecuteProcedureWithDataset("APP_FETCH_COMMON_CITIZEN_PROFILE", Parameters);
            if (_objHelper.checkDBResponse(_ds))
            {
                if (Convert.ToInt32(Convert.ToString(_ds.Tables[0].Rows[0]["response"])) > 0)
                {
                    _objResponse.response = 1;
                    List<object> _list = new List<object>();
                    for (int i = 0; i < _ds.Tables.Count; i++)
                    {
                        _list.Add(_objHelper.ConvertTableToDictionary(_ds.Tables[i]));
                    }
                    _objResponse.data = _list;

                    //_objResponse.data = _objHelper.ConvertTableToDictionary();
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = Convert.ToString(_ds.Tables[0].Rows[0]["sys_message"].ToString());
                }
            }
            #endregion

            return _objResponse;
        }
        #endregion

        #region Check eSewa User Detail
        [HttpGet("CheckeSewaUser")]
        public async Task<ServiceResponseModel> CheckeSewaUserAsync()
        {
            try
            {
                int? Citizen_Ref_ID = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Citizen_Ref_ID"));

                #region DB PARAMETERS
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("grievanceID", Citizen_Ref_ID));
                #endregion

                #region DB CALL & BUSINESS LOGIC
                _ds = _MSSQLGateway.ExecuteProcedureWithDataset("APP_FETCH_COMMON_CITIZEN_PROFILE", Parameters);
                if (_objHelper.checkDBResponse(_ds))
                {
                    if (Convert.ToInt32(Convert.ToString(_ds.Tables[0].Rows[0]["response"])) > 0)
                    {
                        _objResponse.response = 1;
                        EsewaCitizenProfileModel _eSewaUser = new EsewaCitizenProfileModel();
                        _eSewaUser.CitizenID = Convert.ToInt64(_ds.Tables[0].Rows[0]["eSewaID"]);
                        _eSewaUser.Firstname = Convert.ToString(_ds.Tables[0].Rows[0]["first_name"]);
                        _eSewaUser.Middlename = Convert.ToString(_ds.Tables[0].Rows[0]["middle_name"]);
                        _eSewaUser.Lastname = Convert.ToString(_ds.Tables[0].Rows[0]["last_name"]);

                        _eSewaUser.Fatherfirstname = Convert.ToString(_ds.Tables[0].Rows[0]["father_first_name"]);
                        _eSewaUser.Fathermiddlename = Convert.ToString(_ds.Tables[0].Rows[0]["father_middle_name"]);
                        _eSewaUser.Fatherlastname = Convert.ToString(_ds.Tables[0].Rows[0]["father_last_name"]);

                        _eSewaUser.Motherfirstname = Convert.ToString(_ds.Tables[0].Rows[0]["mother_first_name"]);
                        _eSewaUser.Mothermiddlename = Convert.ToString(_ds.Tables[0].Rows[0]["mother_middle_name"]);
                        _eSewaUser.Motherlastname = Convert.ToString(_ds.Tables[0].Rows[0]["mother_last_name"]);

                        _eSewaUser.Gender = Convert.ToString(_ds.Tables[0].Rows[0]["gender"]);
                        _eSewaUser.Dob = Convert.ToString(_ds.Tables[0].Rows[0]["dob"]);
                        _eSewaUser.Maritalstatus = Convert.ToString(_ds.Tables[0].Rows[0]["marital_status"]);
                        _eSewaUser.Mobileno = Convert.ToString(_ds.Tables[0].Rows[0]["phone_no"]);
                        _eSewaUser.email = Convert.ToString(_ds.Tables[0].Rows[0]["email_id"]);
                        _eSewaUser.Husbandfirstname = Convert.ToString(_ds.Tables[0].Rows[0]["husband_first_name"]);
                        _eSewaUser.Husbandmiddlename = Convert.ToString(_ds.Tables[0].Rows[0]["husband_middle_name"]);
                        _eSewaUser.Husbandlastname = Convert.ToString(_ds.Tables[0].Rows[0]["husband_last_name"]);

                        _eSewaUser.Husbandfirstnamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["husband_first_name_punjabi"]);
                        _eSewaUser.Husbandmiddlenamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["husband_middle_name_punjabi"]);
                        _eSewaUser.Husbandlastnamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["husband_last_name_punjabi"]);

                        _eSewaUser.Userid = "";
                        _eSewaUser.Maritalstatuspunjabi = "";
                        _eSewaUser.Placeofbirthpunjabi = "";
                        _eSewaUser.Genderpunjabi = "";

                        _eSewaUser.Firstnamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["first_name_punjabi"]);
                        _eSewaUser.Middlenamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["middle_name_punjabi"]);
                        _eSewaUser.Lastnamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["last_name_punjabi"]);
                        _eSewaUser.Fatherfirstnamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["father_first_name_punjabi"]);
                        _eSewaUser.Fathermiddlenamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["father_middle_name_punjabi"]);
                        _eSewaUser.Fatherlastnamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["father_last_name_punjabi"]);
                        _eSewaUser.Motherfirstnamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["mother_first_name_punjabi"]);
                        _eSewaUser.Mothermiddlenamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["mother_middle_name_punjabi"]);
                        _eSewaUser.Motherlastnamepunjabi = Convert.ToString(_ds.Tables[0].Rows[0]["mother_last_name_punjabi"]);

                        _eSewaUser.address_line_1 = Convert.ToString(_ds.Tables[1].Rows[0]["address_line_1"]);
                        _eSewaUser.address_line_2 = Convert.ToString(_ds.Tables[1].Rows[0]["address_line_2"]);
                        _eSewaUser.address_line_3 = Convert.ToString(_ds.Tables[1].Rows[0]["address_line_3"]);
                        _eSewaUser.region = Convert.ToString(_ds.Tables[1].Rows[0]["region"]);
                        _eSewaUser.stateID = Convert.ToInt64(_ds.Tables[1].Rows[0]["stateID"]);
                        _eSewaUser.districtID = Convert.ToInt64(_ds.Tables[1].Rows[0]["districtID"]);
                        _eSewaUser.tehsilID = Convert.ToInt64(_ds.Tables[1].Rows[0]["tehsilID"]);
                        _eSewaUser.blockID = Convert.ToInt64(_ds.Tables[1].Rows[0]["blockID"]);
                        _eSewaUser.villageID = Convert.ToInt64(_ds.Tables[1].Rows[0]["villageID"]);
                        _eSewaUser.pincode = Convert.ToInt64(_ds.Tables[1].Rows[0]["pincode"]);
                        _eSewaUser.address_line_1pb = Convert.ToString(_ds.Tables[1].Rows[0]["address_punjabi"]);

                        _eSewaUser.address_line_1_c = Convert.ToString(_ds.Tables[1].Rows[1]["address_line_1"]);
                        _eSewaUser.address_line_2_c = Convert.ToString(_ds.Tables[1].Rows[1]["address_line_2"]);
                        _eSewaUser.address_line_3_c = Convert.ToString(_ds.Tables[1].Rows[1]["address_line_3"]);
                        _eSewaUser.region_c = Convert.ToString(_ds.Tables[1].Rows[1]["region"]);
                        _eSewaUser.stateID_c = Convert.ToInt64(_ds.Tables[1].Rows[1]["stateID"]);
                        _eSewaUser.districtID_c = Convert.ToInt64(_ds.Tables[1].Rows[1]["districtID"]);
                        _eSewaUser.tehsilID_c = Convert.ToInt64(_ds.Tables[1].Rows[1]["tehsilID"]);
                        _eSewaUser.blockID_c = Convert.ToInt64(_ds.Tables[1].Rows[1]["blockID"]);
                        _eSewaUser.villageID_c = Convert.ToInt64(_ds.Tables[1].Rows[1]["villageID"]);
                        _eSewaUser.pincode_c = Convert.ToInt64(_ds.Tables[1].Rows[1]["pincode"]);
                        _eSewaUser.address_line_1_cpb = Convert.ToString(_ds.Tables[1].Rows[1]["address_punjabi"]);

                        string _ProfileInput = JsonConvert.SerializeObject(_eSewaUser);
                        string token = Request.Headers["Authorization"].ToString();

                        ServiceResponseModel resAccessKey = await UserWebRequestWithBody(this.eSewaURL + this.eSewaCreateCitizenProfileAPI, "POST", _ProfileInput, token);

                        List<SqlParameter> _parm = new List<SqlParameter>();
                        ServiceResponseModel _rmProfile = (ServiceResponseModel)resAccessKey.data;
                        if (this._objHelper.checkResponseModel(_rmProfile))
                        {
                            // Declare JArray for getting data from response model
                            JArray _obj = (JArray)_rmProfile.data;
                            if (this._objHelper.checkJArray(_obj))
                            {
                                DataTable responseDataTable = new DataTable();
                                if (Convert.ToString(_obj[0]["response"].ToString()) == "1")
                                {
                                    await UploadProfilePicture(token, _ds);

                                    #region DB PARAMETERS
                                    _parm.Add(new SqlParameter("userID", _ds.Tables[0].Rows[0]["userID"]));
                                    _parm.Add(new SqlParameter("eSewa_id", Convert.ToString(_obj[0]["CitizenID"].ToString())));
                                    #endregion

                                    #region DB CALL & BUSINESS LOGIC
                                    responseDataTable = _MSSQLGateway.ExecuteProcedure("UPADTE_ESEWAID_COMMON_CITIZEN_PROFILE", _parm);
                                    #endregion

                                    DataColumn userID = new DataColumn("userID", typeof(string));
                                    userID.DefaultValue = _ds.Tables[0].Rows[0]["userID"].ToString();
                                    responseDataTable.Columns.Add(userID);
                                    /*Short Token For eSewa*/
                                    responseDataTable.Columns.Add(new DataColumn("TokenID", typeof(string)));
                                    responseDataTable.Rows[0]["TokenID"] = Convert.ToString(_obj[0]["TokenID"].ToString());
                                    /*Citizen ID*/
                                    responseDataTable.Columns.Add(new DataColumn("CitizenRef", typeof(string)));
                                    responseDataTable.Rows[0]["CitizenRef"] = Convert.ToString(_obj[0]["CitizenRef"].ToString());
                                    /*Citizen User ID*/
                                    responseDataTable.Columns.Add(new DataColumn("uidExternal", typeof(string)));
                                    responseDataTable.Rows[0]["uidExternal"] = Convert.ToString(_obj[0]["uidExternal"].ToString());
                                    /*Citizen Mobile Number*/
                                    responseDataTable.Columns.Add(new DataColumn("mobile", typeof(string)));
                                    responseDataTable.Rows[0]["mobile"] = Convert.ToString(_ds.Tables[0].Rows[0]["phone_no"]);

                                    _objResponse.response = 1;
                                    _objResponse.data = _objHelper.ConvertTableToDictionary(responseDataTable);
                                }
                                else
                                {
                                    _objResponse.response = 0;
                                    _objResponse.sys_message = "We are unable to process your request. Please try again later.";
                                }
                            }
                        }
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.sys_message = Convert.ToString(_ds.Tables[0].Rows[0]["sys_message"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "We are unable to process your request. Please try again later.";
            }
            #endregion

            return _objResponse;
        }

        private async Task UploadProfilePicture(string token, DataSet _ds)
        {
            try
            {
                if (Convert.ToInt64(_ds.Tables[0].Rows[0]["eSewaID"]) > 0 && _ds.Tables[2].Rows.Count > 0)
                {
                    _DocumentController = new DocumentController(_configuration, this._env);

                    ServiceResponseModel _docresponse = new ServiceResponseModel();
                    _docresponse = await _DocumentController.GetDocumentforesewauser(Convert.ToInt64(_ds.Tables[2].Rows[0]["docid"]));

                    Dictionary<string, string> _responseOfDocument = (Dictionary<string, string>)_docresponse.data;

                    if (_responseOfDocument != null)
                    {
                        MediaFileModelForeSewa mediaFileModel = new MediaFileModelForeSewa()
                        {
                            files = new List<InputFileForeSewa>
                                                {
                                                    new InputFileForeSewa
                                                    {
                                                        base64 = _responseOfDocument["base64"],
                                                        filetype = _responseOfDocument["mime"],
                                                        userid = Convert.ToString(_ds.Tables[0].Rows[0]["eSewaID"])
                                                    }
                                                }
                        };

                        string _DocumentInput = JsonConvert.SerializeObject(mediaFileModel);

                        await UserWebRequestWithBody(this.eSewaURL + this.eSewaUploadProfilePictureAPI, "POST", _DocumentInput, token);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        public async Task<ServiceResponseModel> UserWebRequestWithBody(string URL, string Method, string body, string token = null)
        {
            ServiceResponseModel rm = new ServiceResponseModel();
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = Method;
                //  string token1 = token.Replace("bearer ", "");
                httpWebRequest.Headers.Add("Authorization", token);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = body;
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    rm.data = JsonConvert.DeserializeObject<ServiceResponseModel>(result);
                }
                return rm;
            }
            catch (Exception ex)
            {
                return rm;
            }
        }
    }
}
