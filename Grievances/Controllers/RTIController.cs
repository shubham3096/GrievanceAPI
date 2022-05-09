using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GrievanceService.Controllers
{
    [Route("api/rti")]
    public class RTIController : Controller
    {
        #region Controller Properties
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        CommonFunctions APICall;
        string GetActor = string.Empty;
        string rti_URL = String.Empty;
        string rti_API1 = string.Empty;
        string rti_API2= string.Empty;
        string username = string.Empty;
        string password = string.Empty;
        #endregion

        public RTIController(IConfiguration configuration, IHostingEnvironment env)
        {
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev")); 
                this.rti_URL = this._configuration["AppSettings_Dev:RTI:URL"];
                this.rti_API1 = this._configuration["AppSettings_Dev:RTI:APIKey1"];
                this.rti_API2= this._configuration["AppSettings_Dev:RTI:APIKey2"];
                this.username = this._configuration["AppSettings_Dev:RTI:username"];
                this.password = this._configuration["AppSettings_Dev:RTI:password"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.rti_URL = this._configuration["AppSettings_Stag:RTI:URL"];
                this.rti_API1 = this._configuration["AppSettings_Stag:RTI:APIKey1"];
                this.rti_API2 = this._configuration["AppSettings_Stag:RTI:APIKey2"];
                this.username = this._configuration["AppSettings_Stag:RTI:username"];
                this.password = this._configuration["AppSettings_Stag:RTI:password"];

            }
            else if (env.IsProduction())
            {
                this.rti_URL = this._configuration["AppSettings_Pro:RTI:URL"];
                this.rti_API1 = this._configuration["AppSettings_Pro:RTI:APIKey1"];
                this.rti_API2 = this._configuration["AppSettings_Pro:RTI:APIKey2"];
                this.username = this._configuration["AppSettings_Pro:RTI:username"];
                this.password = this._configuration["AppSettings_Pro:RTI:password"];
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
            }
        }

        #region CreateApplication
        [HttpPost("CreateApplication")]
        [AllowAnonymous]
        public  async Task<ServiceResponseModel> CreateApplication()
        {
            
            var keyValues = new List<KeyValuePair<string, string>>();            
            FormUrlEncodedContent data = new FormUrlEncodedContent(keyValues);
            var url = this.rti_URL+ "application-masters";
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
                var response = await httpClient.PostAsync(url, data);
                string result = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var res = (JObject)JsonConvert.DeserializeObject(result);
                    _objResponse.data = res;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.data = null;
                    _objResponse.response =0;
                    _objResponse.sys_message = "Failed";
                    _objResponse.response_code = "410";
                }
            }
            return _objResponse;
        }
        #endregion

        #region GetStates
        [HttpGet("GetStates")]
        [AllowAnonymous]
        public ServiceResponseModel GetStates()
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("Get_States", Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (dtresp.Rows.Count > 0)
                {
                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    _objResponse.sys_message = "Data Return Successfully";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "No Record Found.";
                    _objResponse.response_code = "200";
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.data = null;
                _objResponse.sys_message = "No Record Found.";
                _objResponse.response_code = "503";
            }
            #endregion
            return _objResponse;
        }
        #endregion

        #region GetLocationDetails
        [HttpPost("GetLocationDetails")]
        [AllowAnonymous]
        public ServiceResponseModel GetLocationDetails([FromBody]LocationModel model)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("StateId", model.StateId));
            Parameters.Add(new SqlParameter("DistId", model.DistId));
            Parameters.Add(new SqlParameter("TehId", model.TehId));
            Parameters.Add(new SqlParameter("VillId", model.VillId));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("Get_Location_Details", Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (dtresp.Rows.Count > 0)
                {
                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    _objResponse.sys_message = "data return successfully";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "no record found.";
                    _objResponse.response_code = "200";
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.data = null;
                _objResponse.sys_message = "Service Unavailable.";
                _objResponse.response_code = "503";
            }
            #endregion
            return _objResponse;
        }
        #endregion

        #region CitizenApplicationMaster
        [HttpPost("CitizenApplicationMaster")]
        public async Task<ServiceResponseModel> CitizenApplicationMasterAsync([FromBody]ApplicantDetails applicantDetails)
        {
            applicantDetails.dateOfBirth = Convert.ToDateTime(applicantDetails.dateOfBirth).ToString("yyyy-MM-dd");
            var url = this.rti_URL+"citizen-application-masters";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
           if(applicantDetails != null)
            {
                 var json = JsonConvert.SerializeObject(applicantDetails);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
           
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await  httpClient.SendAsync(request);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = (JObject)JsonConvert.DeserializeObject(result);
                    _objResponse.response_code = "201";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";
                }
            }
            return _objResponse;
        }
        #endregion

        #region ApplicantDetails
        [HttpGet("GetCitizenApplicationDetails/{applicationId}")]
        public async Task<ServiceResponseModel> CitizenApplicationDetailsAsync(long  applicationId)
        {
            var url = this.rti_URL+"citizen-application-masters/";
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                var response = await httpClient.GetAsync(url+applicationId);
                HttpResponseMessage _ResponseMessage = new HttpResponseMessage();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = (JObject)JsonConvert.DeserializeObject(result);
                    _objResponse.response_code = "201";

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region GetCitizenByUserId
        [HttpGet("GetCitizenByUserId/{userId}")]
        public async Task<ServiceResponseModel> GetCitizenByUserIdAsync(long userId)
        {
            var url = this.rti_URL + "citizen-application-masters/get-citizen-by-userid/";
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url + userId);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    
                    var res = JsonConvert.DeserializeObject<dynamic>(result);
                    //   var res = (JObject)JsonConvert.DeserializeObject(result);
                    if (res["data"] == null )
                    {
                        _objResponse.response = 0;
                        _objResponse.data = null;
                        _objResponse.sys_message = "Not Found";
                        _objResponse.response_code = "404";
                    }
                    else
                    {
                            JObject jObj = JObject.Parse(JsonConvert.SerializeObject(res));
                            if (Convert.ToInt32(res["data"]["departmentMaster"]) != 0 && Convert.ToInt32(res["data"]["publicAuthorityMaster"])!=0)
                            {
                                GrievanceV2Controller _grievanceV2 = new GrievanceV2Controller(_configuration, _env);
                                EAapiModel eAapi = new EAapiModel()
                                {
                                    deptid = res["data"]["departmentMaster"],
                                    SelectBy = "singledept",
                                    subdept = res["data"]["publicAuthorityMaster"]
                                };
                                var resDepartment = _grievanceV2.master(eAapi);

                                 JObject jOdbjDept = JObject.Parse(JsonConvert.SerializeObject(resDepartment));

                                jObj.Add(new JProperty("departmentMaster", jOdbjDept["data"][0]["Stakeholder_Name"].ToString()));
                                jObj.Add(new JProperty("publicAuthorityMaster", jOdbjDept["data"][1]["Stakeholder_Name"]));
                            }
                    
                            if (Convert.ToInt32(res["data"]["state"]) != 0)
                            {
                                RTIController rTI = new RTIController(_configuration, _env);
                                LocationModel locationModel = new LocationModel()
                                {
                                    DistId = res["data"]["district"] == null ? (int?)null : Convert.ToInt32(res["data"]["district"]),
                                    StateId = res["data"]["state"] == null ? (int?)null : Convert.ToInt32(res["data"]["state"]),
                                    VillId = res["data"]["village"] == null ? (int?)null : Convert.ToInt32(res["data"]["village"]),
                                    TehId = res["data"]["subDistrictTehsil"] == null ? (int?)null : Convert.ToInt32(res["data"]["subDistrictTehsil"])
                                };
                                var resLocations = rTI.GetLocationDetails(locationModel);

                                JObject jOdbjLoc = JObject.Parse(JsonConvert.SerializeObject(resLocations));
                                var x = JObject.Parse(jOdbjLoc["data"][0].ToString());
                                jObj.Add(new JProperty("stateName", jOdbjLoc["data"][0]["State_Name"].ToString()));
                      
                                if (x.ContainsKey("District_Name")) { 
                                     jObj.Add(new JProperty("districtName", jOdbjLoc["data"][0]["District_Name"].ToString()));
                                }
                                if (x.ContainsKey("Tehsil_Name"))
                                {
                                    jObj.Add(new JProperty("tehsilName", jOdbjLoc["data"][0]["Tehsil_Name"].ToString()));
                                }
                                if (x.ContainsKey("Village_Name"))
                                {
                                    jObj.Add(new JProperty("villageName", jOdbjLoc["data"][0]["Village_Name"].ToString()));
                                }
                            }

                            var endResult = JsonConvert.DeserializeObject(jObj.ToString());
                            _objResponse.response = 1;
                            _objResponse.data = endResult;
                            _objResponse.sys_message = "Successful";
                            _objResponse.response_code = "200";
                    }
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region ApplicationHistory
        [HttpPost("ApplicationHistory")]
        public async Task<ServiceResponseModel> ApplicationHistoryAsync([FromBody]PagingModel model )
        {
            string url = string.Empty;
            if (model.applicationId == null)
            {
                 url = this.rti_URL + "citizen-application-masters/citizen-applications/" + model.userId+"?page="+ model.pageNumber +"&size="+ model.pageSize +"&sort="+ model.sortBy[0]+","+ model.sortBy[1];
            }
            else
            {
                 url = this.rti_URL + "citizen-application-masters/citizen-applications/" + model.userId + "?page=" + model.pageNumber + "&size=" + model.pageSize + "&sort=" + model.sortBy[0] + "," + model.sortBy[1]+ "&applicationId="+model.applicationId;
            }
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                var response = await httpClient.GetAsync(url);               
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = response.Content.ReadAsStringAsync().Result; //  \n json string data 
                    var resData = JsonConvert.DeserializeObject<dynamic>(result);
                   
                    HttpHeaders headers = response.Headers;
                    string session = string.Empty;
                    IEnumerable<string> values;
                    if (headers.TryGetValues("X-Total-Count", out values))
                    {
                        session = values.First();     
                    }
                    Dictionary<dynamic,dynamic> lstDynamics = new Dictionary<dynamic,dynamic>();
                    lstDynamics.Add("Result",resData);
                    lstDynamics.Add("TotalCount",session);
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = lstDynamics;  // serilize object
                    _objResponse.response_code = "200";

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region CitizenAppealMaster
        [HttpPost("CitizenAppealMaster")]
        public async Task<ServiceResponseModel> CitizenAppealMasterAsync([FromBody] CitizenAppealModel model)
        {
            var url = this.rti_URL + "citizen-appeal-masters";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            if (model != null)
            {
                var json = JsonConvert.SerializeObject(model);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.SendAsync(request);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = (JObject)JsonConvert.DeserializeObject(result);
                    _objResponse.response_code = "201";
                }
                else
                {
                     string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 0;
                    if (result != null)
                    {
                        _objResponse.data = (JObject)JsonConvert.DeserializeObject(result);
                        _objResponse.sys_message = "Something went wrong";
                        _objResponse.response_code = "400";
                    }else
                    {
                        _objResponse.data = null;
                        _objResponse.sys_message = "Service Unavailable";
                        _objResponse.response_code = "503";
                    } // close if-else check result is not empty
                }
            }
            return _objResponse;
        }
        #endregion

        #region GetCitizenTrailsByUserId
        [HttpGet("GetCitizenTrailsByRtiId/{rtiId}")]
        public async Task<ServiceResponseModel> GetCitizenTrailsByRtiIdAsync(long rtiId)
        {
            var url = this.rti_URL + "citizen-application-masters/view-trail/"+rtiId;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.sys_message = "Successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region GetRoles
        [HttpGet("GetRoles")]
        public async Task<ServiceResponseModel> GetRolesAsync()
        {
            var url = this.rti_URL + "roles";
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.response_code = "200";

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region RoleAssignment
        [HttpPost("RoleAssignment")]
        public async Task<ServiceResponseModel> RoleAssignmentAsync([FromBody] RoleModel model)
        {
            var url = this.rti_URL + "role-assignment";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            if (model != null)
            {
                var json = JsonConvert.SerializeObject(model);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.SendAsync(request);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = (JObject)JsonConvert.DeserializeObject(result);
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";
                }
            }
            return _objResponse;
        }
        #endregion

        #region RoleAssignmentById
        [HttpGet("RoleAssignmentByActorRefId/{actorRefId}")]
        public async Task<ServiceResponseModel> RoleAssignmentByActorRefId(long actorRefId)
        {
            var url = this.rti_URL + "role-assignment/" + actorRefId;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.sys_message = "Successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region AppealGroundsList
        [HttpGet("AppealGroundsList/{appealType}")]
        public async Task<ServiceResponseModel> AppealGroundsListAsync(int appealType)
        {
            var url = this.rti_URL + "appeal-grounds/get-lst/"+appealType;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.response_code = "200";

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }

        #endregion

        #region GetLevelList
        [HttpGet("GetLevelList")]
        public async Task<ServiceResponseModel> GetLevelListAsync()
        {
            var url = this.rti_URL + "actor-level";
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.response_code = "200";

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }

        #endregion

        #region RelieveRoleAssignmentByActorRefId
        [HttpPut("RelieveRoleAssignmentByActorRefId/{actorRefId}")]
        public async Task<ServiceResponseModel> RelieveRoleAssignmentByActorRefIdAsync(long actorRefId)
        {
            var url = this.rti_URL + "role-assignment/relieve-actor/" + actorRefId;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                
                HttpResponseMessage _ResponseMessage = await httpClient.PutAsync(url, null);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.sys_message = "Successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region RtiTransaction
        [HttpPost("RTITransaction")]
        public async Task<ServiceResponseModel> RTITransactionAsync([FromBody] TransactionModel model)
        {
            var url = this.rti_URL + "payment/process-payment";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            if (model != null)
            {
                var json = JsonConvert.SerializeObject(model);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.SendAsync(request);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = (JObject)JsonConvert.DeserializeObject(result);
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";
                }
            }
            return _objResponse;
        }
        #endregion

        #region PaymentReceiptGenerate
        [HttpPost("GetTransactionReceipt")]
        public async Task<ServiceResponseModel> GetTransactionReceiptAsync([FromBody] ReceiptModel model)
        {
            var url = this.rti_URL + "payment-received/payment-receipt?id=" + model.id + "&fetch_by=" + model.fetch_by;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                var response = await httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = response.Content.ReadAsStringAsync().Result; 
                    var resData = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = resData;  // serilize object
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";
                }
            }
            return _objResponse;
        }
        #endregion

        #region GetActorCountReport
        [HttpPost("GetActorCountReport")]
        public async Task<ServiceResponseModel> GetActorCountReportAsync([FromBody] List<ActorListModel> model)
        {
            var url = this.rti_URL + "role-assignment/get-actors-count-report";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            if (model != null)
            {
                var json = JsonConvert.SerializeObject(model);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.SendAsync(request);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = (JObject)JsonConvert.DeserializeObject(result);
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";
                }
            }
            return _objResponse;
        }
        #endregion

        #region GetActorSearchReport
        [HttpPost("GetActorSearchReport")]
        [Authorize]
        public async Task<ServiceResponseModel> GetActorSearchReportAsync([FromBody] ActorSearchModel model)
        {
            var accessToken = (HttpContext.Request.Headers["Authorization"].ToString()).Replace("Bearer ","");
            var url = this.rti_URL + "role-assignment/get-actors-search-report";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            if (model != null)
            {
                var json = JsonConvert.SerializeObject(model);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                httpClient.DefaultRequestHeaders.Add("Griev-Token", accessToken);
                HttpResponseMessage _ResponseMessage = await httpClient.SendAsync(request);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = (JObject)JsonConvert.DeserializeObject(result);
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";
                }
            }
            return _objResponse;
        }
        #endregion

        #region GetOnBoardStatusRTI
        [HttpPost("GetOnBoardStatusRTI")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetOnBoardStatusRTI([FromBody] DepartmentOnBoardStatusModel model)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Search_by", model.Search_by));
            Parameters.Add(new SqlParameter("OnBoard_status", model.OnBoard_status));
            Parameters.Add(new SqlParameter("Dept_id", model.Dept_id));

            #region DB CALL & BUSINESS LOGIC
            DataTable dtresponse = _MSSQLGateway.ExecuteProcedure("Get_RTI_Onboard_Status", Parameters);
            if (_objHelper.checkDBResponse(dtresponse))
            {
                if (Convert.ToInt64(dtresponse.Rows[0]["response"]) == 1)
                {
                    List<ActorListModel> actorList = new List<ActorListModel>();
                    for (int i = 0; i < dtresponse.Rows.Count; i++)
                    {
                        ActorListModel actor = new ActorListModel();
                        actor.id = Convert.ToInt32(dtresponse.Rows[i]["Stakeholder_ID"]);
                        actor.isactive = Convert.ToBoolean(dtresponse.Rows[i]["Rti_Is_OnBoard"]);
                        actorList.Add(actor);
                    }
                    await GetActorCountReportAsync(actorList);
                }
                else
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresponse.Rows[0]["response"]));
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresponse);
                    _objResponse.sys_message ="Failed";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion
        #endregion

        #region GetOnBoardDepartment
        [HttpPost("GetOnBoardDepartment")]
        [AllowAnonymous]
        public ServiceResponseModel GetOnBoardDepartmentAsync([FromBody] DepartmentOnBoardStatusModel model)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Search_by", model.Search_by));
            Parameters.Add(new SqlParameter("OnBoard_status", model.OnBoard_status));

            #region DB CALL & BUSINESS LOGIC
            DataTable dtresponse =  _MSSQLGateway.ExecuteProcedure("Get_RTI_Onboard_Status", Parameters);
            if (_objHelper.checkDBResponse(dtresponse))
            {
                if (Convert.ToInt64(dtresponse.Rows[0]["response"]) == 1)
                {
                    if (dtresponse.Rows.Count > 0)
                    {
                        _objResponse.response = 1;
                        _objResponse.data = _objHelper.ConvertTableToDictionary(dtresponse);
                        _objResponse.sys_message = "data return successfully";
                        _objResponse.response_code = "200";
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.data = null;
                        _objResponse.sys_message = "no record found.";
                        _objResponse.response_code = "200";
                    }
                }
                else
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresponse.Rows[0]["response"]));
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresponse);
                    _objResponse.sys_message = "Failed";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion
        #endregion

        #region GetActorOffice
        [HttpPost("GetActorOffice")]
        [Authorize]
        public async Task<ServiceResponseModel> GetActorOfficeAsync([FromBody] ActorOfficeModel model)
        {
            var accessToken = (HttpContext.Request.Headers["Authorization"].ToString()).Replace("Bearer ", "");
            var url = this.rti_URL + "role-assignment/get-actor-with-office";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            if (model != null)
            {
                var json = JsonConvert.SerializeObject(model);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                httpClient.DefaultRequestHeaders.Add("Griev-Token", accessToken);
                HttpResponseMessage _ResponseMessage = await httpClient.SendAsync(request);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";
                }
            }
            return _objResponse;
        }
        #endregion


        #region UpdateOnBoardStatus
        [HttpPost("UpdateOnBoardStatus")]
        [AllowAnonymous]
        public ServiceResponseModel UpdateOnBoardStatus([FromBody] UpdateOnBoardModel model)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Dept_id", model.Dept_id));
            Parameters.Add(new SqlParameter("Status", model.Status));
            Parameters.Add(new SqlParameter("P_id", model.P_id));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("Update_Rti_OnBoard_Status", Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (dtresp.Rows[0]["response"].ToString() == "1")
                {
                    _objResponse.response = 1;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Record Updated Successfully.";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Failed";
                    _objResponse.response_code = "503";
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.data = null;
                _objResponse.sys_message = "No Record Found.";
                _objResponse.response_code = "503";
            }
            #endregion
            return _objResponse;
        }
        #endregion

        #region GetDistrictsByDeptId
        [HttpGet("GetDistrictsByDeptId/{deptId}")]
        public async Task<ServiceResponseModel> GetDistrictsByDeptIdAsync(long deptId)
        {
            var url = this.rti_URL + "role-assignment/get-district-by-stakeholder?stakeholder_id=" + deptId;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.sys_message = "Successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion


        #region GetRTICountByCitizenId
        [HttpGet("GetRTICountByCitizenId/{CitizenId}")]
        public async Task<ServiceResponseModel> GetRTICountByCitizenId(long CitizenId)
        {
            var url = this.rti_URL + "citizen-application-masters/rti-count/" + CitizenId;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.sys_message = "Successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region GetTransactionReceiptFailure
        [HttpPost("GetTransactionReceiptFailure")]
        public async Task<ServiceResponseModel> GetTransactionReceiptFailure([FromBody] ReceiptModel model)
        {
            var url = this.rti_URL + "payment-received/payment-failure-receipt?id=" + model.id + "&fetch_by=" + model.fetch_by;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                var response = await httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var resData = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = resData;  // serilize object
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";
                }
            }
            return _objResponse;
        }
        #endregion

        #region VerifyChallan
        [HttpGet("VerifyChallan/{challanId}")]
        public async Task<ServiceResponseModel> VerifyChallan(long challanId)
        {
            var url = this.rti_URL + "payment/verify-challan/" + challanId;
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = result;
                    _objResponse.response_code = "200";

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion


        #region GetOnBoardDepartmentActiveActors
        [HttpPost("GetOnBoardDepartmentActiveActors")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetOnBoardDepartmentActiveActorsAsync([FromBody] DepartmentOnBoardActiveActorModel model)
        {
            
            List<SqlParameter> Parameters = new List<SqlParameter>();
            if (model.publicAuthorityId != 0)
            {
                Parameters.Add(new SqlParameter("Search_by", "Department"));
                Parameters.Add(new SqlParameter("OnBoard_status", Convert.ToBoolean(true)));
                Parameters.Add(new SqlParameter("Dept_id", model.publicAuthorityId));
            }
            else
            {
                Parameters.Add(new SqlParameter("Search_by", "All"));
                Parameters.Add(new SqlParameter("OnBoard_status", Convert.ToBoolean(true)));
            }
            DataTable dtresponse = _MSSQLGateway.ExecuteProcedure("Get_RTI_Onboard_Status", Parameters);
            if (dtresponse.Rows.Count > 0)
            {
                List<long> stkIds = new List<long>();
                for (var n = 0; n < dtresponse.Rows.Count; n++)
                {
                    stkIds.Add(Convert.ToInt32(dtresponse.Rows[n]["Stakeholder_ID"]));
                }
                model.stakeholderIds = stkIds;
                var url = this.rti_URL + "role-assignment/get-active-pio-lst";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                if (model != null)
                {
                    var json = JsonConvert.SerializeObject(model);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                using (var httpClient = new HttpClient())
                {
                    var username = this.username;
                    var password = this.password;
                    string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                                   .GetBytes(username + ":" + password));
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                    HttpResponseMessage _ResponseMessage = await httpClient.SendAsync(request);
                    if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _objResponse = _ResponseMessage.Content.ReadAsAsync<ServiceResponseModel>().Result;
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.data = null;
                        _objResponse.sys_message = "Service Unavailable";
                        _objResponse.response_code = "503";

                    }
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.data = null;
                _objResponse.sys_message = "no record found.";
                _objResponse.response_code = "200";
            }
            return _objResponse;
        }
        #endregion


        #region GetLevelListByStakeholder
        [HttpGet("GetLevelListByStakeholder/{stakeholderId}")]
        public async Task<ServiceResponseModel> GetLevelListByStakeholderAsync(long stakeholderId)
        {
            var url = this.rti_URL + "stakeholder-actor-level";
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url+"/"+stakeholderId);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.response_code = "200";

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion

        #region CheckAppealSubmission
        [HttpGet("CheckAppealSubmission/{citizenAppId}/{appealType}")]
        public async Task<ServiceResponseModel> CheckAppealSubmissionAsync(long citizenAppId, long appealType)
        {
            var url = this.rti_URL + "citizen-appeal-masters/check-appeal-submission";
            using (var httpClient = new HttpClient())
            {
                var username = this.username;
                var password = this.password;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url + "/" + citizenAppId+"/"+appealType);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Successful";
                    _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                    _objResponse.response_code = "200";

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.data = null;
                    _objResponse.sys_message = "Service Unavailable";
                    _objResponse.response_code = "503";

                }
            }
            return _objResponse;
        }
        #endregion


}


}
