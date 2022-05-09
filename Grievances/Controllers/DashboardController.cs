using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EnterpriseSupportLibrary;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : Controller
    {
        #region Controller Properties
        private ServiceResponseModelRows _objResponseRows = new ServiceResponseModelRows();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        public string username_Notification = string.Empty;
        public string password_Notification = string.Empty;
        public string url_Notification = string.Empty;
        public string survey_API = string.Empty;

        #endregion

        public DashboardController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this.username_Notification = this._configuration["AppSettings_Dev:Latest_Notification:username"];
                this.password_Notification = this._configuration["AppSettings_Dev:Latest_Notification:password"];
                this.url_Notification = this._configuration["AppSettings_Dev:Latest_Notification:url"];
                this.survey_API = this._configuration["AppSettings_Dev:Survey_API"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.username_Notification = this._configuration["AppSettings_Stag:Latest_Notification:username"];
                this.password_Notification = this._configuration["AppSettings_Stag:Latest_Notification:password"];
                this.url_Notification = this._configuration["AppSettings_Stag:Latest_Notification:url"];
                this.survey_API = this._configuration["AppSettings_Stag:Survey_API"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.username_Notification = this._configuration["AppSettings_Pro:Latest_Notification:username"];
                this.password_Notification = this._configuration["AppSettings_Pro:Latest_Notification:password"];
                this.url_Notification = this._configuration["AppSettings_Pro:Latest_Notification:url"];
                this.survey_API = this._configuration["AppSettings_Pro:Survey_API"];
            }
        }

        #region Get filter wise Dashboard
        [HttpPost("Filtergetdashboard")]
        public ServiceResponseModelRows GetFilterwiseDashboard([FromBody] DashBoardModel dashBoardModel)
        {

            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            
            string Roles = Convert.ToString(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Roles"));
            string Geographical_Operational_Level = Convert.ToString(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Geographical_Operational_Level"));
            int? Application_Department = Convert.ToInt32(dashBoardModel.Application_Department);
            int? Application_District = Convert.ToInt32(dashBoardModel.Application_District);
           



            string[] roles = Roles.Split(',').Select(p => p.Trim()).ToArray();


            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Operational_Level", Convert.ToString(Geographical_Operational_Level)));
            Parameters.Add(new SqlParameter("Application_Department", Application_Department));
            Parameters.Add(new SqlParameter("Application_District", Application_District));
            Parameters.Add(new SqlParameter("FromDate", Convert.ToString(dashBoardModel.FromDate)));
            Parameters.Add(new SqlParameter("ToDate", Convert.ToString(dashBoardModel.ToDate)));
           // int index = Array.IndexOf(roles, "6493");

             if (Array.IndexOf(roles, "6490") != -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(6490)));
                
            }
            else if (Array.IndexOf(roles, "6491") != -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(6490)));

            }
            else if (Array.IndexOf(roles, "6494") != -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(6494)));

            }
            else if (Array.IndexOf(roles, "6493") != -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(6493)));
               
            }
            else if (Array.IndexOf(roles, "5") != -1 && Array.IndexOf(roles, "6493") == -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(5)));

            }
            else if (Array.IndexOf(roles, "6") != -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(6)));

            }
            
            else if (Array.IndexOf(roles, "8") != -1 && Array.IndexOf(roles, "6493") == -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(8)));

            }
            else if (Array.IndexOf(roles, "2") != -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(2)));

            }
            else if (Array.IndexOf(roles, "7") != -1)
            {
                Parameters.Add(new SqlParameter("Role", Convert.ToInt32(7)));

            }


            //foreach (var rol in roles)
            //{

            //    if (rol.Trim() == "5" || rol.Trim() != "6493")
            //    {
            //        Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
            //        break;
            //    }
            //    else if (rol.Trim() == "6" || rol.Trim() == "6490" || rol.Trim() == "6491" )
            //    {
            //        Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
            //        break;
            //    }
            //    else if (rol.Trim() == "7" || rol.Trim() == "6494")
            //    {
            //        Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
            //    }
            //    else if (rol.Trim() == "8" || rol.Trim() != "6493")
            //    {
            //        Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
            //        break;
            //    }
            //    else if (rol.Trim() == "6493")
            //    {
            //        Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
            //        break;
            //    }
            //    else if (rol.Trim() == "2")
            //    {
            //        Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
            //        break;
            //    }

            //}



            _objResponseRows = DashboardResponse("APP_FETCH_GET_ADMIN_DASHBOARD_FilterWise", Parameters);
            return _objResponseRows;
        }
        #endregion
        #region Get Dashboard
        [HttpPost("getdashboard")]
        public ServiceResponseModelRows GetDashboard([FromBody] GetDashBoardModel InputModel)
        {

            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));

            //string Geographical_Operational_Level = Convert.ToString(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Geographical_Operational_Level"));
            //int? Application_Department = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Stakeholder_ID"));
            //int? Application_District = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "District_ID"));
            string Roles = Convert.ToString(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Roles"));
            string Geographical_Operational_Level = Convert.ToString(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Geographical_Operational_Level"));
            int? Application_Department = Convert.ToInt32(InputModel.Application_Department);
            int? Application_District = Convert.ToInt32(InputModel.Application_District);
           // string Roles = Convert.ToString(InputModel.Roles);


            string[] roles = Roles.Split(',');


            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Operational_Level", Convert.ToString(Geographical_Operational_Level)));
            Parameters.Add(new SqlParameter("Application_Department", Application_Department));
            Parameters.Add(new SqlParameter("Application_District", Application_District));
            
            foreach (var rol in roles)
            {

                if (rol.Trim() == "5")
                {
                    Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
                    break;
                }
                else if (rol.Trim() == "6" || rol.Trim() == "6490" || rol.Trim() == "6491" )
                {
                    Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
                    break;
                }
                else if (rol.Trim() == "7" || rol.Trim() == "6494")
                {
                    Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
                }
                else if (rol.Trim() == "8")
                {
                    Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
                    break;
                }
                else if (rol.Trim() == "2")
                {
                    Parameters.Add(new SqlParameter("Role", Convert.ToInt32(rol.Trim())));
                    break;
                }

            }



            _objResponseRows = DashboardResponse("APP_FETCH_GET_ADMIN_DASHBOARD", Parameters);
            return _objResponseRows;
        }
        #endregion

        [AllowAnonymous]
        #region Get Dashboard
        [HttpGet("getdashboard_EA/{stakeholderid}")]
        public ServiceResponseModel GetDashboard_EA(Int64? stakeholderid)
        {

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Stakeholder_ID", stakeholderid));
            _objResponse = Response("APP_FETCH_ADMIN_DESHBOARD_EA", Parameters);
            return _objResponse;
        }
        #endregion

        #region Insert Actor Role Mapping
        [HttpPost("insert-actor-role-mapping")]
        public ServiceResponseModel InsertActorUserMappingV1([FromBody] ActorRoleMappingModel armM)
        {
            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                return _objResponse;
            }

            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("Mapping_ID", armM.Mapping_ID));
            param.Add(new SqlParameter("Actor_ID", armM.Actor_Id));
            param.Add(new SqlParameter("Role_ID", armM.Role_ID));
            param.Add(new SqlParameter("Location_Ref_ID", armM.Location_Ref_ID));
            param.Add(new SqlParameter("Is_Active", armM.Is_Active));
            param.Add(new SqlParameter("Geo_Operation_Level", armM.Geo_Operation_Level));            
            param.Add(new SqlParameter("Stakeholder_ID", armM.Stakeholder_ID));
            param.Add(new SqlParameter("District_Name", armM.District_Name));
            param.Add(new SqlParameter("District_ID", armM.District_ID));
            param.Add(new SqlParameter("Created_By", Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "login_id"))));

            _objResponse = DashboardRequest("MAP_INSERT_ACTOR_ROLES", param);
            return _objResponse;
        }
        #endregion

        // Non API Route Methods

        #region Dashboard Request and Response

        private ServiceResponseModel DashboardRequest(string procedureName, List<SqlParameter> sp)
        {
            // Execute procedure with parameters for post data
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString(procedureName), sp);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
                else
                {
                    _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "Record not found!";
            }
            return _objResponse;
        }


        private ServiceResponseModelRows DashboardResponse(string procedureName, List<SqlParameter> sp)
        {
            // Execute procedure with parameters for get data
            DataSet dsresp = _MSSQLGateway.ExecuteProcedureWithDataset(Convert.ToString(procedureName), sp);
            if (_objHelper.checkDBResponse(dsresp))
            {
                if (Convert.ToInt32(Convert.ToString(dsresp.Tables[0].Rows[0]["response"])) <= 0)
                {
                    _objResponseRows.response = Convert.ToInt32(Convert.ToString(dsresp.Tables[0].Rows[0]["response"]));
                    _objResponseRows.sys_message = Convert.ToString(dsresp.Tables[0].Rows[0]["message"].ToString());
                }
                else
                {
                    _objResponseRows.response = Convert.ToInt32(dsresp.Tables[0].Rows[0]["response"]);
                    _objResponseRows.data = _objHelper.ConvertTableToDictionary(dsresp.Tables[0]);
                    _objResponseRows.dataRows = _objHelper.ConvertTableToDictionary(dsresp.Tables[1]);
                }
            }
            return _objResponseRows;
        }

        private ServiceResponseModel Response(string procedureName, List<SqlParameter> sp)
        {
            // Execute procedure with parameters for get data
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString(procedureName), sp);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
                else
                {
                    _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                }
            }
            return _objResponse;
        }

        #endregion


        #region GetLatestNotification
        [HttpGet("GetLatestNotification")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetLatestNotification()
        {
            var url = this.url_Notification;
            using (var httpClient = new HttpClient())
            {
                var username = this.username_Notification;
                var password = this.password_Notification;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8")
                                               .GetBytes(username + ":" + password));
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
                HttpResponseMessage _ResponseMessage = await httpClient.GetAsync(url);
                if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                    var newResult = JsonConvert.DeserializeObject<Dictionary<long, dynamic>>(result).ToArray();
                    List<NotificationResultModel> lstReult = new List<NotificationResultModel>();
                    foreach (var n in newResult)
                    {
                        
                        lstReult.Add(new NotificationResultModel{ 
                        id = n.Key,
                        data= n.Value
                        });
                    }
                    _objResponse.response = 1;
                    _objResponse.data = lstReult;
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



        /// <summary>
        /// GetImportantLinks
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("GetImportantLinks/{type}")]
        [AllowAnonymous]
        public  ServiceResponseModel GetImportantLinksAsync(string type)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("type",type));
            DataTable dtresp =  _MSSQLGateway.ExecuteProcedure("Get_Important_Links", parameters);
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


        #region GetAllSurvey
        [HttpGet("GetAllSurvey")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetAllSurvey()
        {
            var url = this.survey_API + "allsurvey";
            using (var httpClient = new HttpClient())
            {
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


    }
}