using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GrievanceService.Controllers
{
    [Route("api/pmidc")]
    public class PMIDCController :Controller
    {
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        PmidcLoginModel pmidcLoginModel = new PmidcLoginModel();
        CommonFunctions APICall;
        private CommonHelper _objHelper = new CommonHelper();
        private CommonFunctions _objHelperLoc;
        private MSSQLGateway _MSSQLGateway;
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        public string pmidc_authAPIUrl = string.Empty;
        public string pmidc_authAPIkey = string.Empty;
        public string pmidc_createAPIUrl = string.Empty;
        public string pmidc_serachAPIUrl = string.Empty;

        public PMIDCController(IConfiguration configuration, IHostingEnvironment env)
        {
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            _objHelperLoc = new CommonFunctions(configuration, env);

            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this.pmidc_authAPIkey = this._configuration["AppSettings_Dev:PMIDC:PMIDC_API_Token_Key"];
                this.pmidc_authAPIUrl = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Auth_API_Url"];
                this.pmidc_createAPIUrl = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Create_API_Url"];
                this.pmidc_serachAPIUrl = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Search_API_Url"];
                pmidcLoginModel.grant_type = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Credentials:grant_type"];
                pmidcLoginModel.username = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Credentials:username"];
                pmidcLoginModel.password = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Credentials:password"];
                pmidcLoginModel.tenantId = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Credentials:tenantId"];
                pmidcLoginModel.scope = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Credentials:scope"];
                pmidcLoginModel.userType = this._configuration["AppSettings_Dev:PMIDC:PMIDC_Credentials:userType"];


            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.pmidc_authAPIkey = this._configuration["AppSettings_Stag:PMIDC:PMIDC_API_Token_Key"];
                this.pmidc_authAPIUrl = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Auth_API_Url"];
                this.pmidc_createAPIUrl = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Create_API_Url"];
                this.pmidc_serachAPIUrl = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Search_API_Url"];
                pmidcLoginModel.grant_type = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Credentials:grant_type"];
                pmidcLoginModel.username = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Credentials:username"];
                pmidcLoginModel.password = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Credentials:password"];
                pmidcLoginModel.tenantId = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Credentials:tenantId"];
                pmidcLoginModel.scope = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Credentials:scope"];
                pmidcLoginModel.userType = this._configuration["AppSettings_Stag:PMIDC:PMIDC_Credentials:userType"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.pmidc_authAPIkey = this._configuration["AppSettings_Pro:PMIDC:PMIDC_API_Token_Key"];
                this.pmidc_authAPIUrl = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Auth_API_Url"];
                this.pmidc_createAPIUrl = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Create_API_Url"];
                this.pmidc_serachAPIUrl = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Search_API_Url"];
                pmidcLoginModel.grant_type = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Credentials:grant_type"];
                pmidcLoginModel.username = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Credentials:username"];
                pmidcLoginModel.password = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Credentials:password"];
                pmidcLoginModel.tenantId = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Credentials:tenantId"];
                pmidcLoginModel.scope = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Credentials:scope"];
                pmidcLoginModel.userType = this._configuration["AppSettings_Pro:PMIDC:PMIDC_Credentials:userType"];
            }
        }




        #region Call PMIDC
        public async Task<ServiceResponseModel> CallPMIDC(PMIDCService pMIDCService, long Grievance_id)
        {
            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("username", pmidcLoginModel.username));
            keyValues.Add(new KeyValuePair<string, string>("password", pmidcLoginModel.password));
            keyValues.Add(new KeyValuePair<string, string>("grant_type", pmidcLoginModel.grant_type));
            keyValues.Add(new KeyValuePair<string, string>("tenantId", pmidcLoginModel.tenantId));
            keyValues.Add(new KeyValuePair<string, string>("userType", pmidcLoginModel.userType));
            keyValues.Add(new KeyValuePair<string, string>("scope", pmidcLoginModel.scope));

         

            FormUrlEncodedContent data = new FormUrlEncodedContent(keyValues);
            var url = this.pmidc_authAPIUrl;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", this.pmidc_authAPIkey);
                var response = await httpClient.PostAsync(url, data);
                string result = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var res = (JObject)JsonConvert.DeserializeObject(result);
                    string access_token = res["access_token"].Value<string>();
                    Dictionary<string, string> intMap = new Dictionary<string, string>();
                    intMap.Add("Grievance_Id", Grievance_id.ToString());
                    intMap.Add("Created_On", DateTime.Now.ToString());

                    PmidcModel pmidcModel = new PmidcModel()
                    {
                        RequestInfo = new RequestInfo
                        {
                            authToken = access_token,
                            action = "_create",
                            apiId = "Rainmaker",
                            did = "1",
                            key = "",
                            msgId = "20170310130900|en_IN",
                            ts = "",
                            ver = ".01"
                        },
                        services = new List<PMIDCService>
                        {
                          new PMIDCService
                          {
                              citizen = pMIDCService.citizen,
                              serviceCode=pMIDCService.serviceCode,
                              addressDetail= pMIDCService.addressDetail,
                              description = pMIDCService.description,
                              phone = pMIDCService.phone,
                              tenantId = pMIDCService.tenantId,
                              source = pMIDCService.source,
                              attributes = intMap

                          }
                        }
                    };
                    string jsonString = JsonConvert.SerializeObject(pmidcModel);
                    HttpResponseMessage _ResponseMessage = new HttpResponseMessage();
                    var keyValuesPair = new List<KeyValuePair<string, string>>();
                    keyValuesPair.Add(new KeyValuePair<string, string>("tenantId", "pb.punjab"));
                    _ResponseMessage = await _objHelperLoc.ExecuteExternalAPI(this.pmidc_createAPIUrl, HttpMethod.Post, pmidcModel, keyValuesPair);
                    var responseResult = await _ResponseMessage.Content.ReadAsStringAsync();
                    if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        var jString = await _ResponseMessage.Content.ReadAsStringAsync();
                        PMIDCResponse pMIDCResponse = JsonConvert.DeserializeObject<PMIDCResponse>(jString);
                        List<SqlParameter> Parameters = new List<SqlParameter>();
                        Parameters.Add(new SqlParameter("Grievance_Id", Grievance_id));
                        Parameters.Add(new SqlParameter("Response_String", jString));
                        Parameters.Add(new SqlParameter("PMIDC_Complaint_Id", pMIDCResponse.services[0].serviceRequestId));

                        #region DB CALL & BUSINESS LOGIC
                        DataTable dtresponse = _MSSQLGateway.ExecuteProcedure("PMIDC_Response_Save", Parameters);
                        if (_objHelper.checkDBResponse(dtresponse))
                        {
                            if (Convert.ToInt64(dtresponse.Rows[0]["response"]) == 1)
                            {
                                _objResponse.response = Convert.ToInt32(Convert.ToString(dtresponse.Rows[0]["response"]));
                                _objResponse.data = _objHelper.ConvertTableToDictionary(dtresponse);
                                _objResponse.sys_message = Convert.ToString(dtresponse.Rows[0]["message"].ToString());
                            }
                            else
                            {
                                _objResponse.response = Convert.ToInt32(Convert.ToString(dtresponse.Rows[0]["response"]));
                                _objResponse.data = _objHelper.ConvertTableToDictionary(dtresponse);
                                _objResponse.sys_message = Convert.ToString(dtresponse.Rows[0]["message"].ToString());
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.response_code = "400";
                    }
                }

                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _objResponse.data = null;
                    _objResponse.response = 0;
                    _objResponse.response_code = "401";
                    _objResponse.sys_message = "Unauthorized";
                }
                else
                {
                    _objResponse.data = null;
                    _objResponse.response = 2;
                    _objResponse.sys_message = "Please check your credential and try again !";
                }

            }
            return _objResponse;
        }
        #endregion

        #region GetPMIDCDistrictsAll
        [HttpGet("GetPMIDCDistrictsAll")]
        public ServiceResponseModel GetPMIDCDistrictsAll()
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("getPMIDCDistrict", Parameters);
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
            }
            #endregion
            return _objResponse;
        }
        #endregion


        #region GetComplaintStatus_PMIDC
        [HttpPost("GetComplaintStatus_PMIDC")]
        [AllowAnonymous]
        public ServiceResponseModel GetComplaintStatus_PMIDC([FromBody] PMIDCStatus model)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Complaint_Id", model.Complaint_Id));
            Parameters.Add(new SqlParameter("Status", model.Status));
            Parameters.Add(new SqlParameter("Remarks", model.Remarks));

            #region DB CALL & BUSINESS LOGIC
            DataTable dtresponse = _MSSQLGateway.ExecuteProcedure("PMIDC_Status_Update", Parameters);
            if (_objHelper.checkDBResponse(dtresponse))
            {
                if (Convert.ToInt64(dtresponse.Rows[0]["response"]) == 1)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresponse.Rows[0]["response"]));
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresponse);
                    _objResponse.sys_message = Convert.ToString(dtresponse.Rows[0]["message"].ToString());
                }
                else
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresponse.Rows[0]["response"]));
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresponse);
                    _objResponse.sys_message = Convert.ToString(dtresponse.Rows[0]["message"].ToString());
                }
            }
            return _objResponse;
        }
        #endregion
        #endregion



        #region GetTownByDist
        [HttpGet("GetTownByDist/{districtId}")]
        [AllowAnonymous]
        public ServiceResponseModel GetTownByDist(long districtId)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("districtid", districtId));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("get_town_by_districtId", Parameters);
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
            }
            #endregion
            return _objResponse;
        }
        #endregion

        #region GetLocalityByTown
        [HttpGet("GetLocalityByTown/{townId}")]
        [AllowAnonymous]
        public ServiceResponseModel GetLocalityByTown(long townId)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("townId", townId));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("get_locality_by_town", Parameters);
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
            }
            #endregion
            return _objResponse;
        }
        #endregion

    }
}
