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
using EmailService.Models;
using EmailService.Controllers;
using RabbitMQ.Client;
using RabbitMQservice;
using iTextSharp.text.html.simpleparser;

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("api/grievance")]
    [Authorize]
    public class bulkgrievanceadd : Controller
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
        string SMSTOACTOR = string.Empty;
        string SMSTOACTORFWD = string.Empty;
        string SMSOTPAFTRRESOLVED = string.Empty;
        string base64String = string.Empty;
        string RBQ = string.Empty;
        ConnectionFactory _factory;

        #endregion
        // private static ConnectionFactory _factory;


        //public static ConnectionFactory GetConnection()
        //{
        //    return _factory;
        //}

        FetchNodalOfficerAPIModel APIModel = new FetchNodalOfficerAPIModel();
        CommonFunctions APICall;
        emailReportModelResponse _emailobj = new emailReportModelResponse();
        Emails _emailobjList = new Emails();
        EmailController sendEmail;
        GrievanceController ga;
        string mdmurl;
        // RabbitMQCont rbqobj;
        public bulkgrievanceadd(IConfiguration configuration, IHostingEnvironment env)
        {
            ga = new GrievanceController(configuration, env);
            // rbqobj = new RabbitMQCont(env.ToString());
            _factory = new ConnectionFactory() { HostName = "52.172.41.137", Port = 5672, Password = "eaadmin", UserName = "guest" };
            sendEmail = new EmailController(configuration, env);
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
                this.SMSTOACTOR = this._configuration["AppSettings_Dev:SMSTOACTOR"];
                this.SMSTOACTORFWD = this._configuration["AppSettings_Dev:SMSTOACTORFWD"];
                this.SMSOTPAFTRRESOLVED = this._configuration["AppSettings_Dev:SMSOTPAFTRRESOLVED"];
                this.AbsolutePortalURL = this._configuration["AppSettings_Dev:AbsolutePortalURL"];
                this.RBQ = this._configuration["AppSettings_Dev:RBQ"];
                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
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
                this.AbsolutePortalURL = this._configuration["AppSettings_Stag:AbsolutePortalURL"];
                this.RBQ = this._configuration["AppSettings_Stag:RBQ"];
                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
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
                this.RBQ = this._configuration["AppSettings_Pro:RBQ"];
                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
            }
        }
        #region multidepartment bulk grievances
        [AllowAnonymous]
        [HttpPost("sendbulkgrievncesmulti")]
        public async Task<ServiceResponseModel> sendbulkgrievncesstate([FromBody]GrievanceModel InputModel)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("isactive", '1'));
            JArray dist;
             // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
             DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("getstakeholders"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {

                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {

                    HttpResponseMessage _ResponseMessage;

                    _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/region/v1/getsgddistrictbystateid/7");

                    var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                    ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
                    if (model != null && model.data != null)
                    {
                        // Declare JArray for getting data from response model
                        JArray jArray = JArray.FromObject(model.data);
                        if (this._objHelper.checkJArray(jArray))
                        {
                            
                             dist = jArray;

                            if (InputModel.LocationType == "2")
                            {

                                for (int i = 0; i < dtresp.Rows.Count; i++)
                                {
                                    for (int j = 0; j < dist.Count(); j++)
                                    {
                                        Int32 sid = Convert.ToInt32(dtresp.Rows[i]["Stakeholder_ID"]);
                                        string sname = Convert.ToString(dtresp.Rows[i]["Stakeholder_Name"]);
                                        InputModel.Application_Department = sid;
                                        InputModel.Application_Department_Name = sname;

                                        InputModel.Application_District = Convert.ToInt32(jArray[0]["District_ID"]);
                                        InputModel.Application_District_Name = Convert.ToString(jArray[0]["District_Name"]);
                                        _objResponse = await ga.CreateGrievanceAsync(InputModel);

                                    }
                                }
                            }
                            if (InputModel.LocationType == "1")
                            {

                                for (int i = 0; i < dtresp.Rows.Count; i++)
                                {
                                   
                                        Int32 sid = Convert.ToInt32(dtresp.Rows[i]["Stakeholder_ID"]);
                                        string sname = Convert.ToString(dtresp.Rows[i]["Stakeholder_Name"]);
                                        InputModel.Application_Department = sid;
                                        InputModel.Application_Department_Name = sname;                                      
                                        _objResponse = await ga.CreateGrievanceAsync(InputModel);

                                }
                            }
                        }
                    }
                       
                }
                   

            }              
            
            return _objResponse;
        }
        #endregion

        #region single bulk grievances
        [AllowAnonymous]
        [HttpPost("sendbulkgrievncessingle")]
        public async Task<ServiceResponseModel> sendbulkgrievncessingle([FromBody]GrievanceModel InputModel)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("isactive", '1'));
            JArray dist;
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            
                    HttpResponseMessage _ResponseMessage;

                    _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/region/v1/getsgddistrictbystateid/7");

                    var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                    ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
                    if (model != null && model.data != null)
                    {
                        // Declare JArray for getting data from response model
                        JArray jArray = JArray.FromObject(model.data);
                        if (this._objHelper.checkJArray(jArray))
                        {

                            dist = jArray;

                            if (InputModel.LocationType == "2")
                            {

                               
                                    for (int j = 0; j < dist.Count(); j++)
                                    {                                      

                                        InputModel.Application_District = Convert.ToInt32(jArray[0]["District_ID"]);
                                        InputModel.Application_District_Name = Convert.ToString(jArray[0]["District_Name"]);
                                        _objResponse = await ga.CreateGrievanceAsync(InputModel);

                                    }
                            }
                        }
                            if (InputModel.LocationType == "1")
                            {                            
                                                      
                                    _objResponse = await ga.CreateGrievanceAsync(InputModel);
                                                   
                            }
                    }
       
            return _objResponse;
        }
        #endregion
    }
}