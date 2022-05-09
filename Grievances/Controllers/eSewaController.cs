using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Http.Headers;
using System.Text;

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("esewa")]
    [Authorize]
    public class eSewaController : Controller
    {
        #region Controller Properties
        private ServiceResponseModelRows _objResponseRows = new ServiceResponseModelRows();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        CommonFunctions APICall;
        HttpResponseMessage _ResponseMessage;
        string eSewaURL = string.Empty;
        string CreateCitizenProfile = string.Empty;
        string FetchMaritalStatus = string.Empty;
        #endregion
        public eSewaController(IConfiguration configuration, IHostingEnvironment env)
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
            this.CreateCitizenProfile = this._configuration["eSewa:CreateCitizenProfile"];
            this.FetchMaritalStatus = this._configuration["eSewa:FetchMaritalStatus"];
        }

        #region Get Service By Department
        [HttpGet("getServiceByDepartment")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> getServiceByDepartment()
        {
            HttpResponseMessage _ResponseMessage;

            var body = "{flagData: 'All_service',ServiceID: '',DepartmentID: ''}";

            _ResponseMessage = await APICall.PostExternalAPI(CreateCitizenProfile, body);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 0;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


                }
                //var ActorID = model.data;
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "failed";
                return _objResponse;
            }


            return _objResponse;
        }
        #endregion

        #region Fetch Marital Status
        [HttpGet("FetchMaritalStatus")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> fetchMaritalStatus()
        {
            string body = "";

            ServiceResponseModel _apiResponse = await UserWebRequestWithBody(eSewaURL + FetchMaritalStatus, "POST", body, Request.Headers["Authorization"]);
            ServiceResponseModel _apiResponseData = (ServiceResponseModel)_apiResponse.data;
            if (this._objHelper.checkResponseModel(_apiResponseData))
            {
                // Declare JArray for getting data from response model
                JArray _obj = (JArray)_apiResponseData.data;
                if (this._objHelper.checkJArray(_obj))
                {
                    if (Convert.ToString(_obj[0]["response"].ToString()) == "1")
                    {
                        _objResponse.response = 1;
                        _objResponse.data = _obj;
                        _objResponse.sys_message = "succuss";
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.sys_message = "No record found!";
                    }

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "No record found!";
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "Request data failed!";
                return _objResponse;
            }
            return _objResponse;
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
   
        #region Get Services Count 
    [HttpGet("getAllCitizenServiceCounts")]
    [AllowAnonymous]
    public async Task<ServiceResponseModel> GetAllCitizenServiceCounts()
    {
        string body = "";

        ServiceResponseModel _apiResponse = await UserWebRequestWithBody(eSewaURL + "/offlineapi/api/Citizen/ConnectDashboard", "POST", body, Request.Headers["Authorization"]);
        ServiceResponseModel _apiResponseData = (ServiceResponseModel)_apiResponse.data;
        if (this._objHelper.checkResponseModel(_apiResponseData))
        {
            // Declare JArray for getting data from response model
            JArray _obj = (JArray)_apiResponseData.data;
            if (this._objHelper.checkJArray(_obj))
            {
                if (Convert.ToString(_obj[0]["response"].ToString()) == "1")
                {
                    _objResponse.response = 1;
                    _objResponse.data = _obj;
                    _objResponse.sys_message = "succuss";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "No record found!";
                }

            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "No record found!";
            }
        }
        else
        {
            _objResponse.response = 0;
            _objResponse.sys_message = "Request data failed!";
            return _objResponse;
        }
        return _objResponse;
    }
    #endregion
    
        #region Get Services Count By CitizenId
    [HttpGet("getCitizenServiceCountsByCitizenID/{CitizenId}")]
    [Authorize]
    public async Task<ServiceResponseModel> GetCitizenServiceCountsByCitizenID(long CitizenId)
    {
            var body = new 
            {
                CitizenID = CitizenId
            };
            ServiceResponseModel _apiResponse = await UserWebRequestWithBody(eSewaURL + "/offlineapi/api/Citizen/ConnectDashboardByCitizenID", "POST", JsonConvert.SerializeObject(body), Request.Headers["Authorization"]);
        ServiceResponseModel _apiResponseData = (ServiceResponseModel)_apiResponse.data;
        if (this._objHelper.checkResponseModel(_apiResponseData))
        {
            // Declare JArray for getting data from response model
            JArray _obj = (JArray)_apiResponseData.data;
            if (this._objHelper.checkJArray(_obj))
            {
                if (Convert.ToString(_obj[0]["response"].ToString()) == "1")
                {
                    _objResponse.response = 1;
                    _objResponse.data = _obj;
                    _objResponse.sys_message = "succuss";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "No record found!";
                }

            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "No record found!";
            }
        }
        else
        {
            _objResponse.response = 0;
            _objResponse.sys_message = "Request data failed!";
            return _objResponse;
        }
        return _objResponse;
    }
    #endregion
    }


}





