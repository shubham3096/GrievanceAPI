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
using GrievanceService.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.html.simpleparser;


namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    [Authorize]
    public class obdrequestresponse : Controller
    {
        #region Controller Properties
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private CommonFunctions _objHelperLoc;
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        string GetNodelOfficer = string.Empty;
        string emailReport = string.Empty;
        string SendEmailEndpoint = string.Empty;
        string GetNodelOfficerState = string.Empty;
        string GetACG = string.Empty;
        string AbsolutePortalURL = string.Empty;
        string GetADCGRID = string.Empty;
        string GetActor = string.Empty;
        string SMSOTP = string.Empty;
        string SMSOTPAFTR = string.Empty;
        string SMSOTPAFTRRESOLVED = string.Empty;
        string base64String = string.Empty;
        string gid = string.Empty;
        FetchNodalOfficerAPIModel APIModel = new FetchNodalOfficerAPIModel();
        CommonFunctions APICall;
        GrievanceController gricont;
        emailReportModelResponse _emailobj = new emailReportModelResponse();
        #endregion
        public obdrequestresponse(IConfiguration configuration, IHostingEnvironment env)
        {
             gricont = new GrievanceController(configuration, env);
            _objHelperLoc = new CommonFunctions(configuration, env);
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this.GetNodelOfficer = this._configuration["AppSettings_Dev:GetNodelOfficer"];
                this.emailReport = this._configuration["AppSettings_Dev:emailReport"];
                this.SendEmailEndpoint = this._configuration["AppSettings_Dev:SendEmailEndpoint"];
                this.GetNodelOfficerState = this._configuration["AppSettings_Dev:GetNodelOfficerState"];
                this.GetACG = this._configuration["AppSettings_Dev:GetACG"];
                this.GetADCGRID = this._configuration["AppSettings_Dev:GetADCGR"];
                this.GetActor = this._configuration["AppSettings_Dev:GetActor"];
                this.SMSOTP = this._configuration["AppSettings_Dev:SMSOTP"];
                this.SMSOTPAFTR = this._configuration["AppSettings_Dev:SMSOTPAFTER"];
                this.SMSOTPAFTRRESOLVED = this._configuration["AppSettings_Dev:SMSOTPAFTRRESOLVED"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.GetNodelOfficer = this._configuration["AppSettings_Stag:GetNodelOfficer"];
                this.emailReport = this._configuration["AppSettings_Stag:emailReport"];
                this.GetNodelOfficerState = this._configuration["AppSettings_Stag:GetNodelOfficerState"];
                this.GetADCGRID = this._configuration["AppSettings_Stag:GetADCGR"];
                this.SendEmailEndpoint = this._configuration["AppSettings_Stag:SendEmailEndpoint"];
                this.GetActor = this._configuration["AppSettings_Stag:GetActor"];
                this.SMSOTP = this._configuration["AppSettings_Stag:SMSOTP"];
                this.SMSOTPAFTR = this._configuration["AppSettings_Stag:SMSOTPAFTER"];
                this.SMSOTPAFTRRESOLVED = this._configuration["AppSettings_Stag:SMSOTPAFTRRESOLVED"];
                this.GetACG = this._configuration["AppSettings_Stag:GetACG"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.GetNodelOfficer = this._configuration["AppSettings_Pro:GetNodelOfficer"];
                this.emailReport = this._configuration["AppSettings_Pro:emailReport"];
                this.SendEmailEndpoint = this._configuration["AppSettings_Pro:SendEmailEndpoint"];
                this.GetNodelOfficerState = this._configuration["AppSettings_Pro:GetNodelOfficerState"];
                this.GetADCGRID = this._configuration["AppSettings_Pro:GetADCGR"];
                this.GetActor = this._configuration["AppSettings_Pro:GetActor"];
                this.SMSOTP = this._configuration["AppSettings_Pro:SMSOTP"];
                this.SMSOTPAFTR = this._configuration["AppSettings_Pro:SMSOTPAFTER"];
                this.AbsolutePortalURL = this._configuration["AppSettings_Pro:AbsolutePortalURL"];
                this.SMSOTPAFTRRESOLVED = this._configuration["AppSettings_Pro:SMSOTPAFTRRESOLVED"];
                this.GetACG = this._configuration["AppSettings_Pro:GetACG"];
               }

        }

        #region GET GRIEVANCE obd response
        [AllowAnonymous]
        [HttpGet("Getobdresponse")]
        public async Task<ServiceResponseModel> GetobdresponseAsync(Int64? ref_id, Int64?  mobile, string call_status, string duration, string start_time, string end_time, string keypress, string Operator, string circle)
        {

            if (keypress.Trim() == "2:1")
            {
                List<DocumentModel> docinput = new List<DocumentModel>();
                List<SqlParameter> Parametersget = new List<SqlParameter>();
                Parametersget.Add(new SqlParameter("Access_Flag", "P"));
                Parametersget.Add(new SqlParameter("Grievance_ID", ref_id));


                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("GRIEVANCE_SELECT_BY_ID", Parametersget);
                if (_objHelper.checkDBResponse(dtresp))
                {
                    if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                    {
                        _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                        _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                    }
                    else
                    {
                        GrievanceModel grivmodle = new GrievanceModel();
                        var GrievanceTitle = Convert.ToString(dtresp.Rows[0]["Application_Title"]);
                        if (GrievanceTitle.Contains("1stAppeal:"))
                        {

                            GrievanceTitle = "2ndAppeal:" + GrievanceTitle;
                        }
                        else
                        {
                            GrievanceTitle = "1stAppeal:" + GrievanceTitle;

                        }


                        grivmodle.Application_Title = Convert.ToString(GrievanceTitle);
                        grivmodle.Application_Description = Convert.ToString(dtresp.Rows[0]["Application_Description"]);
                        grivmodle.Application_District = Convert.ToInt32(dtresp.Rows[0]["Application_District"]);
                        grivmodle.Application_District_Name = Convert.ToString(dtresp.Rows[0]["Application_District_Name"]);
                        grivmodle.Application_Department = Convert.ToInt32(dtresp.Rows[0]["Application_Department"]);
                        grivmodle.Application_Department_Name = Convert.ToString(dtresp.Rows[0]["Application_Department_Name"]);
                        grivmodle.Citizen_Name = Convert.ToString(dtresp.Rows[0]["Citizen_Name"]);
                        grivmodle.Citizen_Address = Convert.ToString(dtresp.Rows[0]["Citizen_Address"]);
                        grivmodle.Citizen_Village = Convert.ToString(dtresp.Rows[0]["Citizen_Village"]);
                        grivmodle.Citizen_State = Convert.ToString(dtresp.Rows[0]["Citizen_State"]);
                        grivmodle.Citizen_Pincode = Convert.ToString(dtresp.Rows[0]["Citizen_Pincode"]);
                        grivmodle.Citizen_Email = Convert.ToString(dtresp.Rows[0]["Citizen_Email"]);
                        grivmodle.Category_ID = Convert.ToInt32(dtresp.Rows[0]["Category_ID"]);
                        grivmodle.Sub_Category_ID = Convert.ToInt32(dtresp.Rows[0]["Sub_Category_ID"]);
                        grivmodle.Citizen_EA_User_ID = Convert.ToInt32(dtresp.Rows[0]["Citizen_EA_User_ID"]);
                        grivmodle.Citizen_Municipality_ID = Convert.ToString(dtresp.Rows[0]["Citizen_Municipality_ID"]);
                        grivmodle.Citizen_Municipality = Convert.ToString(dtresp.Rows[0]["Citizen_Municipality"]);
                        grivmodle.Citizen_State = Convert.ToString(dtresp.Rows[0]["Citizen_State"]);
                        grivmodle.Citizen_District_ID = Convert.ToInt32(dtresp.Rows[0]["Citizen_District_ID"]);
                        grivmodle.Citizen_District = Convert.ToString(dtresp.Rows[0]["Citizen_District"]);

                        grivmodle.Citizen_State_ID = Convert.ToInt32(dtresp.Rows[0]["Citizen_State_ID"]);
                        grivmodle.System_type = Convert.ToString("EA");
                        grivmodle.Flow_Type = Convert.ToString("I");
                        grivmodle.Citizen_Mobile_No = Convert.ToString(dtresp.Rows[0]["Citizen_Mobile_No"]);
                        grivmodle.Citizen_Type = Convert.ToString(dtresp.Rows[0]["Citizen_Type"]);

                        grivmodle.doc = docinput;
                        grivmodle.Previous_Grievance = Convert.ToString(dtresp.Rows[0]["Grievance_ID"]);



                        _objResponse = await gricont.ReappealGrievanceAsync(grivmodle);
                        JArray jArray = JArray.FromObject(_objResponse.data);
                        if (this._objHelper.checkJArray(jArray))
                        {
                            gid = Convert.ToString(jArray[0]["Grievance_id"]);

                        }
                        // _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    }
                }
            }
            
            List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("ref_id", ref_id));
                Parameters.Add(new SqlParameter("mobile", mobile));
                Parameters.Add(new SqlParameter("call_status", call_status));
                Parameters.Add(new SqlParameter("duration", duration));
                Parameters.Add(new SqlParameter("start_time", start_time));
                Parameters.Add(new SqlParameter("end_time", end_time));
                Parameters.Add(new SqlParameter("keypress", keypress));
                Parameters.Add(new SqlParameter("Operator", Operator));
                Parameters.Add(new SqlParameter("circle", circle));
            Parameters.Add(new SqlParameter("Reapeal_grievanceid", gid));
            Parameters.Add(new SqlParameter("update", "res"));

                #region DB CALL & BUSINESS LOGIC
                _objResponse = GrievanceResponse("obdresponse", Parameters);
                #endregion
                return _objResponse;
        }
        #endregion


        #region GET GRIEVANCE obd Request
        [AllowAnonymous]
        [HttpGet("Getobdrequest")]
        public async Task<ServiceResponseModel> GetobdrequestAsync()
        {
            List<SqlParameter> Parametersget = new List<SqlParameter>();
            Parametersget.Add(new SqlParameter("update", "getgriev"));

          
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("obdresponse", Parametersget);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {
                   _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
                else
                {
                    foreach (DataRow data in dtresp.Rows)
                    {
                      

                        var mobile =  data["Citizen_Mobileno"].ToString();
                        var ref_id = data["Grievance_ID"].ToString();


                        String obdurlrequest = "http://164.100.14.7/pb/voiceapi/api_obd_resolution.php?campaign=162098&msisdn=" + mobile + "&tts=" + ref_id;

                        HttpResponseMessage _ResponseMessage = await APICall.GetExternalAPI(obdurlrequest);
                        if (_ResponseMessage.StatusCode == HttpStatusCode.OK)
                        {

                            List<SqlParameter> Parametersupdate = new List<SqlParameter>();
                            Parametersupdate.Add(new SqlParameter("ref_id", ref_id));
                            Parametersupdate.Add(new SqlParameter("update", "initcallupdate"));
                            #region DB CALL & BUSINESS LOGIC
                                     GrievanceResponse("obdresponse", Parametersupdate);

                            #endregion
                        }
                        List<SqlParameter> Parameters = new List<SqlParameter>();
                        Parameters.Add(new SqlParameter("ref_id", ref_id));
                        Parameters.Add(new SqlParameter("mobile", mobile));
                        Parameters.Add(new SqlParameter("OBDApistatus", _ResponseMessage.StatusCode.ToString()));
                        Parameters.Add(new SqlParameter("update", "req"));

                        #region DB CALL & BUSINESS LOGIC
                        _objResponse =  GrievanceResponse("obdresponse", Parameters);
                        #endregion
                    }

                }
            }
                return _objResponse;
            }
            #endregion


            private ServiceResponseModel GrievanceResponse(string procedureName, List<SqlParameter> sp)
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
        }
    }
