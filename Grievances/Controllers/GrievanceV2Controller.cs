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
    [Route("api/grievance/v2")]
    [Authorize]
    public class GrievanceV2Controller : Controller
    {
        #region Controller Properties
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private CommonFunctions _objHelperLoc;
        private MSSQLGateway _MSSQLGateway;
        private MSSQLGateway _MSSQLGateway_CP;
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
        string SMSOTPTemplateId = string.Empty;
        string SMSOTPAFTR = string.Empty;
        string SMSOTPAFTRTemplateId = string.Empty;
        string SMSTOACTOR = string.Empty;
        string SMSTOACTORTemplateId = string.Empty;
        string SMSTOACTORFWD = string.Empty;
        string SMSTOACTORFWDTemplateId = string.Empty;
        string SMSOTPAFTRRESOLVED = string.Empty;
        string SMSOTPAFTRRESOLVEDTemplateId = string.Empty;
        string base64String = string.Empty;
        string RBQ = string.Empty;
        ConnectionFactory _factory;
        string Smsurl = string.Empty;
        string SmsServerKey = string.Empty;
        #endregion

        FetchNodalOfficerAPIModel APIModel = new FetchNodalOfficerAPIModel();
        CommonFunctions APICall;
        emailReportModelResponse _emailobj = new emailReportModelResponse();
        Emails _emailobjList = new Emails();
        EmailController sendEmail;

        public GrievanceV2Controller(IConfiguration configuration, IHostingEnvironment env)
        {

            // rbqobj = new RabbitMQCont(env.ToString());
            _factory = new ConnectionFactory() { HostName = "13.71.125.187", Port = 30183, Password = "eaadmin", UserName = "guest" };
            sendEmail = new EmailController(configuration, env);
            _objHelperLoc = new CommonFunctions(configuration, env);
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP"));
                this.GetNodelOfficer = this._configuration["AppSettings_Dev:GetNodelOfficer"];
                this.emailReport = this._configuration["AppSettings_Dev:emailReport"];
                this.SendEmailEndpoint = this._configuration["AppSettings_Dev:SendEmailEndpoint"];
                this.GetNodelOfficerState = this._configuration["AppSettings_Dev:GetNodelOfficerState"];
                this.GetACG = this._configuration["AppSettings_Dev:GetACG"];
                this.GetADCGRID = this._configuration["AppSettings_Dev:GetADCGR"];
                this.GetActor = this._configuration["AppSettings_Dev:GetActor"];
                this.SMSOTP = this._configuration["AppSettings_Dev:SMSOTP"];
                this.SMSOTPTemplateId = this._configuration["AppSettings_Dev:SMSOTP_TemplateId"];
                this.SMSOTPAFTR = this._configuration["AppSettings_Dev:SMSOTPAFTER"];
                this.SMSOTPAFTRTemplateId = this._configuration["AppSettings_Dev:SMSOTPAFTER_TemplateId"];
                this.SMSTOACTOR = this._configuration["AppSettings_Dev:SMSTOACTOR"];
                this.SMSTOACTORTemplateId = this._configuration["AppSettings_Dev:SMSTOACTOR_TemplateId"];
                this.SMSTOACTORFWD = this._configuration["AppSettings_Dev:SMSTOACTORFWD"];
                this.SMSTOACTORFWDTemplateId = this._configuration["AppSettings_Dev:SMSTOACTORFWD_TemplateId"];
                this.SMSOTPAFTRRESOLVED = this._configuration["AppSettings_Dev:SMSOTPAFTRRESOLVED"];
                this.SMSOTPAFTRRESOLVEDTemplateId = this._configuration["AppSettings_Dev:SMSOTPAFTRRESOLVED_TemplateId"];
                this.AbsolutePortalURL = this._configuration["AppSettings_Dev:AbsolutePortalURL"];
                this.RBQ = this._configuration["AppSettings_Dev:RBQ"];
                this.Smsurl = this._configuration["Smsurl"];
                this.SmsServerKey = this._configuration["SmsServerKey"];
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
                this.GetACG = this._configuration["AppSettings_Stag:GetACG"];
                this.AbsolutePortalURL = this._configuration["AppSettings_Stag:AbsolutePortalURL"];
                this.RBQ = this._configuration["AppSettings_Stag:RBQ"];
                this.SMSOTP = this._configuration["AppSettings_Stag:SMSOTP"];
                this.SMSOTPTemplateId = this._configuration["AppSettings_Stag:SMSOTP_TemplateId"];
                this.SMSOTPAFTR = this._configuration["AppSettings_Stag:SMSOTPAFTER"];
                this.SMSOTPAFTRTemplateId = this._configuration["AppSettings_Stag:SMSOTPAFTER_TemplateId"];
                this.SMSTOACTOR = this._configuration["AppSettings_Stag:SMSTOACTOR"];
                this.SMSTOACTORTemplateId = this._configuration["AppSettings_Stag:SMSTOACTOR_TemplateId"];
                this.SMSTOACTORFWD = this._configuration["AppSettings_Stag:SMSTOACTORFWD"];
                this.SMSTOACTORFWDTemplateId = this._configuration["AppSettings_Stag:SMSTOACTORFWD_TemplateId"];
                this.SMSOTPAFTRRESOLVED = this._configuration["AppSettings_Stag:SMSOTPAFTRRESOLVED"];
                this.SMSOTPAFTRRESOLVEDTemplateId = this._configuration["AppSettings_Stag:SMSOTPAFTRRESOLVED_TemplateId"];
                this.Smsurl = this._configuration["Smsurl"];
                this.SmsServerKey = this._configuration["SmsServerKey"];
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP_stag"));

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
                this.AbsolutePortalURL = this._configuration["AppSettings_Pro:AbsolutePortalURL"];
                this.GetACG = this._configuration["AppSettings_Pro:GetACG"];
                this.RBQ = this._configuration["AppSettings_Pro:RBQ"];

                this.SMSOTP = this._configuration["AppSettings_Pro:SMSOTP"];
                this.SMSOTPTemplateId = this._configuration["AppSettings_Pro:SMSOTP_TemplateId"];
                this.SMSOTPAFTR = this._configuration["AppSettings_Pro:SMSOTPAFTER"];
                this.SMSOTPAFTRTemplateId = this._configuration["AppSettings_Pro:SMSOTPAFTER_TemplateId"];
                this.SMSTOACTOR = this._configuration["AppSettings_Pro:SMSTOACTOR"];
                this.SMSTOACTORTemplateId = this._configuration["AppSettings_Pro:SMSTOACTOR_TemplateId"];
                this.SMSTOACTORFWD = this._configuration["AppSettings_Pro:SMSTOACTORFWD"];
                this.SMSTOACTORFWDTemplateId = this._configuration["AppSettings_Pro:SMSTOACTORFWD_TemplateId"];
                this.SMSOTPAFTRRESOLVED = this._configuration["AppSettings_Pro:SMSOTPAFTRRESOLVED"];
                this.SMSOTPAFTRRESOLVEDTemplateId = this._configuration["AppSettings_Pro:SMSOTPAFTRRESOLVED_TemplateId"];

                this.Smsurl = this._configuration["AppSettings_Pro:Smsurl"];
                this.SmsServerKey = this._configuration["SmsServerKey"];
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP_pro"));
            }
        }

        #region SaveGrievance
        [Authorize]
        [HttpPost("CreateGrievance")]
        public async Task<ServiceResponseModel> CreateGrievanceAsync([FromBody] GrievanceModel InputModel)
        {
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            FetchNodalOfficerNameAPIModel ActorName = new FetchNodalOfficerNameAPIModel();

            List<SqlParameter> Parameters = new List<SqlParameter>();

            #endregion

            #region DB PARAMETERS
            Parameters.Add(new SqlParameter("Application_Title", InputModel.Application_Title));
            Parameters.Add(new SqlParameter("Application_Description", InputModel.Application_Description));
            Parameters.Add(new SqlParameter("Application_Department", InputModel.Application_Department));
            Parameters.Add(new SqlParameter("Application_Department_Name", InputModel.Application_Department_Name));
            Parameters.Add(new SqlParameter("Application_District", Convert.ToInt64(InputModel.Application_District)));
            Parameters.Add(new SqlParameter("Application_District_Name", InputModel.Application_District_Name));
            Parameters.Add(new SqlParameter("Citizen_EA_User_ID", InputModel.Citizen_EA_User_ID));
            Parameters.Add(new SqlParameter("Citizen_Name", InputModel.Citizen_Name));
            Parameters.Add(new SqlParameter("Citizen_Email", InputModel.Citizen_Email));
            Parameters.Add(new SqlParameter("Citizen_Mobile_No", InputModel.Citizen_Mobile_No));
            Parameters.Add(new SqlParameter("Citizen_Address", InputModel.Citizen_Address));
            Parameters.Add(new SqlParameter("Citizen_District", InputModel.Citizen_District));
            Parameters.Add(new SqlParameter("Citizen_District_ID", InputModel.Citizen_District_ID));
            Parameters.Add(new SqlParameter("Citizen_Tehsil", InputModel.Citizen_Tehsil));
            Parameters.Add(new SqlParameter("Citizen_Tehsil_ID", InputModel.Citizen_Tehsil_ID));
            Parameters.Add(new SqlParameter("Citizen_Village", InputModel.Citizen_Village));
            Parameters.Add(new SqlParameter("Citizen_Village_ID", InputModel.Citizen_Village_ID));
            Parameters.Add(new SqlParameter("Citizen_Municipality", InputModel.Citizen_Municipality));
            Parameters.Add(new SqlParameter("Citizen_Municipality_ID", InputModel.Citizen_Municipality_ID));
            Parameters.Add(new SqlParameter("Citizen_State", InputModel.Citizen_State));
            Parameters.Add(new SqlParameter("Citizen_State_ID", InputModel.Citizen_State_ID));
            Parameters.Add(new SqlParameter("Citizen_Pincode", InputModel.Citizen_Pincode));
            Parameters.Add(new SqlParameter("Citizen_Type", InputModel.Citizen_Type));
            Parameters.Add(new SqlParameter("Previous_Grievance", Convert.ToInt64(InputModel.Previous_Grievance)));
            Parameters.Add(new SqlParameter("submittedby", InputModel.submittedby));
            Parameters.Add(new SqlParameter("CPGrievance", InputModel.CPGrievance));
            //  Parameters.Add(new SqlParameter("NFSAGrievanceId", InputModel.NFSAGrievanceId));
            if (string.IsNullOrEmpty(Convert.ToString(InputModel.Citizen_EA_User_ID)) || InputModel.Citizen_EA_User_ID == 0)
            {
                InputModel.Otp = Convert.ToString(_objHelper.generateOTP(6));
                // InputModel.Otp = "898989";
                string message = SMSOTP.Replace("[OTP]", Convert.ToString(InputModel.Otp));
                SendSMS(InputModel.Citizen_Mobile_No, message, SMSOTPTemplateId);
                Parameters.Add(new SqlParameter("Otp", InputModel.Otp));
            }
            else
            {
                Parameters.Add(new SqlParameter("Is_Otp_Verified", 1));
            }

            Parameters.Add(new SqlParameter("Sub_Category_ID", InputModel.Sub_Category_ID));
            Parameters.Add(new SqlParameter("Category_ID", InputModel.Category_ID));
            Parameters.Add(new SqlParameter("Flow_Type", InputModel.Flow_Type));
            //Parameters.Add(new SqlParameter("Nodel_Officer_ID", Convert.ToInt64(NextActorID)));
            Parameters.Add(new SqlParameter("Origin_Officer_ID", ""));
            Parameters.Add(new SqlParameter("Request_type", "PC"));
            Parameters.Add(new SqlParameter("System_type", InputModel.System_type));

            Parameters.Add(new SqlParameter("Location_Type", InputModel.LocationType));

            Parameters.Add(new SqlParameter("Process_ID", ComputeProcessId(InputModel.LocationType, InputModel.Previous_Grievance)));


            #endregion
            #region DB CALL & BUSINESS LOGIC
            //_objResponse = GrievanceRequest("APP_GRIE--VANCE_CREATE", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("APP_GRIEVANCE_CREATE", Parameters);

            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {

                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
                else
                {
                    //--------Adding to  RabitMQ                     

                    using (var connection = _factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: this.RBQ,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);


                        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dtresp.Rows[0]["Grievance_id"]));
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        channel.BasicPublish(exchange: "",
                                                routingKey: this.RBQ,
                                                basicProperties: properties,
                                                body: body);
                        //System.Console.WriteLine(" [x] Sent {0}", JsonConvert.SerializeObject(dtresp.Rows[0]["Grievance_id"]));

                    }
                    //------ end RabbitMQ

                    if (!string.IsNullOrEmpty(InputModel.CPGrievance))
                    {
                        List<SqlParameter> Parameterscp = new List<SqlParameter>();

                        #endregion

                        #region DB PARAMETERS
                        Parameterscp.Add(new SqlParameter("pgrsGrievance_ID", dtresp.Rows[0]["Grievance_id"]));
                        Parameterscp.Add(new SqlParameter("cpGrievance_ID", InputModel.CPGrievance));

                        DataTable dtrespcp = _MSSQLGateway_CP.ExecuteProcedure("updatepgrsgrievanceid", Parameterscp);

                    }

                    if (InputModel.doc != null && InputModel.doc.Count > 0)
                    {
                        _objResponse = UploadDocuments(InputModel.doc.ToList(), Convert.ToInt64(dtresp.Rows[0]["Grievance_id"]));
                        if (Convert.ToInt64(dtresp.Rows[0]["response"]) == 1)
                        {
                            _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                            _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                            _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                            if (!string.IsNullOrEmpty(Convert.ToString(InputModel.Citizen_EA_User_ID)) && InputModel.Citizen_EA_User_ID != 0)
                            {
                                SendSMS(Convert.ToString(InputModel.Citizen_Mobile_No), SMSOTPAFTR.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_id"])), SMSOTPAFTRTemplateId);

                            }
                        }
                        else
                        {
                            _objResponse.response = 1;
                            _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                            _objResponse.sys_message = "Document not saved!";
                        }
                    }
                    else
                    {
                        if (Convert.ToInt64(dtresp.Rows[0]["response"]) == 1)
                        {
                            _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                            _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                            _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                            if (!string.IsNullOrEmpty(Convert.ToString(InputModel.Citizen_EA_User_ID)) && InputModel.Citizen_EA_User_ID != 0)
                            {
                                SendSMS(Convert.ToString(InputModel.Citizen_Mobile_No), SMSOTPAFTR.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_id"])), SMSOTPAFTRTemplateId);

                            }

                            //-- send message to Actor 
                            HttpResponseMessage _ResponseMessage = await APICall.PostExternalAPI(GetActor + dtresp.Rows[0]["ActorID"].ToString());
                            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);

                            if (model != null && model.data != null)
                            {
                                // Declare JArray for getting data from response model
                                JArray jArray = JArray.FromObject(model.data);
                                if (this._objHelper.checkJArray(jArray))
                                {
                                    string Actor_email = Convert.ToString(jArray[0]["Email"]);
                                    string Actor_mobile = Convert.ToString(jArray[0]["Mobile"]);

                                    if (Actor_mobile != "")
                                    {
                                        //   SendSMS(Convert.ToString(Actor_mobile), SMSTOACTOR.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_id"])), SMSTOACTORTemplateId);
                                    }
                                    if (Actor_email != "")
                                    {
                                        List<EmailData> _EmailData = new List<EmailData>();

                                        EmailData _ed = new EmailData();
                                        _ed.emailId = Actor_email;
                                        _ed.subject = "New Grievance with Grievance ID: " + Convert.ToString(dtresp.Rows[0]["Grievance_id"]);
                                        _EmailData.Add(_ed);



                                        // _emailobjList.body = SMSTOACTOR.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_id"]));
                                        // _emailobjList.emails = _EmailData;

                                        //      ServiceResponseModel _ServiceResponseModel = await sendEmail.SendMultipleEmail(_emailobjList);
                                    }
                                }
                                //var ActorID = model.data;
                            }

                            //--------------

                        }
                    }

                }
            }

            #endregion

            return _objResponse;
        }
        #endregion

        #region COMPUTE PROCESS ID
        protected string ComputeProcessId(string _locationType, string prieviousGrievanceId)
        {
            string processId = "";
            if (_locationType == "1")
            {
                processId = "1";
            }
            else if (_locationType == "2")
            {
                processId = "2";
            }
            else
            {
                processId = "";
            }


            return processId;
        }
        #endregion
        public string SendSMS(string mobilenumber, string msg, string templateId)
        {
            ServiceResponseModel accToken = _objHelperLoc.GenerateAccessToken();
            string ret = "";
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.Smsurl);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Server-Key", this.SmsServerKey);
                //httpWebRequest.Headers.Add("Authorization", "Bearer " + accToken.sys_message);
                httpWebRequest.Method = "Post";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //string json = "{ 'mobileno':'" + mobilenumber + "','message':'" + msg + "','Template_Id':'" + templateId + "'}";
                    string json = "{ 'mobile_no':'" + mobilenumber + "','message':'" + msg + "','template_id':'" + templateId + "','is_unicode':'" + false + "'}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    ret = result;
                }
            }
            catch (Exception ex)
            {
                ret = ex.Message.ToString();
            }
            return ret;
        }
        #region uploaddocument
        [HttpPost]
        public ServiceResponseModel UploadDocuments(List<DocumentModel> dm, Int64? GrievanceID)
        {
            if (dm != null && dm.Count > 0)
            {
                foreach (DocumentModel _dm in dm)
                {
                    List<SqlParameter> Parameters = new List<SqlParameter>();
                    Parameters.Add(new SqlParameter("File_ID", _dm.File_ID));
                    Parameters.Add(new SqlParameter("Grievance_ID", GrievanceID));

                    _objResponse = GrievanceRequest("INSERT_DOCUMENTS", Parameters);
                }

            }
            return _objResponse;
        }
        #endregion
        #region Grievance Request and Response
        private ServiceResponseModel GrievanceRequest(string procedureName, List<SqlParameter> sp)
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
                    _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]); ;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
            }
            return _objResponse;
        }

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

        #endregion

        #region Get Department
        [HttpPost("master")]
        public ServiceResponseModel master([FromBody] EAapiModel InputModel)
        {
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("isactive", '1'));
            Parameters.Add(new SqlParameter("SelectBy", InputModel.SelectBy));
            Parameters.Add(new SqlParameter("deptid", InputModel.deptid));
            Parameters.Add(new SqlParameter("subdept", InputModel.subdept));

            #region DB CALL & BUSINESS LOGIC
            DataTable dtresp =  _MSSQLGateway.ExecuteProcedure(Convert.ToString("getstakeholders_v2"), Parameters);
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
            #endregion
            return _objResponse;

        }
        #endregion


        #region Get Department
        [HttpPost("masteropen")]
        [AllowAnonymous]
        public ServiceResponseModel masteropen([FromBody] EAapiModel InputModel)
        {
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("isactive", '1'));
            Parameters.Add(new SqlParameter("SelectBy", InputModel.SelectBy));
            Parameters.Add(new SqlParameter("deptid", InputModel.deptid));
            Parameters.Add(new SqlParameter("subdept", InputModel.subdept));

            #region DB CALL & BUSINESS LOGIC
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("getstakeholders_v2"), Parameters);
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
            #endregion
            return _objResponse;

        }
        #endregion

        #region Get Location Type
        [HttpGet("GetLocationType/{deptId}")]
        public ServiceResponseModel GetLocationType(long deptId)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("deptid", deptId));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("APP_FETCH_LocationType_v2", Parameters);
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
        #region GetDistrictByDepartRole
        [HttpGet("GetDistrictsByDeptRole/{deptId}")]
        public ServiceResponseModel GetDistrictsByDeptRole(long deptId)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("deptid", deptId));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("getdistrictbydeptrole_v2", Parameters);
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

        #region GetDistrict22
        [HttpGet("GetDistrictsAll")]
        public ServiceResponseModel GetDistrictsAll()
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("getDistrictCallCentre", Parameters);
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



        #region GetTownByDeptId
        [HttpPost("GetTownByDeptId")]
        [AllowAnonymous]
        public ServiceResponseModel GetTownByDeptId([FromBody] TownByDeptModel model)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("deptid", model.deptId));
            Parameters.Add(new SqlParameter("districtid", model.districtId));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("get_town_by_deptId", Parameters);
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

        #region GrievanceCounts
        [HttpGet("GetGrievanceCounts")]
        [Authorize]
        public ServiceResponseModel GetGrievanceCounts()
        {
            #region DB CALL & BUSINESS LOGIC
            long Citizen_Id = Convert.ToInt64(HttpContext.User.Claims.First(x => x.Type == "Citizen_Ref_ID").Value);
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Citizen_Id", Citizen_Id));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("GetGrievanceCountReceived", Parameters);
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

        #region GetGrievanceHistory
        [HttpPost("GetGrievanceHistory")]
        public ServiceResponseModel GetGrievanceHistoryCitizen([FromBody]GrievanceHistoryModel model)
        {
            try
            {
                model.Citizen_ID = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Citizen_Ref_ID"));
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("Citizen_ID", model.Citizen_ID));
                Parameters.Add(new SqlParameter("Page_number", model.Page_number));
                Parameters.Add(new SqlParameter("Page_size", model.Page_size));
                Parameters.Add(new SqlParameter("Search_by", model.Search_by));
                Parameters.Add(new SqlParameter("Search_value", model.Search_value));
                Parameters.Add(new SqlParameter("Sort_by", model.Sort_by));
                Parameters.Add(new SqlParameter("Sort_order", model.Sort_order));
                Parameters.Add(new SqlParameter("From_date", model.From_date));
                Parameters.Add(new SqlParameter("To_date", model.To_date));
                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("App_Grievance_History_By_CitizenID", Parameters);
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
                return _objResponse;
            }
            catch(Exception e)
            {
                _objResponse.response = 0;
                _objResponse.data = "unauthorized";
                _objResponse.sys_message = e.ToString();
                _objResponse.response_code = "403";
                return _objResponse;
            }
        }
        #endregion

        #region Get Nodal officer Grievances
        [HttpPost("GetGrievanceNodalOfficer")]
        [AllowAnonymousAttribute]
        public ServiceResponseModel GetGrievanceNodalOfficer([FromBody]GrievanceNodalOffcerModel model)
        {
            try
            {
                //model.Citizen_ID = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Citizen_Ref_ID"));
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("Department_ID", model.Department_ID));
                Parameters.Add(new SqlParameter("Page_number", model.Page_number));
                Parameters.Add(new SqlParameter("Page_size", model.Page_size));             
                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("fetch_nodaloffcer_grievance", Parameters);
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
                return _objResponse;
            }
            catch (Exception e)
            {
                _objResponse.response = 0;
                _objResponse.data = "unauthorized";
                _objResponse.sys_message = e.ToString();
                _objResponse.response_code = "403";
                return _objResponse;
            }
        }
        #endregion

    }
}
