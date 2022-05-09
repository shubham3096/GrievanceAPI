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
using System.Threading.Tasks;

namespace GrievanceService.Controllers
{
    [Route("SewaKendra")]
    public class SewaKendraController : Controller
    {
        ServiceResponseModel _objResponse = new ServiceResponseModel();
        private MSSQLGateway _msSqlGateway;
        IHostingEnvironment _env;
        IConfiguration _configuration;
        CommonFunctions APICall;
        string mdmurl = string.Empty;
        private CommonHelper _objHelper = new CommonHelper();


        public SewaKendraController(IConfiguration configuration, IHostingEnvironment env)
        {
            APICall = new CommonFunctions(configuration, env);
            this._env = env;
            this._configuration = configuration;
            if (env.IsStaging())
            {
                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
            }
            else if (env.IsProduction())
            {
                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
            }
            else
            {
                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
            }
        }



        //[HttpGet("GetSewaKendraDepartments")]
        //[AllowAnonymous]
        //public ServiceResponseModel GetSewaKendraDepartments()
        //{
        //    List<SqlParameter> param = new List<SqlParameter>();
        //    try
        //    {
        //      DataTable dt=  _msSqlGateway.ExecuteProcedure("get_sewa_kendra_deparments", param);
        //        if (dt.Rows.Count > 0)
        //        {
        //            _objResponse.data = dt;
        //              _objResponse.response = 1;
        //              _objResponse.response_code = "200";
        //            _objResponse.sys_message = "Successfull";
        //        }
        //        else
        //        {
        //            _objResponse.data = null;
        //            _objResponse.response = 1;
        //            _objResponse.sys_message = "No data found";
        //            _objResponse.response_code = "200";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _objResponse.data = null;
        //        _objResponse.response = 0;
        //        _objResponse.sys_message = "Failed";
        //        _objResponse.response_code = ex.Message;
        //    }
        //    return _objResponse;
        //}

        //[HttpPost("GetSewaKendraServices")]
        //[AllowAnonymous]
        //public ServiceResponseModel GetSewaKendraServices([FromBody]SewaKendraServiceModel model)
        //{
        //    List<SqlParameter> parameters = new List<SqlParameter>();
        //    try
        //    {
        //        parameters.Add(new SqlParameter("Dept_id", model.dept_id));
        //        parameters.Add(new SqlParameter("Page_number", model.Page_number));
        //        parameters.Add(new SqlParameter("Page_size", model.Page_size));
        //        DataTable dt = _msSqlGateway.ExecuteProcedure("get_sewa_kendra_services_by_dept", parameters);
        //        if (dt.Rows.Count > 0)
        //        {
        //            _objResponse.data = dt;
        //            _objResponse.response = 1;
        //            _objResponse.response_code = "200";
        //            _objResponse.sys_message = "Successfull";
        //        }
        //        else
        //        {
        //            _objResponse.data = null;
        //            _objResponse.response = 1;
        //            _objResponse.sys_message = "No data found";
        //            _objResponse.response_code = "200";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _objResponse.data = null;
        //        _objResponse.response = 0;
        //        _objResponse.sys_message = "Failed";
        //        _objResponse.response_code = ex.Message;
        //    }
        //    return _objResponse;
        //}

        //[HttpGet("GetSewaKendraAddressDetails/{district_id}")]
        //[AllowAnonymous]
        //public ServiceResponseModel GetSewaKendraAddressDetails(long district_id)
        //{
        //    List<SqlParameter> parameters = new List<SqlParameter>();
        //    try
        //    {
        //        parameters.Add(new SqlParameter("district_id", district_id));
        //        DataTable dt = _msSqlGateway.ExecuteProcedure("get_sewa_kendra_address_details", parameters);
        //        if (dt.Rows.Count > 0)
        //        {
        //            _objResponse.data = dt;
        //            _objResponse.response = 1;
        //            _objResponse.response_code = "200";
        //            _objResponse.sys_message = "Successfull";
        //        }
        //        else
        //        {
        //            _objResponse.data = null;
        //            _objResponse.response = 1;
        //            _objResponse.sys_message = "No data found";
        //            _objResponse.response_code = "200";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _objResponse.data = null;
        //        _objResponse.response = 0;
        //        _objResponse.sys_message = "Failed";
        //        _objResponse.response_code = ex.Message;
        //    }
        //    return _objResponse;
        //}


        #region GetSewaKendraDepartments
        [HttpPost("GetSewaKendraDepartments")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetServicesAsync([FromBody] GetSewaKendraDepartmentModel serviceModel)
        {
            ServiceResponseModel accToken = APICall.GenerateAccessToken();
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + accToken.sys_message));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/SewaKendra/GetSewaKendraDepartments", HttpMethod.Post, serviceModel, header);
            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {
                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "failed";
                    _objResponse.response_code = "503";
                    return _objResponse;
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "no data";
                _objResponse.response_code = "200";
                return _objResponse;
            }
            return _objResponse;
        }
        #endregion

        #region GetSewaKendraServices
        [HttpPost("GetSewaKendraServices")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetSewaKendraServices([FromBody] SewaKendraServiceModel serviceModel)
        {
            ServiceResponseModel accToken = APICall.GenerateAccessToken();
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + accToken.sys_message));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/SewaKendra/GetSewaKendraServices", HttpMethod.Post, serviceModel, header);
            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {
                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "failed";
                    _objResponse.response_code = "503";
                    return _objResponse;
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "no data";
                _objResponse.response_code = "200";
                return _objResponse;
            }
            return _objResponse;
        }
        #endregion


        #region GetSewaKendraAddressDetails
        [HttpPost("GetSewaKendraAddressDetails")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetSewaKendraAddressDetails([FromBody] GetSewaKendraModel serviceModel)
        {
            ServiceResponseModel accToken = APICall.GenerateAccessToken();
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + accToken.sys_message));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/SewaKendra/GetSewaKendraAddressDetails", HttpMethod.Post, serviceModel, header);
            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {
                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "successful";
                    _objResponse.response_code = "200";
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "failed";
                    _objResponse.response_code = "503";
                    return _objResponse;
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "no data";
                _objResponse.response_code = "200";
                return _objResponse;
            }
            return _objResponse;
        }
        #endregion
    }
}
