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
    public class GrievanceController : Controller
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

       // RabbitMQCont rbqobj;
        public GrievanceController(IConfiguration configuration, IHostingEnvironment env)
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
        public async Task<ServiceResponseModel> CreateGrievanceAsync([FromBody]GrievanceModel InputModel)
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
                    
                    if (Convert.ToInt64(dtresp.Rows[0]["response"]) == 1)
                    {
                            //call PMIDC
                            if (InputModel.Application_Department == 108)
                            {
                                GrievanceController grievanceController = new GrievanceController(_configuration, _env);

                                PMIDCService pMIDCService = new PMIDCService
                                {
                                    citizen = new Citizen
                                    {
                                        mobileNumber = InputModel.Citizen_Mobile_No,
                                        name = InputModel.Citizen_Name,
                                    },
                                    serviceCode = InputModel.Service_Code.Replace(" ", ""),
                                    addressDetail = new AddressDetail
                                    {

                                        city = "pb."+(InputModel.Town_Name.Replace(" ", string.Empty)).ToLower(),
                                        mohalla = InputModel.Locality_Code,
                                        houseNoAndStreetName = "",
                                        landmark = "",
                                    },
                                    description = InputModel.Application_Description,
                                    phone = InputModel.Citizen_Mobile_No,
                                    source = "ivr",
                                    tenantId = "pb."+ (InputModel.Town_Name.Replace(" ", string.Empty)).ToLower()
                                };

                                long Grievance_Id = Convert.ToInt64(dtresp.Rows[0][1]);
                                PMIDCController pmidc = new PMIDCController(_configuration,_env);
                                var pmidcResult = pmidc.CallPMIDC(pMIDCService, Grievance_Id);
                                if (pmidcResult.Result.response == 1)
                                {
                                    _objResponse.response = pmidcResult.Result.response;
                                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                                    _objResponse.sys_message = pmidcResult.Result.sys_message;
                                }

                            }
                            else
                            {

                                _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                                _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                                _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(InputModel.Citizen_EA_User_ID)) && InputModel.Citizen_EA_User_ID != 0)
                            {
                                SendSMS(Convert.ToString(InputModel.Citizen_Mobile_No), SMSOTPAFTR.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_id"])), SMSOTPAFTRTemplateId);

                            }

                            //-- send message to Actor 
                            //HttpResponseMessage _ResponseMessage = await APICall.PostExternalAPI(GetActor + dtresp.Rows[0]["ActorID"].ToString());
                            //var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                            //ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);

                            //if (model != null && model.data != null)
                            //{
                            //    // Declare JArray for getting data from response model
                            //    JArray jArray = JArray.FromObject(model.data);
                            //    if (this._objHelper.checkJArray(jArray))
                            //    {
                            //        string Actor_email = Convert.ToString(jArray[0]["Email"]);
                            //        string Actor_mobile = Convert.ToString(jArray[0]["Mobile"]);
                                    
                            //        if (Actor_mobile != "")
                            //        {
                            //          SendSMS(Convert.ToString(Actor_mobile), SMSTOACTOR.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_id"])), SMSTOACTORTemplateId);
                            //        }
                            //        if (Actor_email != "")
                            //        {
                            //            List<EmailData> _EmailData = new List<EmailData>();
                                    
                            //            EmailData _ed = new EmailData();
                            //            _ed.emailId = Actor_email;
                            //            _ed.subject = "New Grievance with Grievance ID: "+ Convert.ToString(dtresp.Rows[0]["Grievance_id"]);
                            //            _EmailData.Add(_ed);
                                    


                            //       // _emailobjList.body = SMSTOACTOR.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_id"]));
                            //       // _emailobjList.emails = _EmailData;
                                    
                            //      //      ServiceResponseModel _ServiceResponseModel = await sendEmail.SendMultipleEmail(_emailobjList);
                            //        }
                            //    }
                            //    //var ActorID = model.data;
                            //}
                           
                            //--------------
                    }
                }
            }

            #endregion

            return _objResponse;
        }
        #endregion

        #region COMPUTE PROCESS ID
        protected string ComputeProcessId(string _locationType, string prieviousGrievanceId) {
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
      

        #region ReapealGrievance
        [AllowAnonymous]
        [HttpPost("ReappealGrievance")]
        public async Task<ServiceResponseModel> ReappealGrievanceAsync([FromBody]GrievanceModel InputModel)
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
            Parameters.Add(new SqlParameter("Application_Title", InputModel.Application_Title));
            Parameters.Add(new SqlParameter("Application_Description", InputModel.Application_Description));
            Parameters.Add(new SqlParameter("Application_District", Convert.ToInt64(InputModel.Application_District)));
            Parameters.Add(new SqlParameter("Application_Department", Convert.ToInt64(InputModel.Application_Department)));
            Parameters.Add(new SqlParameter("Application_Department_Name", InputModel.Application_Department_Name));
            Parameters.Add(new SqlParameter("Application_District_Name", InputModel.Application_District_Name));
            Parameters.Add(new SqlParameter("Citizen_EA_User_ID", InputModel.Citizen_EA_User_ID));
            Parameters.Add(new SqlParameter("Citizen_Name", InputModel.Citizen_Name));
            Parameters.Add(new SqlParameter("Citizen_Email", InputModel.Citizen_Email));
            Parameters.Add(new SqlParameter("Citizen_Mobile_No", InputModel.Citizen_Mobile_No));
            Parameters.Add(new SqlParameter("Citizen_Address", InputModel.Citizen_Address));
            Parameters.Add(new SqlParameter("Citizen_District", InputModel.Citizen_District));
            Parameters.Add(new SqlParameter("Citizen_District_ID", Convert.ToInt64(InputModel.Citizen_District_ID)));
            Parameters.Add(new SqlParameter("Citizen_Tehsil", InputModel.Citizen_Tehsil));
            Parameters.Add(new SqlParameter("Citizen_Tehsil_ID", InputModel.Citizen_Tehsil_ID));
            Parameters.Add(new SqlParameter("Citizen_Village", InputModel.Citizen_Village));
            Parameters.Add(new SqlParameter("Citizen_Village_ID", InputModel.Citizen_Village_ID));
            Parameters.Add(new SqlParameter("Citizen_Municipality", InputModel.Citizen_Municipality));
            Parameters.Add(new SqlParameter("Citizen_Municipality_ID", InputModel.Citizen_Municipality_ID));
            Parameters.Add(new SqlParameter("Citizen_State", InputModel.Citizen_State));
            Parameters.Add(new SqlParameter("Citizen_State_ID", Convert.ToInt64(InputModel.Citizen_State_ID)));
            Parameters.Add(new SqlParameter("Citizen_Pincode", InputModel.Citizen_Pincode));
            Parameters.Add(new SqlParameter("Citizen_Type", InputModel.Citizen_Type));
            Parameters.Add(new SqlParameter("Previous_Grievance", Convert.ToInt64(InputModel.Previous_Grievance)));
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
            Parameters.Add(new SqlParameter("Assigned_To", Convert.ToInt64(0)));
            Parameters.Add(new SqlParameter("Sub_Category_ID", Convert.ToInt64(InputModel.Sub_Category_ID)));
            Parameters.Add(new SqlParameter("Category_ID", Convert.ToInt64(InputModel.Category_ID)));
            Parameters.Add(new SqlParameter("Flow_Type", InputModel.Flow_Type));
            Parameters.Add(new SqlParameter("Nodel_Officer_ID", Convert.ToInt64(0)));
            Parameters.Add(new SqlParameter("State_Nodal_Officer_ID", Convert.ToInt64(0)));
            Parameters.Add(new SqlParameter("ACG_ID", Convert.ToInt64(0)));
            Parameters.Add(new SqlParameter("Adcgr_ID", Convert.ToInt64(0)));
            //Parameters.Add(new SqlParameter("Origin_Type", ""));
            //Parameters.Add(new SqlParameter("Origin_Officer_ID", ""));

            Parameters.Add(new SqlParameter("Request_type", "PC"));
            Parameters.Add(new SqlParameter("System_type", InputModel.System_type));

            //bool isAuthenticated = User.Identity.IsAuthenticated;
            //if (isAuthenticated)
            //{
            //    string User_type = _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "User_type");

            //    if (User_type == "Actor")
            //    {
            //        int? Application_Department = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Stakeholder_ID"));
            //        String Designation_Name = _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Designation_Name");
            //        String Actor_Ref_ID = _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID");

            //        if (!string.IsNullOrEmpty(Convert.ToString(Application_Department)) && Designation_Name == "ADCGR")
            //        {

            //            Parameters.Add(new SqlParameter("Commission_ID", Actor_Ref_ID));
            //        }
            //        Parameters.Add(new SqlParameter("Is_Otp_Verified", 1));
            //    }

            //}
            #endregion

            #region DB CALL & BUSINESS LOGIC
            //_objResponse = GrievanceRequest("APP_GRIE--VANCE_CREATE", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("APP_GRIEVANCE_CREATE_REAPPEAL", Parameters);
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

                        _objResponse = UploadDocumentsreappeal(InputModel.doc.ToList(), Convert.ToInt64(dtresp.Rows[0]["Grievance_id"]));
                    if (_objResponse.response == 1)
                    {
                        _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                        _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                        _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());

                    }
                    else
                    {
                        _objResponse.response = 1;
                        _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                        _objResponse.sys_message = "Document not saved!";
                    }
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



                              //  _emailobjList.body = SMSTOACTOR.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_id"]));
                              //  _emailobjList.emails = _EmailData;

                              //  ServiceResponseModel _ServiceResponseModel = await sendEmail.SendMultipleEmail(_emailobjList);
                            }
                        }
                        //var ActorID = model.data;
                    }

                    //--------------
                }
            }

            #endregion

            return _objResponse;
        }
        #endregion

        #region uploaddocument
        [HttpPost]
        public ServiceResponseModel UploadDocumentsreappeal(List<DocumentModel> dm, Int64? GrievanceID)
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

        #region GET GRIEVANCE CATEGARIES
        [AllowAnonymous]
        [HttpGet("GetGrievanceCategories")]
        public ServiceResponseModel GetGrievanceCategories(Int64? stakeholderid)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Get_Type", 'S'));
            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("GET_GRIEVANCE_CATEGORY", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region GET GRIEVANCE CATEGARY By Department ID
        [AllowAnonymous]
        [HttpGet("GetGrievanceCategory/{stakeholderid}")]
        public ServiceResponseModel GetGrievanceCategory(Int64? stakeholderid)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Stakeholder_ID", stakeholderid));
            Parameters.Add(new SqlParameter("Get_Type", 'A'));
            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("GET_GRIEVANCE_CATEGORY", Parameters);
            #endregion
            return _objResponse;    
        }
        #endregion

        #region GET GRIEVANCE CATEGARY By Category ID
        [AllowAnonymous]
        [HttpGet("GetGrievanceCategorybyID/{categoryid}")]
        public ServiceResponseModel GetGrievanceCategorybyID(Int64? categoryid)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Stakeholder_ID", categoryid));
            Parameters.Add(new SqlParameter("Get_Type", 'C'));
            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("GET_GRIEVANCE_CATEGORY", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region GET SUB GRIEVANCE CATEGARY BY SUB CATEGORY
        [AllowAnonymous]
        [HttpGet("GetSubGrievanceCategory/{categoryid}")]
        public ServiceResponseModel GetSubGrievanceCategory(Int16? categoryid)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Sub_Category_ID", categoryid));
            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("GET_SUBGRIEVANCE_CATEGORY", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region GET SUB GRIEVANCE CATEGARY BY CATEGORY ID
        [AllowAnonymous]
        [HttpGet("GetSubGrievanceCategoryByCategoryID/{categoryid}")]
        public ServiceResponseModel GetSubGrievanceCategoryByCategoryID(Int16? categoryid)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Category_ID", categoryid));
            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("GET_SUBGRIEVANCE_CATEGORY_BY_Category_ID", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Verify OTP
        [AllowAnonymous]
        [HttpPost("GrievanceOTPCheck")]
        public ServiceResponseModel GrievanceOTPCheck([FromBody]CheckOTP InputMode)
        {

            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Grievance_ID", InputMode.Grievance_ID));
            Parameters.Add(new SqlParameter("OTP", InputMode.OTP));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("VERIFY_OTP", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Resend OTP
        [AllowAnonymous]
        [HttpPost("GrievanceOTPResend")]
        public ServiceResponseModel GrievanceOTPResend([FromBody]ResendOTP InputMode)
        {

            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            var otp = Convert.ToString(_objHelper.generateOTP(6));


            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Grievance_ID", InputMode.Grievance_ID));
            Parameters.Add(new SqlParameter("OTP", otp));

            #region DB CALL & BUSINESS LOGIC
            //  _objResponse = GrievanceResponse("[ResendOTP]", Parameters);

            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("[ResendOTP]"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {
                    string message = SMSOTP.Replace("[OTP]", Convert.ToString(otp));
                    SendSMS(Convert.ToString(dtresp.Rows[0]["Citizen_Mobile_No"]), message, SMSOTPTemplateId);
                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    _objResponse.sys_message = Convert.ToString("OTP Resent");

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

        #region Verify OTP for new grievance
        [AllowAnonymous]
        [HttpPost("GrievanceOTPCheckNewGrievance")]
        public ServiceResponseModel GrievanceOTPCheckNewGrievance([FromBody]CheckOTP InputMode)
        {

            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Grievance_ID", InputMode.Grievance_ID));
            Parameters.Add(new SqlParameter("OTP", InputMode.OTP));

            #region DB CALL & BUSINESS LOGIC
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("VERIFY_OTP"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {
                    SendSMS(Convert.ToString(dtresp.Rows[0]["Citizen_Mobile_No"]), SMSOTPAFTR.Replace("[GriNo]", InputMode.Grievance_ID), SMSOTPAFTRTemplateId);
                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    _objResponse.sys_message = Convert.ToString("Grievance Added");

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

        #region Track Grievance By Citizen
        [AllowAnonymous]
        [HttpGet("TrackGrivance/{Grievance_ID?}/{Mobile?}")]
        public ServiceResponseModel TrackGrivance(Int64? Grievance_ID, Int64? Mobile)
        {
            String Otp = Convert.ToString(_objHelper.generateOTP(6));
            // String Otp = "898989";

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Grievance_ID", Grievance_ID));
            Parameters.Add(new SqlParameter("Mobile", Mobile));
            Parameters.Add(new SqlParameter("OTP", Otp));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("TRAKEGRIEVANCE", Parameters);
            if (_objResponse.response == 1)
            {
                string message = SMSOTP.Replace("[OTP]", Convert.ToString(Otp));
                string smsResponse  = SendSMS(Convert.ToString(Mobile), message, SMSOTPTemplateId);
                _objResponse.sys_message = message + "smsREsponse =>" + smsResponse;
            }
            else
            {
                _objResponse.sys_message = "Mobile No. Or GrievanceID not matched";
            }


            #endregion
            return _objResponse;
        }
        #endregion

        #region Track Grievance By Citizen Mobile        
        [HttpGet("TrackGrivanceMobile/{Grievance_ID?}/{Mobile?}")]
        public ServiceResponseModel TrackGrivanceMobile(Int64? Grievance_ID, Int64? Mobile)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Grievance_ID", Grievance_ID));
            Parameters.Add(new SqlParameter("Mobile", Mobile));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("TRAKEGRIEVANCEMOBILE", Parameters);
            if (_objResponse.response == 1)
            {
                _objResponse.sys_message = "Grievance Matched";
            }
            else
            {
                _objResponse.sys_message = "Mobile No. Or GrievanceID not matched";
            }


            #endregion
            return _objResponse;
        }
        #endregion

        #region Get Actor Pendency List
        [HttpGet("GetGrievancePendency/{page_number?}")]
        public ServiceResponseModel GetGrievancePendency(Int64? page_number)
        {
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Actor_ID", actor_id));
            Parameters.Add(new SqlParameter("PageNumber", page_number));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("ACTOR_PENDENCY", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Get Actor Hisory List
        [HttpGet("GetGrievanceHistory/{page_number?}")]
        public ServiceResponseModel GetGrievanceHistory(Int64? page_number)
        {
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Actor_ID", actor_id));
            Parameters.Add(new SqlParameter("PageNumber", page_number));
            //Parameters.Add(new SqlParameter("PageSize", page_size));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("APP_GET_USER_HISTORY", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Get Actor submitted List
        [HttpGet("GetGrievanceSubmitted/{page_number?}/{page_size?}")]
        // [AllowAnonymous]
        public ServiceResponseModel GetGrievanceSubmitted(Int64? page_number, Int64? page_size)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("registration_no", registration_no));
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            Parameters.Add(new SqlParameter("Actor_ID", actor_id));
            Parameters.Add(new SqlParameter("PageNumber", page_number));
            Parameters.Add(new SqlParameter("PageSize", page_size));

            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("getactorgrievancessubmitted"), Parameters);
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

        #region Get Citizen Hisory List
        [HttpGet("GetGrievanceHistoryCitizen/{Flow_Type}")]
        public ServiceResponseModel GetGrievanceHistoryCitizen(string Flow_Type)
        {
            int? citizen_id;
            try
            {
                citizen_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Citizen_Ref_ID"));
            }
            catch
            {
                citizen_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "uid"));
            }
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Citizen_ID", citizen_id));
            Parameters.Add(new SqlParameter("Flow_Type", Flow_Type));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("APP_GET_CITIZEN_HISTORY", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Get Grievance by ID
        [HttpGet("GetGrievanceByID/{Grievance_ID}/{Access_Flag}")]
        public ServiceResponseModel GetGrievanceByID(string Grievance_ID, string Access_Flag)
        {

            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Actor_ID", actor_id));
            Parameters.Add(new SqlParameter("Grievance_ID", Convert.ToInt64(Grievance_ID)));
            Parameters.Add(new SqlParameter("Access_Flag", Access_Flag));


            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("GRIEVANCE_SELECT_BY_ID", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion


        #region Get Grievance Actions   
        [HttpGet("GetGrievanceActionbystateid/{State_ID}")]
        public ServiceResponseModel GetGrievanceActionbystateid(string State_ID)
        {

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("actionstage", State_ID));


            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("GRIEVANCE_GET_ACTION_STATES", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Get Grievance by ID Citizen
        [HttpGet("GetCitizenGrievanceByID/{Grievance_ID}/{Access_Flag}")]
        public ServiceResponseModel GetCitizenGrievanceByID(string Grievance_ID, string Access_Flag)
        {

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Grievance_ID", Convert.ToInt64(Grievance_ID)));
            Parameters.Add(new SqlParameter("Access_Flag", Access_Flag));



            #region DB CALL & BUSINESS LOGIC
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("GRIEVANCE_SELECT_BY_ID"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {
                   // SendSMS(Convert.ToString(dtresp.Rows[0]["Citizen_Mobile_No"]), SMSOTPAFTR.Replace("[GriNo]", Grievance_ID), SMSOTPAFTRTemplateId);
                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    _objResponse.sys_message = Convert.ToString("Grievance Added");

                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                }
            }

            //_objResponse = GrievanceResponse("GRIEVANCE_SELECT_BY_ID", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        /// <summary>
        /// Bunty Katal
        /// Get Location level 
        /// 27-02-2020
        /// </summary>        
        /// <returns></returns>
        #region Get Location Type
        [AllowAnonymous]
        [HttpGet("GetLocationType")]
        public ServiceResponseModel GetLocationType()
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("APP_FETCH_LocationType", Parameters);
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


        #region Get Grievance by Citizen ID
        [HttpGet("GetGrievanceByCitizenID/{Grievance_ID}/{citizenid}")]
        public ServiceResponseModel GetGrievanceByCitizenID(string Grievance_ID, string citizenid)
        {
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("citizen_ID", citizenid));
            Parameters.Add(new SqlParameter("Grievance_ID", Convert.ToInt64(Grievance_ID)));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("GRIEVANCE_SELECT_BY_ID_CITIZEN", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Add Favrioutes Officer List
        [HttpPost("AddoffcersList")]
        public ServiceResponseModel AddoffcersList([FromBody] AddofficereModel InputModel)
        {
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Actor_List", InputModel.Actor_ID));
            Parameters.Add(new SqlParameter("Parent_Actor_ID", Convert.ToInt64(InputModel.Parent_Actor_ID)));


            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("MD_INSERT_MOST_USED_ACTOR_LIST", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Get Favrioutes Officer List
        [HttpPost("GetoffcersList")]
        public ServiceResponseModel GetoffcersList()
        {
            int? citizen_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Parent_Actor_ID", citizen_id));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("MD_GET_MOST_USED_ACTOR_LIST", Parameters);
            #endregion

            return _objResponse;
        }
        #endregion

        #region Take Action
        [HttpPost("TakeAction")]
        public async Task<ServiceResponseModel> TakeActionAsync([FromBody]TakeActionModel InputModel)
        {

            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }


            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            int? Stakeholder_ID = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Stakeholder_ID"));
            string Actor_name = "";
            string Actor_location = "";
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Actor_ID", Convert.ToInt64(actor_id)));
            Parameters.Add(new SqlParameter("Grievance_ID", Convert.ToInt64(InputModel.Grievance_ID)));
            //Parameters.Add(new SqlParameter("To", Convert.ToInt64(InputModel.To)));
            Parameters.Add(new SqlParameter("Action_Type", InputModel.Action_Type));
            Parameters.Add(new SqlParameter("Remarks", InputModel.Remarks));
            Parameters.Add(new SqlParameter("Status_Text", InputModel.Status_Text));
            Parameters.Add(new SqlParameter("Dms_ID", InputModel.Dms_ID));
            Parameters.Add(new SqlParameter("ChangeOrigin", InputModel.changeOrigin));
            Parameters.Add(new SqlParameter("criteria", InputModel.criteria));
            Parameters.Add(new SqlParameter("Taction", InputModel.Taction));
            Parameters.Add(new SqlParameter("Stakeholder_ID", Stakeholder_ID));
            Parameters.Add(new SqlParameter("Category_ID", InputModel.Category_ID));
            Parameters.Add(new SqlParameter("Sub_Category_ID", InputModel.Sub_Category_ID));



            #region DB CALL & BUSINESS LOGIC
            // _objResponse = GrievanceResponse("TAKE_ACTION_ON_GRIEVANCE", Parameters);

            //DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("TAKE_ACTION_ON_GRIEVANCE"), Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("Take_Action"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {
                    HttpResponseMessage _ResponseMessage = await APICall.PostExternalAPI(GetActor + Convert.ToString(dtresp.Rows[0]["actor_ID"]));
                    var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                    ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);                   

                    if (Convert.ToString(dtresp.Rows[0]["message"]) == "GRIEVANCE RESOLVED")
                    {

                       
                        if (model != null)
                        {
                            // Declare JArray for getting data from response model
                            JArray jArray = JArray.FromObject(model.data);
                            if (this._objHelper.checkJArray(jArray))
                            {
                                Actor_name = Convert.ToString(jArray[0][Convert.ToString("Designation_Name")]);
                                Actor_location = Convert.ToString(jArray[0][Convert.ToString("Office_Name")]);
                            }
                            //var ActorID = model.data;
                        }
                        string message = SMSOTPAFTRRESOLVED.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_ID"]));
                        message = message.Replace("[Actor_name]", Actor_name);
                        message = message.Replace("[Actor_location]", Actor_location);
                        SendSMS(Convert.ToString(dtresp.Rows[0]["citizenmobile"]), message, SMSOTPAFTRRESOLVEDTemplateId);
                    }
                    else {
                        string nextActorIDs = Convert.ToString(dtresp.Rows[0]["nextactor_ID"]);   // 2096,6098,2555,
                        string[] nextActorIdsArr = nextActorIDs.Split(",");
                        for(int i= 0; i< nextActorIdsArr.Length; i++)
                        {
                            if (nextActorIdsArr[i] != "")
                            {

                                HttpResponseMessage _ResponseMessagenext = await APICall.PostExternalAPI(GetActor + nextActorIdsArr[i]);
                                var jsonStringnext = await _ResponseMessagenext.Content.ReadAsStringAsync();
                                ServiceResponseModel modelnext = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonStringnext);
                                if (modelnext != null)
                                {
                                    // Declare JArray for getting data from response model
                                    JArray jArray = JArray.FromObject(modelnext.data);
                                    if (this._objHelper.checkJArray(jArray))
                                    {
                               
                                        string Actor_mobile = Convert.ToString(jArray[0]["Mobile"]);
                                        if (Actor_mobile != "")
                                        {
                                            SendSMS(Convert.ToString(Actor_mobile), SMSTOACTORFWD.Replace("[GriNo]", Convert.ToString(dtresp.Rows[0]["Grievance_ID"])), SMSTOACTORFWDTemplateId);
                                        }
                                    }
                                    //var ActorID = model.data;
                                }
                            }
                        }
                    }
                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);

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

        #region Get Grievance Trail
        [HttpGet("GetGrievanceTrail/{grievanceID}")]
        public async Task<ServiceResponseModel> GetGrievanceTrailAsync(Int64? grievanceID)
        {

            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }

            String both = "";

            // int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Grievance_ID", Convert.ToInt64(grievanceID)));


            #region DB CALL & BUSINESS LOGIC
            //_objResponse = GrievanceResponse("GRIEVANCE_DETAIL_TRAIL", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("GRIEVANCE_DETAIL_TRAIL"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
                else
                {

                    dtresp.Columns.Add(Convert.ToString("Designation_Name"));
                    dtresp.Columns.Add(Convert.ToString("Location_Name"));
                    dtresp.Columns.Add(Convert.ToString("Assigned_To_DesiName"));
                    dtresp.Columns.Add(Convert.ToString("Assigned_To_LocName"));
                    dtresp.Columns.Add(Convert.ToString("Assigned_ActorName"));
                    dtresp.Columns.Add(Convert.ToString("Assigned_TO_ActorName"));
                    dtresp.Columns.Add(Convert.ToString("CommitteeMembersName"));

                    for (int i = 0; i < dtresp.Rows.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(dtresp.Rows[i][1])) && Convert.ToString(dtresp.Rows[i][1]) != "0")
                        {

                            HttpResponseMessage _ResponseMessage = await APICall.PostExternalAPI(GetActor + Convert.ToString(dtresp.Rows[i][1]));
                            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);

                            if (model != null)
                            {
                                // Declare JArray for getting data from response model
                                JArray jArray = JArray.FromObject(model.data);
                                if (this._objHelper.checkJArray(jArray))
                                {
                                    dtresp.Rows[i][Convert.ToString("Assigned_To_DesiName")] = Convert.ToString(jArray[0][Convert.ToString("Designation_Name")]);
                                    dtresp.Rows[i][Convert.ToString("Assigned_To_LocName")] = Convert.ToString(jArray[0][Convert.ToString("Office_Name")]);
                                    dtresp.Rows[i][Convert.ToString("Assigned_TO_ActorName")] = Convert.ToString(jArray[0][Convert.ToString("First_Name")]);
                                }
                                //var ActorID = model.data;
                            }
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(dtresp.Rows[i][2])) && Convert.ToString(dtresp.Rows[i][2]) != "0" && Convert.ToString(dtresp.Rows[i][2]) != "-1")
                        {
                            HttpResponseMessage _ResponseMessage1 = await APICall.PostExternalAPI(GetActor + Convert.ToString(dtresp.Rows[i][2]));
                            var jsonString1 = await _ResponseMessage1.Content.ReadAsStringAsync();
                            ServiceResponseModel model1 = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString1);

                            if (model1.data != null)
                            {
                                // Declare JArray for getting data from response model
                                JArray jArray1 = JArray.FromObject(model1.data);
                                if (this._objHelper.checkJArray(jArray1))
                                {
                                    dtresp.Rows[i][Convert.ToString("Designation_Name")] = Convert.ToString(jArray1[0][Convert.ToString("Designation_Name")]);
                                    dtresp.Rows[i][Convert.ToString("Location_Name")] = Convert.ToString(jArray1[0][Convert.ToString("Office_Name")]);
                                    dtresp.Rows[i][Convert.ToString("Assigned_ActorName")] = Convert.ToString(jArray1[0][Convert.ToString("First_Name")]);
                                }
                                //var ActorID = model.data;
                            }
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtresp.Rows[i][2])) && Convert.ToString(dtresp.Rows[i][3]) != "0" && Convert.ToString(dtresp.Rows[i][3]) != "" && Convert.ToString(dtresp.Rows[i][3]) != "-1")
                        {
                            String commitid = Convert.ToString(dtresp.Rows[i][3]);
                            string[] split = commitid.Split(',');

                            foreach (string item in split)
                            {


                                HttpResponseMessage _ResponseMessage2 = await APICall.PostExternalAPI(GetActor + Convert.ToString(item));
                                var jsonString2 = await _ResponseMessage2.Content.ReadAsStringAsync();
                                ServiceResponseModel model1 = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString2);

                                if (model1.data != null)
                                {
                                    // Declare JArray for getting data from response model
                                    JArray jArray2 = JArray.FromObject(model1.data);
                                    if (this._objHelper.checkJArray(jArray2))
                                    {

                                        var commidesig = Convert.ToString(jArray2[0][Convert.ToString("Designation_Name")]);
                                        var commioffic = Convert.ToString(jArray2[0][Convert.ToString("Office_Name")]);
                                        both = both + commidesig + "(" + commioffic + "), ";
                                        dtresp.Rows[i][Convert.ToString("CommitteeMembersName")] = both;


                                    }
                                    //var ActorID = model.data;
                                }
                            }
                        }
                    }

                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                }
            }
            #endregion
            return _objResponse;
        }
        #endregion

        #region Get PDF File Of Grievance
        [HttpGet("GetPdfFileOfGrievance/{grievanceID}")]
        [AllowAnonymous]
        public ServiceResponseModel GetPDFFileOfGrievance(Int64? grievanceID)
        {
            if (string.IsNullOrEmpty(Convert.ToString(grievanceID)))
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "Grievance ID is required!";
                return _objResponse;
            }

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Grievance_ID", grievanceID));

            //_objResponse = GrievanceResponse("APP_FETCH_GRIEVANCE_BY_GRIEVANCE_ID", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("APP_FETCH_GRIEVANCE_BY_GRIEVANCE_ID"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    //sb.Append("<header class='clearfix'>");
                    //sb.Append("<h1>INVOICE</h1>");
                    sb.Append("<div id='Grievance' class='clearfix'><br /><br />");
                    sb.Append("<div style='text-align:center'><h2>Public Grievance Redressal Portal</h2><h3>Punjab, India</h3></div><br /><br />");
                    sb.Append("<table style=''border-bottom:1pt solid black;''>");
                    sb.Append("<tr>");
                    sb.Append("<td>Grievance ID : " + Convert.ToString(dtresp.Rows[0]["Grievance_ID"]) + "</td>");
                    sb.Append("<td style='text-align:right'>Date : " + Convert.ToString(dtresp.Rows[0]["Created_On"]) + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                    sb.Append("-----------------------------------------------------------------------------------");
                    sb.Append("------------------------------------------------------------<br />");
                    sb.Append("<table style='border-bottom:1pt solid black;'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Citizen Name : " + Convert.ToString(dtresp.Rows[0]["Citizen_Name"]) + "</td>");
                    sb.Append("<td style='text-align:right'>Email : " + Convert.ToString(dtresp.Rows[0]["Citizen_Email"]) + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Contact : " + Convert.ToString(dtresp.Rows[0]["Citizen_Mobile_No"]) + "</td>");
                    sb.Append("<td style='text-align:right'>District : " + Convert.ToString(dtresp.Rows[0]["Citizen_District"]) + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr style='border-bottom:1pt solid black;'>");
                    sb.Append("<td>Address : " + Convert.ToString(dtresp.Rows[0]["Citizen_Address"]) + "</td>");
                    sb.Append("<td></td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                    sb.Append("-----------------------------------------------------------------------------------");
                    sb.Append("------------------------------------------------------------<br />");
                    sb.Append("<table style='border-bottom:1pt solid black;'>");
                    sb.Append("<tr>");
                    sb.Append("<td>Complaint Title : " + Convert.ToString(dtresp.Rows[0]["Application_Title"]) + "</td>");
                    sb.Append("<td style='text-align:right'>Department : " + Convert.ToString(dtresp.Rows[0]["Application_Department_Name"]) + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Complainant Category : " + Convert.ToString(dtresp.Rows[0]["Citizen_Type"]) + "</td>");
                    sb.Append("<td style='text-align:right'>District : " + Convert.ToString(dtresp.Rows[0]["Application_District_Name"]) + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr style='border-bottom:1pt solid black;'>");
                    sb.Append("<td>Complaint Category : " + Convert.ToString(dtresp.Rows[0]["Category_Label"]) + "</td>");
                    sb.Append("<td style='text-align:right'>Complaint Sub Category : " + Convert.ToString(dtresp.Rows[0]["Sub_Category_Label"]) + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td></td>");
                    sb.Append("<td></td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                    sb.Append("<div>Complaint Detail : " + Convert.ToString(dtresp.Rows[0]["Application_Description"]) + "</div>");
                    sb.Append("</div>");
                    //sb.Append("</header>");

                    StringReader sr = new StringReader(sb.ToString());

                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        pdfDoc.Open();

                        htmlparser.Parse(sr);
                        pdfDoc.Close();

                        byte[] bytes = memoryStream.ToArray();
                        base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                        memoryStream.Close();

                        _objResponse.response = 1;
                        _objResponse.sys_message = base64String;
                    }
                }
            }

            return _objResponse;
        }
        #endregion

        #region Get Actions
        [HttpGet("GetActionBystageid/{grievance_id}/{empid}")]
        public ServiceResponseModel GetActionBystageid(string grievance_id, string empid)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("grievance_id", grievance_id));
            Parameters.Add(new SqlParameter("empid", empid));
            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("App_Fetch_Action_By_GrievanceId", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

       

        #region
        [HttpGet("GetNextSelectionCriteriaBYId/{id}")]
        public ServiceResponseModel GetNextSelectionCriteriaBYId (string ID)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("SelectionCriteria", ID));
            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("App_Next_SelectionCriteria_BYId", Parameters);
            #endregion
            return _objResponse;

        }
        #endregion
        //--------------------------sms API---------------------------------------------
        #region Send SMS
        [AllowAnonymous]
        [HttpPost("sendSMS")]
        public async Task<ServiceResponseModel> PostAsync([FromBody] SmsController sms)
        {

            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }

            // string status = _objHelper.SendSMS(sms.mobileno, sms.message);
            _objResponse.sys_message =  APICall.SendSMS(sms.mobileno, sms.message, sms.Template_Id);

            _objResponse.data = null;
            _objResponse.response = 1;
            return _objResponse;
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
                    string json = "{ 'mobile_no':'" + mobilenumber + "','message':'" + msg + "','template_id':'" + templateId + "','is_unicode':'" + false +"'}";
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
        #region Return IP
        [AllowAnonymous]
        [HttpGet("returnuserip")]
        public async Task<ServiceResponseModel> returnUserip()
        {
           
           
            try
            {
                 var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
               //  string[] ips = remoteIpAddress.Select(ip => ip.ToString()).ToArray();

              //  var ipAddress = HttpContext.Connection.RemoteIpAddress;

                IPHostEntry heserver = Dns.GetHostEntry(Dns.GetHostName());
                var ip1 = heserver.AddressList[0].ToString();


                _objResponse.response = 1;
                _objResponse.data = "Client IP: " + remoteIpAddress;
                _objResponse.sys_message = "Server IP: " +  ip1;
                return _objResponse;
            }
            catch (Exception ex)
            {

                _objResponse.response = 0;
                _objResponse.sys_message = ex.Message;
                return _objResponse;
            }
        }
 
        #endregion
        //--------------------------------------------------------------------------------------------------------------------   
        #region email Report
        [AllowAnonymous]
        [HttpGet("getemailreport")]
        public async Task<ServiceResponseModel> emailReportcntrl()
        {
            List<KeyValuePair<string, string>> emails = new List<KeyValuePair<string, string>>();
            String emaildata = String.Empty;
            String rowstyle = String.Empty;
            String total = String.Empty;
            String pending = String.Empty;
            String resolved = String.Empty;
            String overdue = String.Empty;
            String todate = String.Empty;
            String encodedurlunderprocess = String.Empty;
            String encodedurlpending = String.Empty;
            String encodedurltotal = String.Empty;
            String underprocesstotal = String.Empty;
            String encodedurlresolved = String.Empty;
            int stat1;
            // string status = _objHelper.SendSMS(sms.mobileno, sms.message);
            try
            {
                //call of external api to get Stakehoder those are onboared on Grievance

                //  HttpResponseMessage _ResponseMessage = await APICall.PostExternalAPI(emailReport);
                //var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();

                //   emailReportModelMain reportList = JsonConvert.DeserializeObject<emailReportModelMain>(jsonString);


                var dataTable = new DataTable();
                dataTable.Columns.Add("response", typeof(String));
                dataTable.Columns.Add("Stakeholder_ID", typeof(String));
                dataTable.Columns.Add("Stakeholder_Name", typeof(String));
                dataTable.Columns.Add("Stakeholder_Local_Language", typeof(String));


                emailReportModelMain reportList = new emailReportModelMain();
                List<SqlParameter> Parametersstake = new List<SqlParameter>();
                Parametersstake.Add(new SqlParameter("isactive", '1'));
              
                // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
                DataTable dtresptake = _MSSQLGateway.ExecuteProcedure(Convert.ToString("getstakeholders"), Parametersstake);
                if (_objHelper.checkDBResponse(dtresptake))
                {
                    if (Convert.ToString(dtresptake.Rows[0]["response"]) == "1")
                    {

                        for (int statrec = 0; statrec < dtresptake.Rows.Count; statrec++)
                        {
                           
                            var row = dataTable.NewRow();
                            row["response"] = dtresptake.Rows[statrec]["response"];
                            row["Stakeholder_ID"] = dtresptake.Rows[statrec]["Stakeholder_ID"];
                            row["Stakeholder_Name"] = dtresptake.Rows[statrec]["Stakeholder_Name"];
                            row["Stakeholder_Local_Language"] = dtresptake.Rows[statrec]["Stakeholder_Local_Language"];
                            dataTable.Rows.Add(row);
                        }

                    }
                }
                //Get Grievance states
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("stakeholdertable", (dataTable)));
                DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("emailreport"), Parameters);

                //create email body
                for (int stat = 0; stat < dtresp.Rows.Count; stat++)
                {
                    stat1 = stat + 1;
                    rowstyle = stat % 2 == 0 ? "#F5F5F5" : "";
                    todate = DateTime.Now.ToString("dd/MM/yyyy");
                    // for overdue
                    encodedurlunderprocess = Convert.ToBase64String(Encoding.UTF8.GetBytes("grievance/reports?filter=deshboard&Status=overdue&fromdate=19/05/2020&todate=" + todate + "&Dept=" + dtresp.Rows[stat]["Stakeholder_ID"]));
                    overdue = AbsolutePortalURL + encodedurlunderprocess;
                    //underprocess = "http://localhost:62947/login?ReturnUrl=" + encodedurlunderprocess;
                    // for pending
                    encodedurlpending = Convert.ToBase64String(Encoding.UTF8.GetBytes("grievance/reports?filter=deshboard&Status=PENDING&fromdate=19/05/2020&todate=" + todate + "&Dept=" + dtresp.Rows[stat]["Stakeholder_ID"]));
                    pending = AbsolutePortalURL + encodedurlpending;
                   // pending = "http://localhost:62947/login?ReturnUrl=" + encodedurlpending;
                    // for total
                    encodedurltotal = Convert.ToBase64String(Encoding.UTF8.GetBytes("grievance/reports?filter=deshboard&fromdate=19/05/2020&todate=" + todate + "&Dept=" + dtresp.Rows[stat]["Stakeholder_ID"]));
                     total = AbsolutePortalURL + encodedurltotal;
                    //total = "http://localhost:62947/login?ReturnUrl=" + encodedurltotal;
                    // for resolved
                    encodedurlresolved = Convert.ToBase64String(Encoding.UTF8.GetBytes("grievance/reports?filter=deshboard&&Status=RESOLVED&fromdate=19/05/2020&todate=" + todate + "&Dept=" + dtresp.Rows[stat]["Stakeholder_ID"]));
                    resolved = AbsolutePortalURL + encodedurlresolved;
                  //  resolved = "http://localhost:62947/login?ReturnUrl=" + encodedurlresolved;

                    emaildata = emaildata + "<tr>" +
                        "<td bgcolor='" + rowstyle + "' align='center'>" + stat1 + "</td>" +
                        "<td  bgcolor='"+ rowstyle  + "' width='50%'>" + dtresp.Rows[stat]["Stakeholder_Name"] + "</td>" +
                        "<td align='center' bgcolor='" + rowstyle + "' width='10%' ><a href='" + total + "'>" + dtresp.Rows[stat]["total"] + "</a></td>" +
                        "<td align='center' bgcolor='" + rowstyle + "' bgcolor='#ccc' width='10%'><a href='" + pending + "'>" + dtresp.Rows[stat]["pending"] + "</a></td>" +
                        "<td align='center' bgcolor='" + rowstyle + "' width='10%'><a href='" + resolved + "'>" + dtresp.Rows[stat]["Resolved"] + "</a></td>" +
                       "<td align='center' bgcolor='" + rowstyle + "' width='10%'><a href='" + overdue + "'>" + dtresp.Rows[stat]["overdue"] + "</a></td>" +
                       "<td align='center' bgcolor='" + rowstyle + "' width='10%'>" + dtresp.Rows[stat]["IVRRequests"] + "</td>" +
                       "<td align='center' bgcolor='" + rowstyle + "' width='10%'>" + dtresp.Rows[stat]["IVRResponses"] + "</td>" +
                        "</tr>";
                }

                String emailbody = "<center><h4>Grievance Report as on " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt") +
                    "</h4></center><table style='border: 1px solid #cccc'><tr bgcolor='#22363a'>" +
                    "<th align='center' style='color:#F5F5F5' width='10%'>No.</th>" +
                    "<th align='center' style='color:#F5F5F5' width='50%'>Department Name</th>" +
                    "<th align='center' style='color:#F5F5F5' width='10%'>Total Grievance</th>" +
                    "<th align='center' style='color:#F5F5F5' width='10%'>Pending Grievance</th>" +
                    "<th align='center' style='color:#F5F5F5' width='10%'>Resolved Grievance</th>" +
                     "<th align='center' style='color:#F5F5F5' width='10%'>Overdue Grievance</th>" +
                     "<th align='center' style='color:#F5F5F5' width='10%'>IVR Requests</th>" +
                     "<th align='center' style='color:#F5F5F5' width='10%'>IVR Responses</th>" +

                    "</tr>" +
                    emaildata +
                    "</table>";
                
                //Get officers emails from database 
                var dictionary = new Dictionary<string, object>();
                List<SqlParameter> Parameters1 = new List<SqlParameter>();
                Parameters1.Add(new SqlParameter("emails", "all"));
                DataTable dtresp_emails = _MSSQLGateway.ExecuteProcedure(Convert.ToString("GET_OFFICERS_EMAILS"), Parameters1);

                _emailobj.emails = _objHelper.ConvertTableToDictionary(dtresp_emails);
                _emailobj.body = emailbody;

                //External APIs call to send email
                //HttpResponseMessage _ResponseMessageEmail = await APICall.PostExternalAPI(SendEmailEndpoint, _emailobj);
                //var jsonStringEmail = await _ResponseMessageEmail.Content.ReadAsStringAsync();

                //if (_objHelper.checkDBResponse(dtresp))
                //{

                //    _objResponse.response = 1;
                //    _objResponse.data = jsonStringEmail;
                //    _objResponse.sys_message = "done";
                //    return _objResponse;

                //}
                //return _objResponse;


               // _emailobjList.emails = _objHelper.ConvertTableToDictionary(dtresp_emails);
                List<EmailData> _EmailData = new List<EmailData>();
                for (int l = 0; l < dtresp_emails.Rows.Count; l++)
                {
                    EmailData _ed = new EmailData();
                    _ed.emailId = Convert.ToString(dtresp_emails.Rows[l]["emailId"]);
                    _ed.subject = Convert.ToString(dtresp_emails.Rows[l]["subject"]);
                    _EmailData.Add(_ed);
                }


                _emailobjList.body = emailbody;
                _emailobjList.emails = _EmailData;
                ServiceResponseModel _ServiceResponseModel = await sendEmail.SendMultipleEmail(_emailobjList);


                return _ServiceResponseModel;

            }
            catch (Exception ex)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = ex.Message;
                return _objResponse;
            }

        }

        #endregion

        // Non API Route Methods

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



        #region Get Actor notification List
        [HttpGet("GetGrievanceNotification")]
        public ServiceResponseModel GetGrievanceNotification()
        {
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Actor_ID", actor_id));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("actornotification", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region SET Actor notification List
        [HttpGet("SetGrievanceNotification")]
        public ServiceResponseModel SetGrievanceNotification()
        {
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Actor_ID", actor_id));
            Parameters.Add(new SqlParameter("set_Flag", "1"));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("actornotification", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion


        #region SET Actor departement role table
        [HttpGet("getActordepartmentbyid")]
        public ServiceResponseModel getActordepartmentbyid()
        {
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Actor_ID", actor_id));

            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("getActorRoles", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region SET Actor departement role table
        [HttpGet("getdistrictbyRoleID")]
        public ServiceResponseModel getdistrictbyRoleID()
        {
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            string Roles = Convert.ToString(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Roles"));
           

            string[] roles = Roles.Split(',');
            if (roles.Contains(" 5") || roles.Contains("5") )
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("Actor_ID", actor_id));
                Parameters.Add(new SqlParameter("Role_ID", 5));

                #region DB CALL & BUSINESS LOGIC
                _objResponse = GrievanceResponse("getActorDistrictbyRoleID", Parameters);
                #endregion
            }
           else if (roles.Contains(" 6493") || roles.Contains("6493"))
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("Actor_ID", actor_id));
                Parameters.Add(new SqlParameter("Role_ID", 6493));

                #region DB CALL & BUSINESS LOGIC
                _objResponse = GrievanceResponse("getActorDistrictbyRoleID", Parameters);
                #endregion
            }
           else if (roles.Contains(" 6492") || roles.Contains("6492"))
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("Actor_ID", actor_id));
                Parameters.Add(new SqlParameter("Role_ID", 6492));

                #region DB CALL & BUSINESS LOGIC
                _objResponse = GrievanceResponse("getActorDistrictbyRoleID", Parameters);
                #endregion
            }
            else if (roles.Contains(" 6495") || roles.Contains("6495"))
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(new SqlParameter("Actor_ID", actor_id));
                Parameters.Add(new SqlParameter("Role_ID", 6495));

                #region DB CALL & BUSINESS LOGIC
                _objResponse = GrievanceResponse("getActorDistrictbyRoleID", Parameters);
                #endregion
            }
            else {
                _objResponse.response = 0;
                _objResponse.sys_message = "No record";
                _objResponse.data = null;
            }

            return _objResponse;
        }
        #endregion

        public ServiceResponseModel GenerateAccessToken()
        {
            string ret = "";
            ServiceResponseModel apiResponse = new ServiceResponseModel();
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.Smsurl + "/g2gapiaccess/generateToken");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "Post";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{ 'Access_Key':'phNcic3Qkr6lSPYYtACS', 'Public_Key' : 'Q74XKdEoaHZ8TdpHejAC'}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = JsonConvert.DeserializeObject<ServiceResponseModel>(streamReader.ReadToEnd());
                    apiResponse = result;
                }
            }
            catch (Exception ex)
            {
                ret = ex.Message.ToString();
            }
            return apiResponse;
        }


        #region ReappealByCitizen
        [HttpGet("ReappealByCitizen/{grievance_id}")]
        public ServiceResponseModel ReappealByCitizen(long grievance_id)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("GrievanceId", grievance_id));
            #region DB CALL & BUSINESS LOGIC
            _objResponse = GrievanceResponse("grievance_Escalate_by_Citizen", Parameters);
            #endregion
            return _objResponse;
        }
        #endregion

        #region Get Location Type
        [HttpGet("GetLocationTypeByDept/{deptId}")]
        public ServiceResponseModel GetLocationTypeByDept(long deptId)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("deptid", deptId));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("APP_FETCH_LocationType", Parameters);
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
        public ServiceResponseModel GetComplaintStatus_PMIDC([FromBody] PMIDCStatus model )
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

        #region Get Reminder/Feedback Trail
        [HttpGet("GetReminderFeedbackTrail/{grievanceID}")]
        public async Task<ServiceResponseModel> GetReminderFeedbackTrailAsync(Int64? grievanceID)
        {

            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("GrievanceId", Convert.ToInt64(grievanceID)));

            #region DB CALL & BUSINESS LOGIC
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("APP_FETCH_REMINDER_FEEDBACK"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                }
                else
                {
                    dtresp.Columns.Add(Convert.ToString("Assigned_By_Designation"));
                    dtresp.Columns.Add(Convert.ToString("Assigned_By_Location"));
                    dtresp.Columns.Add(Convert.ToString("Assigned_By_ActorName"));
               
                    for (int i = 0; i < dtresp.Rows.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(dtresp.Rows[i][3])))
                        {

                            HttpResponseMessage _ResponseMessage = await APICall.PostExternalAPI(GetActor + Convert.ToString(dtresp.Rows[i][3]));
                            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
                            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);

                            if (model != null)
                            {
                                // Declare JArray for getting data from response model
                                JArray jArray = JArray.FromObject(model.data);
                                if (this._objHelper.checkJArray(jArray))
                                {
                                    dtresp.Rows[i][Convert.ToString("Assigned_By_Designation")] = Convert.ToString(jArray[0][Convert.ToString("Designation_Name")]);
                                    dtresp.Rows[i][Convert.ToString("Assigned_By_Location")] = Convert.ToString(jArray[0][Convert.ToString("Office_Name")]);
                                    dtresp.Rows[i][Convert.ToString("Assigned_By_ActorName")] = Convert.ToString(jArray[0][Convert.ToString("First_Name")]);
                                }
                            }
                        }
                    }

                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                }
            }
            #endregion
            return _objResponse;
        }
        #endregion

    }
}
