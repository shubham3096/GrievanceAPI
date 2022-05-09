using EmailService.Controllers;
using EmailService.Models;
using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("citizen")]
    public class EACitizenControllerV2 : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private MSSQLGateway _MSSQLGateway_Aadhaar;
        private IHostingEnvironment _env;
        CommonFunctions APICall;
        string GetActor = string.Empty;
        string mdmurl = string.Empty;
        private ActorLogSessionModel _ActorLogSessionModel = new ActorLogSessionModel();
        List<SqlParameter> param = new List<SqlParameter>();
        EmailController sendEmail;
        string SendEmailEndpoint = string.Empty;


        private string SMSOTP = string.Empty;
        private string SMSOTPTemplateId = string.Empty;
        string ssoCitizenServerkey = string.Empty;
        string ssoCitizenServerUrl = string.Empty;
        readonly string registerPassMessage = string.Empty;
        #endregion

        public EACitizenControllerV2(IConfiguration configuration, IHostingEnvironment env)
        {
            sendEmail = new EmailController(configuration, env);
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;

            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this._MSSQLGateway_Aadhaar = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_Aadhaar"));
                this.GetActor = this._configuration["AppSettings_Dev:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
                this.SendEmailEndpoint = this._configuration["AppSettings_Dev:SendEmailEndpoint"];
                this.SMSOTP = this._configuration["AppSettings_Dev:SMSOTP"];
                this.SMSOTPTemplateId = this._configuration["AppSettings_Dev:SMSOTP_TemplateId"];
                this.ssoCitizenServerkey = this._configuration["AppSettings_Dev:ssoCitizenServerkey"];
                this.ssoCitizenServerUrl = this._configuration["AppSettings_Dev:ssoCitizenServerUrl"];
                this.registerPassMessage = this._configuration["AppSettings_Dev:registerPassMessage"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this._MSSQLGateway_Aadhaar = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag_Aadhaar"));
                this.GetActor = this._configuration["AppSettings_Stag:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
                this.SendEmailEndpoint = this._configuration["AppSettings_Stag:SendEmailEndpoint"];
                this.SMSOTP = this._configuration["AppSettings_Dev:SMSOTP"];
                this.SMSOTPTemplateId = this._configuration["AppSettings_Dev:SMSOTP_TemplateId"];
                this.ssoCitizenServerkey = this._configuration["AppSettings_Stag:ssoCitizenServerkey"];
                this.ssoCitizenServerUrl = this._configuration["AppSettings_Stag:ssoCitizenServerUrl"];
                this.registerPassMessage = this._configuration["AppSettings_Stag:registerPassMessage"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway_Aadhaar = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro_Aadhaar"));
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.GetActor = this._configuration["AppSettings_Pro:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
                this.SendEmailEndpoint = this._configuration["AppSettings_Pro:SendEmailEndpoint"];
                this.SMSOTP = this._configuration["AppSettings_Dev:SMSOTP"];
                this.SMSOTPTemplateId = this._configuration["AppSettings_Dev:SMSOTP_TemplateId"];
                this.ssoCitizenServerkey = this._configuration["AppSettings_Pro:ssoCitizenServerkey"];
                this.ssoCitizenServerUrl = this._configuration["AppSettings_Pro:ssoCitizenServerUrl"];
                this.registerPassMessage = this._configuration["AppSettings_Pro:registerPassMessage"];
            }
        }


        /// <summary>
        /// Citizen Registration
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        #region Citizen Registration
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] EACitizenModel model)
        {
            ServiceResponseModel serviceResult = new ServiceResponseModel();
            if (ModelState.IsValid)
            {
                param.Add(new SqlParameter("First_Name", Convert.ToString(model.First_Name)));
                param.Add(new SqlParameter("Middle_Name", Convert.ToString(model.Middle_Name)));
                param.Add(new SqlParameter("Last_Name", Convert.ToString(model.Last_Name)));
                param.Add(new SqlParameter("Father_Name", Convert.ToString(model.Father_Name)));
                param.Add(new SqlParameter("Mother_Name", Convert.ToString(model.Mother_Name)));
                param.Add(new SqlParameter("Email_ID", Convert.ToString(model.Email_ID)));
                param.Add(new SqlParameter("Phone_No", Convert.ToString(model.Phone_No)));
                param.Add(new SqlParameter("Date_Of_Birth", Convert.ToDateTime(model.Date_Of_Birth).ToString("yyyy-MM-dd")));
                param.Add(new SqlParameter("Gender", Convert.ToString(model.Gender)));
                param.Add(new SqlParameter("Profile_Type", Convert.ToString("Citizen")));
                param.Add(new SqlParameter("Address_Type", Convert.ToString(model.Address_Type)));
                param.Add(new SqlParameter("Address_Line_1", Convert.ToString(model.Address_Line_1)));
                param.Add(new SqlParameter("Address_Line_2", Convert.ToString(model.Address_Line_2)));
                param.Add(new SqlParameter("State_Ref_ID", "3"));
                param.Add(new SqlParameter("District_Ref_ID", Convert.ToString(model.District_Ref_ID)));
                param.Add(new SqlParameter("Tehsil_Ref_ID", Convert.ToInt64(model.Tehsil_Ref_ID)));
                param.Add(new SqlParameter("Village_ID", Convert.ToInt64(model.Village_ID)));
                param.Add(new SqlParameter("Village_Name", Convert.ToString(model.Village_Name)));
                param.Add(new SqlParameter("Municipality_ID", Convert.ToInt64(model.Municipality_ID)));
                param.Add(new SqlParameter("Pincode", Convert.ToString(model.Pincode)));
                param.Add(new SqlParameter("Aadhaar_Ref_ID", Convert.ToString(model.Aadhaar_Ref_ID)));
                param.Add(new SqlParameter("Created_By", 0));
                param.Add(new SqlParameter("Registered_From", model.Registered_From));

                DataTable dtResponse = _MSSQLGateway.ExecuteProcedure("MD_INSERT_CITIZENS", param);
                if (_objHelper.checkDBResponse(dtResponse))
                {
                    if (Convert.ToInt32(Convert.ToString(dtResponse.Rows[0]["response"])) <= 0)
                    {
                        serviceResult.response = Convert.ToInt32(Convert.ToString(dtResponse.Rows[0]["response"]));
                        serviceResult.sys_message = Convert.ToString(dtResponse.Rows[0]["message"]);
                        serviceResult.data = null;
                        serviceResult.response_code = "403";
                    }
                    else
                    {
                        string generateOTP = _objHelper.generateOTP(6);
                        
                        CitizenLoginModel clm = new CitizenLoginModel();
                        clm.User_Name = Convert.ToString(model.Phone_No).Trim().ToLower();
                       
                        if(model.Registered_From == "SSO")
                        {
                            model.Password = RandomPassword(9);
                            string message = registerPassMessage.Replace("[password]", model.Password);
                            message = message.Replace("[username]", model.Phone_No);
                            
                            //Send Password Via SMS
                            SMSModel sms = new SMSModel();
                            sms.mobileno = Convert.ToString(model.Phone_No).Trim();
                            sms.message = message;

                            //Bind Password Via Email Model
                            EmailData ed = new EmailData();
                            ed.emailId = model.Email_ID;
                            ed.subject = "Citizen Password";
                            

                            //APICall.SendSMS(sms.mobileno, sms.message, SMSOTPTemplateId);
                            //sendEmail.SendEmail(model.Email_ID,ed.subject,message,"noreply@punjab.gov.in");
                            
                            clm.User_Password = Convert.ToString(_objHelper.ConvertToSHA512(model.Password));
                        }

                        clm.User_Password = Convert.ToString(_objHelper.ConvertToSHA512(model.Password));
                        clm.Citizen_Ref_ID = Convert.ToInt32(dtResponse.Rows[0]["Citizen_ID"]);
                        clm.OTP = generateOTP;
                        clm.OTP_Verified_From = model.Registered_From;

                        //Insert Login Details in Auth Table
                        ServiceResponseModel authResponse = await InsertCitizenLogin(clm);
                        if(authResponse.response == 1)
                        {
                            ServiceResponseModel fetchDetails = new ServiceResponseModel();
                            if (model.Registered_From == "SSO")
                            {
                                List<SqlParameter> param1 = new List<SqlParameter>
                                {
                                    new SqlParameter("Mobile_no",model.Phone_No)
                                };

                                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("get_Citizen_Details_By_MobileNo", param1);
                                if (_objHelper.checkDBResponse(dtresp))
                                {
                                    List<SqlParameter> paramet = new List<SqlParameter>
                                    {
                                        new SqlParameter("User_Name", Convert.ToString(dtresp.Rows[0]["User_Name"])),
                                        new SqlParameter("pass", Convert.ToString(dtresp.Rows[0]["User_Password"]))
                                    };
                                    fetchDetails= CitizenFetchDetails(paramet);
                                    serviceResult.response = 1;
                                    serviceResult.data = fetchDetails.data;
                                    serviceResult.sys_message = fetchDetails.sys_message;
                                    serviceResult.response_code = "200";
                                }
                                else
                                {
                                    serviceResult.response = 0;
                                    serviceResult.data = null;
                                    serviceResult.sys_message = "No User Found";
                                    serviceResult.response_code = "404";
                                }
                            }
                            else
                            {
                                //Bind SMS Model
                                SMSModel sms = new SMSModel();
                                sms.mobileno = Convert.ToString(model.Phone_No).Trim();
                                sms.message = SMSOTP.Replace("[OTP]", Convert.ToString(generateOTP));

                                //Bind Email Model
                               EmailData ed = new EmailData();
                                ed.emailId = model.Email_ID;
                                ed.subject = "Citizen OTP";
                             
                                APICall.SendSMS(sms.mobileno, sms.message, SMSOTPTemplateId);
                                sendEmail.SendEmail(ed.emailId,ed.subject,sms.message, "noreply@punjab.gov.in");
                                serviceResult.response = Convert.ToInt32(Convert.ToString(authResponse.response));
                                serviceResult.data = _objHelper.ConvertTableToDictionary(dtResponse);
                                serviceResult.sys_message = Convert.ToString(authResponse.sys_message);
                                serviceResult.response_code = "201";
                            }
                        }
                        else
                        {
                            serviceResult.response = Convert.ToInt32(Convert.ToString(authResponse.response));
                            serviceResult.data = null;
                            serviceResult.sys_message = Convert.ToString(authResponse.sys_message);
                            serviceResult.response_code = "403";
                        }
                    }
                }
                return Ok(serviceResult);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        #endregion

        #region Insert Citizen Login
        public async Task<ServiceResponseModel> InsertCitizenLogin(CitizenLoginModel clm)
        {
            ServiceResponseModel responseModel = new ServiceResponseModel();
            try
            {
                List<SqlParameter> param1 = new List<SqlParameter>();

                param1.Add(new SqlParameter("User_Name", clm.User_Name));
                param1.Add(new SqlParameter("User_Password", clm.User_Password));
                param1.Add(new SqlParameter("Citizen_Ref_ID", clm.Citizen_Ref_ID));
                param1.Add(new SqlParameter("OTP", clm.OTP));
                if (clm.OTP_Verified_From == "SSO")
                {
                    param1.Add(new SqlParameter("OTP_Is_Verified", "1"));
                }
                else
                {
                    param1.Add(new SqlParameter("OTP_Is_Verified", "0"));
                }
                param1.Add(new SqlParameter("Status", "A"));
                param1.Add(new SqlParameter("Created_By", "0"));

                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("MD_INSERT_AUTH_CITIZEN_LOGIN", param1);
                responseModel.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                if(responseModel.response == 1)
                {
                    responseModel.response_code = "201";
                }
                else
                {
                    responseModel.response_code = "403";
                }
                responseModel.sys_message =dtresp.Rows[0]["message"].ToString();
                responseModel.data = _objHelper.ConvertTableToDictionary(dtresp);
            }

            catch (Exception ex)
            {
                responseModel.response = 0;
                responseModel.response_code = "503";
                responseModel.sys_message = ex.ToString();
                responseModel.data = null;
            }
            return responseModel;
        }
        #endregion
        
        #region Fetch Citizen Details
        public ServiceResponseModel CitizenFetchDetails(List<SqlParameter> sqlParameters)
        {
            ServiceResponseModel _objResponse = new ServiceResponseModel(); 
            DataTable _dtResp = _MSSQLGateway.ExecuteProcedure("APP_FETCH_AUTH_CITIZEN", sqlParameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    // Generate Login Token
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim("Citizen_Ref_ID", Convert.ToString(_dtResp.Rows[0]["Citizen_Ref_ID"])),
                        new Claim("is_first_login",  Convert.ToString(_dtResp.Rows[0]["Is_First_Login"])),
                        new Claim("is_pass_exp",  Convert.ToString(_dtResp.Rows[0]["Is_Pass_Exp"])),
                        new Claim("login_id",  Convert.ToString(_dtResp.Rows[0]["Login_ID"])),
                        new Claim(ClaimTypes.Role,  "Citizen"),
                    };

                    string token = this.GenerateToken(claims);
                    // Http request for getting Profile data
                    DataTable _resCitizen = GetCitizen(Convert.ToInt32(_dtResp.Rows[0]["Citizen_Ref_ID"]));
                    if (this._objHelper.checkDBResponse(_resCitizen))
                    {
                        // Add new columns in respnse model
                        _dtResp.Columns.Add("First_Name");
                        _dtResp.Columns.Add("Middle_Name");
                        _dtResp.Columns.Add("Last_Name");
                        _dtResp.Columns.Add("Date_Of_Birth");
                        _dtResp.Columns.Add("Email_ID");
                        _dtResp.Columns.Add("Phone_No");
                        _dtResp.Columns.Add("Gender");
                        _dtResp.Columns.Add("Address_Line_1");
                        _dtResp.Columns.Add("Address_Line_2");
                        _dtResp.Columns.Add("Address_Type");
                        _dtResp.Columns.Add("District_Ref_ID");
                        _dtResp.Columns.Add("Village_Name");
                        _dtResp.Columns.Add("State_Ref_ID");
                        _dtResp.Columns.Add("Tehsil_Ref_ID");
                        _dtResp.Columns.Add("Village_ID");
                        _dtResp.Columns.Add("Municipality_ID");
                        _dtResp.Columns.Add("Municipality_Name");
                        _dtResp.Columns.Add("Is_Last_Correspondence");
                        _dtResp.Columns.Add("Is_Permanent");
                        _dtResp.Columns.Add("Pincode");
                        _dtResp.Columns.Add("Profile_Type");
                        _dtResp.Columns.Add("User_Type");
                        _dtResp.Columns.Add("System_Type");
                        _dtResp.Columns.Add("District_Name");
                        _dtResp.Columns.Add("Tehsil_Name");
                        _dtResp.Columns.Add("Aadhaar_Ref_ID");


                        _dtResp.Rows[0]["First_Name"] = Convert.ToString(_resCitizen.Rows[0]["First_Name"]);
                        _dtResp.Rows[0]["Middle_Name"] = Convert.ToString(_resCitizen.Rows[0]["Middle_Name"]);
                        _dtResp.Rows[0]["Last_Name"] = Convert.ToString(_resCitizen.Rows[0]["Last_Name"]);
                        _dtResp.Rows[0]["Date_Of_Birth"] = Convert.ToString(_resCitizen.Rows[0]["Date_Of_Birth"]);
                        _dtResp.Rows[0]["Email_ID"] = Convert.ToString(_resCitizen.Rows[0]["Email_ID"]);
                        _dtResp.Rows[0]["Phone_No"] = Convert.ToString(_resCitizen.Rows[0]["Phone_No"]);
                        _dtResp.Rows[0]["Gender"] = Convert.ToString(_resCitizen.Rows[0]["Gender"]);
                        _dtResp.Rows[0]["Address_Line_1"] = Convert.ToString(_resCitizen.Rows[0]["Address_Line_1"]);
                        _dtResp.Rows[0]["Address_Line_2"] = Convert.ToString(_resCitizen.Rows[0]["Address_Line_2"]);
                        _dtResp.Rows[0]["Address_Type"] = Convert.ToString(_resCitizen.Rows[0]["Address_Type"]);
                        _dtResp.Rows[0]["District_Ref_ID"] = Convert.ToString(_resCitizen.Rows[0]["District_Ref_ID"]);
                        _dtResp.Rows[0]["State_Ref_ID"] = Convert.ToString(_resCitizen.Rows[0]["State_Ref_ID"]);
                        _dtResp.Rows[0]["Tehsil_Ref_ID"] = Convert.ToString(_resCitizen.Rows[0]["Tehsil_Ref_ID"]);
                        _dtResp.Rows[0]["Village_ID"] = Convert.ToString(_resCitizen.Rows[0]["Village_ID"]);
                        _dtResp.Rows[0]["Village_Name"] = Convert.ToString(_resCitizen.Rows[0]["Village_Name"]);
                        _dtResp.Rows[0]["Municipality_ID"] = Convert.ToString(_resCitizen.Rows[0]["Municipality_ID"]);
                        _dtResp.Rows[0]["Municipality_Name"] = Convert.ToString(_resCitizen.Rows[0]["Municipality_Name"]);
                        _dtResp.Rows[0]["Is_Last_Correspondence"] = Convert.ToString(_resCitizen.Rows[0]["Is_Last_Correspondence"]);
                        _dtResp.Rows[0]["Is_Permanent"] = Convert.ToString(_resCitizen.Rows[0]["Is_Permanent"]);
                        _dtResp.Rows[0]["Pincode"] = Convert.ToString(_resCitizen.Rows[0]["Pincode"]);
                        _dtResp.Rows[0]["Profile_Type"] = Convert.ToString(_resCitizen.Rows[0]["Profile_Type"]);
                        _dtResp.Rows[0]["Tehsil_Name"] = Convert.ToString(_resCitizen.Rows[0]["Tehsil_Name"]);
                        _dtResp.Rows[0]["District_Name"] = Convert.ToString(_resCitizen.Rows[0]["District_Name"]);
                        _dtResp.Rows[0]["Aadhaar_Ref_ID"] = Convert.ToString(_resCitizen.Rows[0]["Aadhaar_Ref_ID"]);
                        _dtResp.Rows[0]["User_Type"] = "Citizen";
                        _dtResp.Rows[0]["System_Type"] = "EA";

                    }
                    
                    // Generate Login Token
                    claims = new List<Claim>
                    {
                        new Claim("Citizen_Ref_ID", Convert.ToString(_dtResp.Rows[0]["Citizen_Ref_ID"])),
                        new Claim("First_Name", Convert.ToString(_dtResp.Rows[0]["First_Name"])),
                        new Claim("Middle_Name", Convert.ToString(_dtResp.Rows[0]["Middle_Name"])),
                        new Claim("Last_Name",Convert.ToString( _dtResp.Rows[0]["Last_Name"])),
                        new Claim("Date_Of_Birth", Convert.ToString(_dtResp.Rows[0]["Date_Of_Birth"])),
                        new Claim("Email_ID",Convert.ToString( _dtResp.Rows[0]["Email_ID"])),
                        new Claim("Phone_No", Convert.ToString(_dtResp.Rows[0]["Phone_No"])),
                        new Claim("Gender", Convert.ToString(_dtResp.Rows[0]["Gender"])),
                        new Claim("Address_Line_1", Convert.ToString(_dtResp.Rows[0]["Address_Line_1"])),
                        new Claim("Address_Line_2", Convert.ToString(_dtResp.Rows[0]["Address_Line_2"])),
                        new Claim("Address_Type", Convert.ToString(_dtResp.Rows[0]["Address_Type"])),
                        new Claim("State_Ref_ID",  Convert.ToString(_dtResp.Rows[0]["State_Ref_ID"])),
                        new Claim("District_Ref_ID", Convert.ToString(_dtResp.Rows[0]["District_Ref_ID"])),
                         new Claim("District_Name", Convert.ToString(_dtResp.Rows[0]["District_Name"])),
                          new Claim("Tehsil_Name", Convert.ToString(_dtResp.Rows[0]["Tehsil_Name"])),
                        new Claim("Tehsil_Ref_ID",  Convert.ToString(_dtResp.Rows[0]["Tehsil_Ref_ID"])),
                        new Claim("Village_ID", Convert.ToString(_dtResp.Rows[0]["Village_ID"])),
                        new Claim("Village_Name", Convert.ToString(_dtResp.Rows[0]["Village_Name"])),
                        new Claim("Municipality_ID",  Convert.ToString(_dtResp.Rows[0]["Municipality_ID"])),
                        new Claim("Municipality_Name",  Convert.ToString(_dtResp.Rows[0]["Municipality_Name"])),
                        new Claim("Is_Last_Correspondence",  Convert.ToString(_dtResp.Rows[0]["Is_Last_Correspondence"])),
                        new Claim("Is_Permanent",  Convert.ToString(_dtResp.Rows[0]["Is_Permanent"])),
                        new Claim("Pincode", Convert.ToString(_dtResp.Rows[0]["Pincode"])),
                        new Claim("Profile_Type", Convert.ToString(_dtResp.Rows[0]["Profile_Type"])),
                        new Claim("User_Type", "Citizen"),
                        new Claim("System_Type", Convert.ToString(_dtResp.Rows[0]["System_Type"])),
                        new Claim(ClaimTypes.Role,  "Citizen"),
                        new Claim("Aadhaar_Ref_ID", Convert.ToString(_dtResp.Rows[0]["Aadhaar_Ref_ID"])),
                    };

                    _objResponse.sys_message = this.GenerateToken(claims);

                    _objResponse.response = 1;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(_dtResp);


                    _ActorLogSessionModel.Login_ID = Convert.ToInt64(_dtResp.Rows[0]["Login_ID"]);
                    _ActorLogSessionModel.Token = Convert.ToString(_objResponse.sys_message);

                    InsertLogSession(_ActorLogSessionModel, HttpContext, "Citizen");
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "Login Service Unavailable. Try again later.";
            }
            
            return _objResponse;
        }

        #endregion

        #region Get Citizen By {citizen_id}       
        public DataTable GetCitizen(int? citizen_id)
        {
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("citizen_id", Convert.ToInt32(citizen_id)));

            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("APP_FETCH_CITIZEN", param);
            return dtresp;
        }
        #endregion

        #region LogSession
        private void InsertLogSession(ActorLogSessionModel alsm, HttpContext httpContext, string Type)
        {
            try
            {
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("Token", Convert.ToString(alsm.Token)));
                param.Add(new SqlParameter("IP", Convert.ToString(GetUser_IP())));
                param.Add(new SqlParameter("User_Agent", Convert.ToString(httpContext.Request.Host + httpContext.Request.Path)));
                param.Add(new SqlParameter("Login_ID", Convert.ToInt64(alsm.Login_ID)));

                if (Type == "Actor")
                {
                    _MSSQLGateway.ExecuteProcedureWithDataset("AUTH_ACTOR_INSERT_LOGIN_SESSIONS", param);
                }
                else if (Type == "Citizen")
                {
                    _MSSQLGateway.ExecuteProcedureWithDataset("AUTH_CITIZEN_INSERT_LOGIN_SESSIONS", param);
                }
                else
                {
                    _MSSQLGateway.ExecuteProcedureWithDataset("AUTH_DEPARTMENT_INSERT_LOGIN_SESSIONS", param);
                }
            }
            catch (Exception ex) { }
        }
        #endregion

        #region Generate Token Function 
        public string GenerateToken(List<Claim> claims)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["JWTSetting:Key"]));
            SigningCredentials signInCred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: this._configuration["JWTSetting:Issuer"],
                audience: this._configuration["JWTSetting:Audience"],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(this._configuration["JWTSetting:ExpiryInMins"])),
                claims: claims,
                signingCredentials: signInCred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        #region UserIP
        private string GetUser_IP()
        {
            string remoteIpAddress = null;
            remoteIpAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                remoteIpAddress = Request.Headers["X-Forwarded-For"];
            return remoteIpAddress;
        }
        #endregion

        #region GenerateRandomPassword
        public static string RandomPassword(int length)
        {
            string allowed = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(allowed
                .OrderBy(o => Guid.NewGuid())
                .Take(length)
                .ToArray());
        }

        #endregion

        #region UpdateAaddharRefId
        
        [HttpPut("UpdateAaddharRefId/{Aadhaar_Ref_Id}")]
        [Authorize]
        public IActionResult UpdateAadharRefId(long Aadhaar_Ref_Id)
        {
            ServiceResponseModel _objResponse = new ServiceResponseModel();
            long Citizen_Id =Convert.ToInt64(HttpContext.User.Claims.First(x => x.Type == "Citizen_Ref_ID").Value);
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("Citizen_ID", Citizen_Id));
            param.Add(new SqlParameter("Aadhaar_Ref_Id", Convert.ToString(Aadhaar_Ref_Id)));
            DataTable dtresponse = _MSSQLGateway.ExecuteProcedure("UpdateCitizenAadharRefId", param);
            if (_objHelper.checkDBResponse(dtresponse))
            {
                if (Convert.ToInt64(dtresponse.Rows[0]["response"]) == 1)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresponse.Rows[0]["response"]));
                    _objResponse.data ="OK";
                    _objResponse.sys_message = Convert.ToString(dtresponse.Rows[0]["message"].ToString());
                     return Ok(_objResponse);
                }
                else
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresponse.Rows[0]["response"]));
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresponse);
                    _objResponse.sys_message = Convert.ToString(dtresponse.Rows[0]["message"].ToString());
                    return BadRequest(_objResponse);
                }
            }
            else
            {
                return StatusCode(503);
            }
        }
          
        #endregion
    }

}
