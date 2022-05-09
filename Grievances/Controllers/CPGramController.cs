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

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("cpgram")]
    [Authorize]
    public class CPGramController : Controller
    {


        #region Controller Properties
        private ServiceResponseModelRows _objResponseRows = new ServiceResponseModelRows();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private MSSQLGateway _MSSQLGateway_CP;
        private IHostingEnvironment _env;
        CommonFunctions APICall;
        string Getreminders = string.Empty;
        string sendresponse = string.Empty;
        string returnresponse = string.Empty;
        string GetGrievances = string.Empty;
        string mdmurl = string.Empty;
        #endregion


        public CPGramController(IConfiguration configuration, IHostingEnvironment env)
        {
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP"));
                this.Getreminders = this._configuration["AppSettings_Dev:Getreminders"];
                this.sendresponse = this._configuration["AppSettings_Dev:sendresponse"];
                this.returnresponse = this._configuration["AppSettings_Dev:returnresponse"];
                this.GetGrievances = this._configuration["AppSettings_Dev:GetGrievances"];
                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP_stag"));
                this.Getreminders = this._configuration["AppSettings_Stag:Getreminders"];
                this.sendresponse = this._configuration["AppSettings_Dev:sendresponse"];
                this.returnresponse = this._configuration["AppSettings_Dev:returnresponse"];
                this.GetGrievances = this._configuration["AppSettings_Stag:GetGrievances"];
                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP_pro"));
                this.Getreminders = this._configuration["AppSettings_Pro:Getreminders"];
                this.sendresponse = this._configuration["AppSettings_Pro:sendresponse"];
                this.returnresponse = this._configuration["AppSettings_Pro:returnresponse"];
                this.GetGrievances = this._configuration["AppSettings_Pro:GetGrievances"];
                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
            }


        }


        #region getcpreminders
        [HttpGet("getcpreminders")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> getcpreminders()
        {
            HttpResponseMessage _ResponseMessage;


            _ResponseMessage = await APICall.GetExternalAPICP(Getreminders);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            object model = JsonConvert.DeserializeObject<object>(jsonString);
            if (model != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model);
                if (this._objHelper.checkJArray(jArray))
                {

                    var dataTable = new DataTable();

                    dataTable.Columns.Add("registration_no", typeof(String));
                    dataTable.Columns.Add("srno", typeof(String));
                    dataTable.Columns.Add("date_of_action", typeof(String));
                    dataTable.Columns.Add("type", typeof(String));
                    dataTable.Columns.Add("remarks", typeof(String));
                    dataTable.Columns.Add("FromOrgCode", typeof(String));
                    dataTable.Columns.Add("FromOrgName", typeof(String));

                    foreach (var content in jArray)
                    {
                        var row = dataTable.NewRow();

                        row["registration_no"] = content.SelectToken("registration_no").ToString();
                        row["srno"] = content.SelectToken("srno").ToString();
                        row["date_of_action"] = (string)content.SelectToken("date_of_action").ToString();
                        row["type"] = content.SelectToken("type").ToString();
                        row["remarks"] = content.SelectToken("remarks").ToString();
                        row["FromOrgCode"] = content.SelectToken("FromOrgCode").ToString();
                        row["FromOrgName"] = content.SelectToken("FromOrgName").ToString();
                        dataTable.Rows.Add(row);

                    }
                    if (dataTable.Rows.Count > 0)
                    {

                        List<SqlParameter> Parameters = new List<SqlParameter>();
                        // Parameters.Add(new SqlParameter("registration_no", registration_no));

                        Parameters.Add(new SqlParameter("tblCustomers", (dataTable)));
                        Parameters.Add(new SqlParameter("Action_type", "I"));


                        DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("StoreReminder"), Parameters);
                        if (_objHelper.checkDBResponse(dtresp))
                        {
                            if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                            {

                                _objResponse.response = 1;
                                _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                            }
                            else
                            {
                                _objResponse.response = 0;
                                _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                            }
                        }
                    }


                }

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


        #region sendcpremindersres
        [HttpGet("sendcpremindersres")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> sendcpremindersres()
        {
            HttpResponseMessage _ResponseMessage;
            List<SqlParameter> GetParameters = new List<SqlParameter>();

            GetParameters.Add(new SqlParameter("Action_type", "G"));


            DataTable Getdtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("StoreReminderGetGrievances"), GetParameters);
            if (_objHelper.checkDBResponse(Getdtresp))
            {
                if (Convert.ToString(Getdtresp.Rows[0]["response"]) == "1")
                {

                    Getdtresp.Columns.Remove("response");

                    DataColumn flagNew = new DataColumn("flag", typeof(string));
                    DataColumn officername = new DataColumn("officername", typeof(string));
                    DataColumn officerdesignation = new DataColumn("officerdesignation", typeof(string));

                    flagNew.DefaultValue = "R";
                    Getdtresp.Columns.Add(flagNew);

                    officername.DefaultValue = "PGRS";
                    Getdtresp.Columns.Add(officername);

                    officerdesignation.DefaultValue = "Operator";
                    Getdtresp.Columns.Add(officerdesignation);




                    _ResponseMessage = await APICall.PostExternalAPICP(sendresponse, _objHelper.ConvertTableToDictionary(Getdtresp));

                    var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                    object model = JsonConvert.DeserializeObject<object>(jsonString);
                    if (model != null)
                    {
                        // Declare JArray for getting data from response model
                        JArray jArray = JArray.FromObject(model);
                        if (this._objHelper.checkJArray(jArray))
                        {

                            var dataTable = new DataTable();

                            dataTable.Columns.Add("registration_no", typeof(String));
                            dataTable.Columns.Add("srno", typeof(String));
                            dataTable.Columns.Add("date_of_action", typeof(String));
                            dataTable.Columns.Add("type", typeof(String));
                            dataTable.Columns.Add("remarks", typeof(String));
                            dataTable.Columns.Add("FromOrgCode", typeof(String));
                            dataTable.Columns.Add("FromOrgName", typeof(String));

                            foreach (var content in jArray)
                            {
                                var row = dataTable.NewRow();

                                row["registration_no"] = content.SelectToken("registration_no").ToString();
                                row["srno"] = null;
                                row["date_of_action"] = null;
                                row["type"] = null;
                                row["remarks"] = null;
                                row["FromOrgCode"] = null;
                                row["FromOrgName"] = null;
                                dataTable.Rows.Add(row);

                            }
                            if (dataTable.Rows.Count > 0)
                            {

                                List<SqlParameter> Parameters = new List<SqlParameter>();
                                // Parameters.Add(new SqlParameter("registration_no", registration_no));

                                Parameters.Add(new SqlParameter("tblCustomers", (dataTable)));
                                Parameters.Add(new SqlParameter("Action_type", "U"));


                                DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("StoreReminder"), Parameters);
                                if (_objHelper.checkDBResponse(dtresp))
                                {
                                    if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                                    {

                                        _objResponse.response = 1;
                                        _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                                    }
                                    else
                                    {
                                        _objResponse.response = 0;
                                        _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                                    }
                                }
                            }


                        }

                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.sys_message = "failed";
                        return _objResponse;
                    }
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "NO Grievance to update";
                return _objResponse;
            }


            return _objResponse;
        }
        #endregion



        #region getcpGrievances
        [HttpGet("getcpgrievances")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> getcpgrievances()
        {
            HttpResponseMessage _ResponseMessage;


            _ResponseMessage = await APICall.GetExternalAPICP(GetGrievances);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            object model = JsonConvert.DeserializeObject<object>(jsonString);
            if (model != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model);
                if (this._objHelper.checkJArray(jArray))
                {

                    var dataTable = new DataTable();

                    dataTable.Columns.Add("registration_no", typeof(String));
                    dataTable.Columns.Add("name", typeof(String));
                    dataTable.Columns.Add("gender", typeof(String));
                    dataTable.Columns.Add("address1", typeof(String));
                    dataTable.Columns.Add("address2", typeof(String));
                    dataTable.Columns.Add("address3", typeof(String));
                    dataTable.Columns.Add("pincode", typeof(String));
                    dataTable.Columns.Add("District", typeof(String));
                    dataTable.Columns.Add("State", typeof(String));
                    dataTable.Columns.Add("Country", typeof(String));
                    dataTable.Columns.Add("email_address", typeof(String));
                    dataTable.Columns.Add("phone_no", typeof(String));
                    dataTable.Columns.Add("mobile_no", typeof(String));
                    dataTable.Columns.Add("aadhaar", typeof(String));
                    dataTable.Columns.Add("letter_no", typeof(String));
                    dataTable.Columns.Add("letter_date", typeof(String));
                    dataTable.Columns.Add("language", typeof(String));
                    dataTable.Columns.Add("subject_content", typeof(String));
                    dataTable.Columns.Add("Attachdoc", typeof(String));
                    dataTable.Columns.Add("DateOfReceipt", typeof(String));
                    dataTable.Columns.Add("TRTU", typeof(String));
                    dataTable.Columns.Add("FromOrgCode", typeof(String));
                    dataTable.Columns.Add("FromOrgName", typeof(String));
                    dataTable.Columns.Add("AdditionalInfo1", typeof(String));
                    dataTable.Columns.Add("AdditionalInfo2", typeof(String));
                    dataTable.Columns.Add("AdditionalInfo3", typeof(String));
                    dataTable.Columns.Add("Category", typeof(String));


                    foreach (var content in jArray)
                    {
                        var row = dataTable.NewRow();

                        row["registration_no"] = content.SelectToken("registration_no").ToString();
                        row["name"] = content.SelectToken("name").ToString();
                        row["gender"] = (string)content.SelectToken("gender").ToString();
                        row["address1"] = content.SelectToken("address1").ToString();
                        row["address2"] = content.SelectToken("address2").ToString();
                        row["address3"] = content.SelectToken("address3").ToString();
                        row["pincode"] = content.SelectToken("pincode").ToString();
                        row["District"] = content.SelectToken("District").ToString();
                        row["State"] = content.SelectToken("State").ToString();
                        row["Country"] = content.SelectToken("Country").ToString();
                        row["email_address"] = content.SelectToken("email_address").ToString();
                        row["phone_no"] = content.SelectToken("phone_no").ToString();
                        row["mobile_no"] = content.SelectToken("mobile_no").ToString();
                        row["aadhaar"] = content.SelectToken("aadhaar").ToString();
                        row["letter_no"] = content.SelectToken("letter_no").ToString();
                        row["letter_no"] = content.SelectToken("letter_no").ToString();
                        row["language"] = content.SelectToken("language").ToString();
                        row["subject_content"] = content.SelectToken("subject_content").ToString();
                        row["Attachdoc"] = content.SelectToken("Attachdoc").ToString();
                        row["DateOfReceipt"] = content.SelectToken("DateOfReceipt").ToString();
                        row["TRTU"] = content.SelectToken("TRTU").ToString();
                        row["FromOrgCode"] = content.SelectToken("FromOrgCode").ToString();
                        row["FromOrgName"] = content.SelectToken("FromOrgName").ToString();
                        row["AdditionalInfo1"] = content.SelectToken("AdditionalInfo1").ToString();
                        row["AdditionalInfo2"] = content.SelectToken("AdditionalInfo2").ToString();
                        row["AdditionalInfo3"] = content.SelectToken("AdditionalInfo3").ToString();
                        row["Category"] = content.SelectToken("Category").ToString();
                        dataTable.Rows.Add(row);

                    }
                    if (dataTable.Rows.Count > 0)
                    {

                        List<SqlParameter> Parameters = new List<SqlParameter>();
                        // Parameters.Add(new SqlParameter("registration_no", registration_no));

                        Parameters.Add(new SqlParameter("tblCustomers", (dataTable)));
                        Parameters.Add(new SqlParameter("Action_type", "I"));


                        DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("StoreGrievances"), Parameters);
                        if (_objHelper.checkDBResponse(dtresp))
                        {
                            if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                            {

                                _objResponse.response = 1;
                                _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                            }
                            else
                            {
                                _objResponse.response = 0;
                                _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                            }
                        }
                    }


                }

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

        #region sendcpgrievancesres
        [HttpGet("sendcpgrievancesres")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> sendcpgrievancesres()
        {
            HttpResponseMessage _ResponseMessage;
            List<SqlParameter> GetParameters = new List<SqlParameter>();

            GetParameters.Add(new SqlParameter("Action_type", "GR"));


            DataTable Getdtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("StoreReminderGetGrievances"), GetParameters);
            if (_objHelper.checkDBResponse(Getdtresp))
            {
                if (Convert.ToString(Getdtresp.Rows[0]["response"]) == "1")
                {

                    Getdtresp.Columns.Remove("response");

                    DataColumn flagNew = new DataColumn("flag", typeof(string));
                    DataColumn officername = new DataColumn("officername", typeof(string));
                    DataColumn officerdesignation = new DataColumn("officerdesignation", typeof(string));

                    flagNew.DefaultValue = "G";
                    Getdtresp.Columns.Add(flagNew);

                    officername.DefaultValue = "PGRS";
                    Getdtresp.Columns.Add(officername);

                    officerdesignation.DefaultValue = "Operator";
                    Getdtresp.Columns.Add(officerdesignation);

                    var arr = _objHelper.ConvertTableToDictionary(Getdtresp);


                    _ResponseMessage = await APICall.PostExternalAPICP(sendresponse, arr);


                    var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                    object model = JsonConvert.DeserializeObject<object>(jsonString);
                    if (model != null)
                    {
                        // Declare JArray for getting data from response model
                        JArray jArray = JArray.FromObject(model);
                        if (this._objHelper.checkJArray(jArray))
                        {

                            var dataTable = new DataTable();

                            dataTable.Columns.Add("registration_no", typeof(String));
                            dataTable.Columns.Add("name", typeof(String));
                            dataTable.Columns.Add("gender", typeof(String));
                            dataTable.Columns.Add("address1", typeof(String));
                            dataTable.Columns.Add("address2", typeof(String));
                            dataTable.Columns.Add("address3", typeof(String));
                            dataTable.Columns.Add("pincode", typeof(String));
                            dataTable.Columns.Add("District", typeof(String));
                            dataTable.Columns.Add("State", typeof(String));
                            dataTable.Columns.Add("Country", typeof(String));
                            dataTable.Columns.Add("email_address", typeof(String));
                            dataTable.Columns.Add("phone_no", typeof(String));
                            dataTable.Columns.Add("mobile_no", typeof(String));
                            dataTable.Columns.Add("aadhaar", typeof(String));
                            dataTable.Columns.Add("letter_no", typeof(String));
                            dataTable.Columns.Add("letter_date", typeof(String));
                            dataTable.Columns.Add("language", typeof(String));
                            dataTable.Columns.Add("subject_content", typeof(String));
                            dataTable.Columns.Add("Attachdoc", typeof(String));
                            dataTable.Columns.Add("DateOfReceipt", typeof(String));
                            dataTable.Columns.Add("TRTU", typeof(String));
                            dataTable.Columns.Add("FromOrgCode", typeof(String));
                            dataTable.Columns.Add("FromOrgName", typeof(String));
                            dataTable.Columns.Add("AdditionalInfo1", typeof(String));
                            dataTable.Columns.Add("AdditionalInfo2", typeof(String));
                            dataTable.Columns.Add("AdditionalInfo3", typeof(String));
                            dataTable.Columns.Add("Category", typeof(String));

                            foreach (var content in jArray)
                            {
                                var row = dataTable.NewRow();

                                row["registration_no"] = content.SelectToken("registration_no").ToString();
                                row["name"] = null;
                                row["gender"] = null;
                                row["address1"] = null;
                                row["address2"] = null;
                                row["address3"] = null;
                                row["pincode"] = null;
                                row["District"] = null;
                                row["State"] = null;
                                row["Country"] = null;
                                row["email_address"] = null;
                                row["phone_no"] = null;
                                row["mobile_no"] = null;
                                row["aadhaar"] = null;
                                row["letter_no"] = null;
                                row["letter_no"] = null;
                                row["language"] = null;
                                row["subject_content"] = null;
                                row["Attachdoc"] = null;
                                row["DateOfReceipt"] = null;
                                row["TRTU"] = null;
                                row["FromOrgCode"] = null;
                                row["FromOrgName"] = null;
                                row["AdditionalInfo1"] = null;
                                row["AdditionalInfo2"] = null;
                                row["AdditionalInfo3"] = null;
                                row["Category"] = null;
                                dataTable.Rows.Add(row);

                            }
                            if (dataTable.Rows.Count > 0)
                            {

                                List<SqlParameter> Parameters = new List<SqlParameter>();
                                // Parameters.Add(new SqlParameter("registration_no", registration_no));

                                Parameters.Add(new SqlParameter("tblCustomers", (dataTable)));
                                Parameters.Add(new SqlParameter("Action_type", "U"));


                                DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("StoreGrievances"), Parameters);
                                if (_objHelper.checkDBResponse(dtresp))
                                {
                                    if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                                    {

                                        _objResponse.response = 1;
                                        _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                                    }
                                    else
                                    {
                                        _objResponse.response = 0;
                                        _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                                    }
                                }
                            }


                        }

                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.sys_message = "failed";
                        return _objResponse;
                    }
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "NO Grievance to update";
                return _objResponse;
            }


            return _objResponse;
        }
        #endregion


        #region sendcpgrievancesresovedres
        [HttpGet("sendcpgrievancesresovedres")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> sendcpgrievancesresovedres()
        {
            HttpResponseMessage _ResponseMessage;
            List<SqlParameter> GetParameters = new List<SqlParameter>();

            GetParameters.Add(new SqlParameter("Action_type", "GRR"));


            DataTable Getdtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("StoreReminderGetGrievances"), GetParameters);
            if (_objHelper.checkDBResponse(Getdtresp))
            {
                if (Convert.ToString(Getdtresp.Rows[0]["response"]) == "1")
                {

                    Getdtresp.Columns.Remove("response");

                    DataColumn flagNew = new DataColumn("flag", typeof(string));
                    DataColumn officername = new DataColumn("officername", typeof(string));
                    DataColumn officerdesignation = new DataColumn("officerdesignation", typeof(string));

                    flagNew.DefaultValue = "C";
                    Getdtresp.Columns.Add(flagNew);

                    officername.DefaultValue = "PGRS(Punjab)";
                    Getdtresp.Columns.Add(officername);

                    officerdesignation.DefaultValue = "Concerned Officer";
                    Getdtresp.Columns.Add(officerdesignation);

                    var arr = _objHelper.ConvertTableToDictionary(Getdtresp);


                    _ResponseMessage = await APICall.PostExternalAPICP(sendresponse, arr);

                    var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                    object model = JsonConvert.DeserializeObject<object>(jsonString);
                    if (model != null)
                    {
                        // Declare JArray for getting data from response model
                        JArray jArray = JArray.FromObject(model);
                        if (this._objHelper.checkJArray(jArray))
                        {

                            var dataTable = new DataTable();

                            dataTable.Columns.Add("registration_no", typeof(String));
                            dataTable.Columns.Add("name", typeof(String));
                            dataTable.Columns.Add("gender", typeof(String));
                            dataTable.Columns.Add("address1", typeof(String));
                            dataTable.Columns.Add("address2", typeof(String));
                            dataTable.Columns.Add("address3", typeof(String));
                            dataTable.Columns.Add("pincode", typeof(String));
                            dataTable.Columns.Add("District", typeof(String));
                            dataTable.Columns.Add("State", typeof(String));
                            dataTable.Columns.Add("Country", typeof(String));
                            dataTable.Columns.Add("email_address", typeof(String));
                            dataTable.Columns.Add("phone_no", typeof(String));
                            dataTable.Columns.Add("mobile_no", typeof(String));
                            dataTable.Columns.Add("aadhaar", typeof(String));
                            dataTable.Columns.Add("letter_no", typeof(String));
                            dataTable.Columns.Add("letter_date", typeof(String));
                            dataTable.Columns.Add("language", typeof(String));
                            dataTable.Columns.Add("subject_content", typeof(String));
                            dataTable.Columns.Add("Attachdoc", typeof(String));
                            dataTable.Columns.Add("DateOfReceipt", typeof(String));
                            dataTable.Columns.Add("TRTU", typeof(String));
                            dataTable.Columns.Add("FromOrgCode", typeof(String));
                            dataTable.Columns.Add("FromOrgName", typeof(String));
                            dataTable.Columns.Add("AdditionalInfo1", typeof(String));
                            dataTable.Columns.Add("AdditionalInfo2", typeof(String));
                            dataTable.Columns.Add("AdditionalInfo3", typeof(String));
                            dataTable.Columns.Add("Category", typeof(String));

                            foreach (var content in jArray)
                            {
                                var row = dataTable.NewRow();

                                row["registration_no"] = content.SelectToken("registration_no").ToString();
                                row["name"] = null;
                                row["gender"] = null;
                                row["address1"] = null;
                                row["address2"] = null;
                                row["address3"] = null;
                                row["pincode"] = null;
                                row["District"] = null;
                                row["State"] = null;
                                row["Country"] = null;
                                row["email_address"] = null;
                                row["phone_no"] = null;
                                row["mobile_no"] = null;
                                row["aadhaar"] = null;
                                row["letter_no"] = null;
                                row["letter_no"] = null;
                                row["language"] = null;
                                row["subject_content"] = null;
                                row["Attachdoc"] = null;
                                row["DateOfReceipt"] = null;
                                row["TRTU"] = null;
                                row["FromOrgCode"] = null;
                                row["FromOrgName"] = null;
                                row["AdditionalInfo1"] = null;
                                row["AdditionalInfo2"] = null;
                                row["AdditionalInfo3"] = null;
                                row["Category"] = null;
                                dataTable.Rows.Add(row);

                            }
                            if (dataTable.Rows.Count > 0)
                            {

                                List<SqlParameter> Parameters = new List<SqlParameter>();
                                // Parameters.Add(new SqlParameter("registration_no", registration_no));

                                Parameters.Add(new SqlParameter("tblCustomers", (dataTable)));
                                Parameters.Add(new SqlParameter("Action_type", "UR"));


                                DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("StoreGrievances"), Parameters);
                                if (_objHelper.checkDBResponse(dtresp))
                                {
                                    if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                                    {

                                        _objResponse.response = 1;
                                        _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                                    }
                                    else
                                    {
                                        _objResponse.response = 0;
                                        _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                                    }
                                }
                            }


                        }

                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.sys_message = "failed";
                        return _objResponse;
                    }
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "NO Grievance to update";
                return _objResponse;
            }


            return _objResponse;
        }
        #endregion



        [HttpPost("sendcpreturn")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> Sendcpreturn([FromBody] CPGramReturnModel InputModel)
        {


            HttpResponseMessage _ResponseMessage;
            List<SqlParameter> GetParameters = new List<SqlParameter>();


            _ResponseMessage = await APICall.PostExternalAPICP(returnresponse, InputModel);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            cpsendbackresponseModel _newresp = new cpsendbackresponseModel();
            _newresp = JsonConvert.DeserializeObject<cpsendbackresponseModel>(jsonString);
            //jsonString = jsonString.Replace("{", "").Replace("}", "");
            //object model = JsonConvert.DeserializeObject<object>(jsonString);
            if (_newresp.result == " - Successful.")
            {
                List<SqlParameter> Parameterrb = new List<SqlParameter>();
                Parameterrb.Add(new SqlParameter("pgrsGrievance_ID", 1));
                Parameterrb.Add(new SqlParameter("cpGrievance_ID", InputModel.registration_no));
                Parameterrb.Add(new SqlParameter("upflag", "sb"));

                DataTable dtrespcp = _MSSQLGateway_CP.ExecuteProcedure("updatepgrsgrievanceid", Parameterrb);

                _objResponse.response = 1;
                _objResponse.sys_message = "success";
                _objResponse.data = _newresp;
                return _objResponse;


            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _newresp.result;
                return _objResponse;
            }
            return _objResponse;
        }



        #region Get CP Pendency List
        [HttpGet("GetGrievancePendency/{page_number?}/{state?}")]
        // [AllowAnonymous]
        public ServiceResponseModel GetGrievancePendency(Int64? page_number, Int64? state)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("registration_no", registration_no));

            Parameters.Add(new SqlParameter("Action_type", "0"));
            Parameters.Add(new SqlParameter("PageNumber", page_number));
            Parameters.Add(new SqlParameter("State", state));

            DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("getcpgrievancelist"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {

                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                }
            }
            return _objResponse;
        }
        #endregion




        #region Get CP History List
        [HttpGet("GetGrievanceHistory/{page_number?}")]
        // [AllowAnonymous]
        public ServiceResponseModel GetGrievanceHistory(Int64? page_number)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("registration_no", registration_no));
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            Parameters.Add(new SqlParameter("Actor_ID", actor_id));
            Parameters.Add(new SqlParameter("PageNumber", page_number));

            DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("getcpgrievancehistory"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {

                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                }
            }
            return _objResponse;
        }
        #endregion


        #region Get Grievance by ID
        [HttpPost("GetGrievanceByID")]
        //  [AllowAnonymous]
        public ServiceResponseModel GetGrievanceByID([FromBody] CPGramSearchModel InputModel)
        {


            List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("registration_no", registration_no));

            Parameters.Add(new SqlParameter("Grievance_ID", InputModel.Grievance_ID));
            Parameters.Add(new SqlParameter("Access_Flag", InputModel.Access_Flag));

            DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("getcpgrievancelistdetail"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {
                    //var binstr = Convert.ToString(dtresp.Rows[0]["Attachdoc"]);

                    //dtresp.Rows[0]["Attachdoc"] = Convert.ToBase64String(binstr);

                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                }
            }
            return _objResponse;
        }

        #endregion




        #region Get Grievance Reminders
        [HttpPost("GetGrievanceReminder")]
        //  [AllowAnonymous]
        public ServiceResponseModel GetGrievanceReminder([FromBody] CPGramModel InputModel)
        {


            List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("registration_no", registration_no));

            Parameters.Add(new SqlParameter("Grievance_ID", InputModel.Grievance_ID));

            DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("getcpgrievancereminders"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {
                    //var binstr = Convert.ToString(dtresp.Rows[0]["Attachdoc"]);

                    //dtresp.Rows[0]["Attachdoc"] = Convert.ToBase64String(binstr);

                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);


                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                }
            }
            return _objResponse;
        }
        #endregion


        #region Feedback
        [HttpPost]
        [Route("add-reminder")]
        [Authorize]
        public ServiceResponseModel SaveCPGramReminder([FromBody] ReminderFeedbackModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "Data Not Sufficient";

                    return _objResponse;
                }
                List<SqlParameter> param1 = new List<SqlParameter>();

                param1.Add(new SqlParameter("Grievance_ID", Convert.ToInt64(model.Grievance_ID)));
                param1.Add(new SqlParameter("Action_Taken_By", Convert.ToInt64(model.Action_Taken_By)));
                param1.Add(new SqlParameter("Remarks", model.Remarks));
                param1.Add(new SqlParameter("Action_Type", model.Action_Type));

                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("CPGramReminder", param1);
                _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                if (_objResponse.response == 1)
                {

                    _objResponse.sys_message = "Successfully Inserted";
                }
                else
                {
                    _objResponse.sys_message = "Grievance Already Resolved";
                }
            }

            catch (Exception exc)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = exc.Message;
            }

            return _objResponse;
        }
        #endregion


        #region Dispose
        [HttpPost]
        [Route("dispose-cpgram")]
        [Authorize]
        public ServiceResponseModel DisposeCPgramGrievance([FromBody] DisposeCPModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "Data Not Sufficient";
                    return _objResponse;
                }
                List<SqlParameter> param1 = new List<SqlParameter>();
                model.Action_Taken_By = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
                param1.Add(new SqlParameter("Registration_no", (model.Registration_no)));
                param1.Add(new SqlParameter("Action_taken_by", Convert.ToInt64(model.Action_Taken_By)));
                param1.Add(new SqlParameter("Remarks", model.Remarks));
                param1.Add(new SqlParameter("Actor_name", model.Actor_name));

                DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure("StoreDisposeGrievance", param1);
                _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                if (_objResponse.response == 1)
                {
                    _objResponse.sys_message = "Successfully inserted";
                }
                else
                {
                    _objResponse.sys_message = "Failed to insert";
                }
            }

            catch (Exception exc)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = exc.Message;
            }

            return _objResponse;
        }
        #endregion

        #region Get Dispose Grievance History List
        [HttpGet("get-dispose-grievance-history/{page_number?}")]
        // [AllowAnonymous]
        public ServiceResponseModel GetDisposeGrievanceHistory(Int64? page_number)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
           // int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            Parameters.Add(new SqlParameter("Page_number", page_number));

            DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("GetDisposeGrievances"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {
                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "Something went wrong.";
                }
            }
            return _objResponse;
        }
        #endregion


        #region Get Dispose Grievance By Id
        [HttpPost("get-dispose-grievance-by-id")]
        // [AllowAnonymous]
        public ServiceResponseModel GetDisposeGrievanceById([FromBody] GetDisposeRemarksModel model)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            // int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            Parameters.Add(new SqlParameter("Registration_no", model.Registration_no));
            Parameters.Add(new SqlParameter("Dispose_id", Convert.ToInt64(model.Dispose_id)));

            DataTable dtresp = _MSSQLGateway_CP.ExecuteProcedure(Convert.ToString("GetDisposeGrievanceById"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {
                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "Something went wrong.";
                }
            }
            return _objResponse;
        }
        #endregion

    }

}
