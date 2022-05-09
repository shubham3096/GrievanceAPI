using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EmailService.Controllers;
using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("Auth")]
    public class AuthController: Controller
    {
        #region Controller Properties
        private ServiceResponseModelRows _objResponseRows = new ServiceResponseModelRows();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        CommonFunctions APICall;
        string GetActor = string.Empty;
        string mdmurl = string.Empty;
        string ssoserverkey = string.Empty;
        string ssoserverURL = string.Empty;
        EmailController sendEmail;

        #endregion

        public AuthController(IConfiguration configuration, IHostingEnvironment env)
        {
            sendEmail = new EmailController(configuration, env);
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;    
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this.GetActor = this._configuration["AppSettings_Dev:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
                this.ssoserverkey = this._configuration["AppSettings_Dev:ssoserverkey"];
                this.ssoserverURL = this._configuration["AppSettings_Dev:ssoserverURL"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.GetActor = this._configuration["AppSettings_Stag:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
                this.ssoserverkey = this._configuration["AppSettings_Stag:ssoserverkey"];
                this.ssoserverURL = this._configuration["AppSettings_Stag:ssoserverURL"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.GetActor = this._configuration["AppSettings_Pro:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
                this.ssoserverkey = this._configuration["AppSettings_Pro:ssoserverkey"];
                this.ssoserverURL = this._configuration["AppSettings_Pro:ssoserverURL"];
            }
        }


        #region Get Getactor
        [HttpPost]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> Auth([FromBody]AuthModel InputModel)
        {

            HttpResponseMessage _ResponseMessage;

            InputModel.password  = APICall.Decrypt(InputModel.password);

            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion


            //List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("SelectBy", InputModel.SelectBy));

            _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/Auth", InputModel);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {
                    _objResponse = model;
                   

                }
                //var ActorID = model.data;
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = model.sys_message;
                return _objResponse;
            }


            return _objResponse;
        }
        #endregion


        #region Get sso
        [HttpPost("Ssoauth")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> Ssoauth([FromBody]SSOAuthModel InputModel)
        {

            HttpResponseMessage _ResponseMessage;

           

            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion


            //List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("SelectBy", InputModel.SelectBy));
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Server-Key", this.ssoserverkey));

            _ResponseMessage = await APICall.PostExternalAPI(this.ssoserverURL + "/auth/employee/validate-login-token", InputModel, header);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            EASSO_Response_Model model = JsonConvert.DeserializeObject<EASSO_Response_Model>(jsonString);
            if (model != null && model.Data != null)
            {
                if(model.Status)
                {
                    EASSO_Employee_Details empDetails = JsonConvert.DeserializeObject<EASSO_Employee_Details>(JsonConvert.SerializeObject(model.Data));

                    EASSO_Token_Email ete = new EASSO_Token_Email();
                    ete.User_name = empDetails.Email;
                    _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/auth/details", ete);

                    var jsonString1 = await _ResponseMessage.Content.ReadAsStringAsync();
                    ServiceResponseModel model1 = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString1);
                    if (model1 != null && model1.data != null)
                    {
                        JArray jArray = new JArray();
                        // Declare JArray for getting data from response model
                        try
                        {
                            jArray = JArray.FromObject(model1.data);
                        }
                        catch(Exception ex)
                        {
                            var cas = ex.Message;
                        }
                        if (this._objHelper.checkJArray(jArray))
                        {
                            _objResponse = model1;
                            #region GENERATE LOGIN TOKEN
                            List<Claim> claims;
                            claims = new List<Claim>
                                    {
                                        new Claim("Actor_Ref_ID", Convert.ToString(jArray[0]["Actor_Ref_ID"])),
                                        new Claim("Name", Convert.ToString(jArray[0]["Name"])),
                                        new Claim("Is_First_Login",Convert.ToString(jArray[0]["Is_First_Login"])),
                                        new Claim("Is_Pass_Exp", Convert.ToString(jArray[0]["Is_Pass_Exp"])),
                                        new Claim("Login_ID",Convert.ToString(jArray[0]["Login_ID"])),
                                        new Claim("Actor_ID", Convert.ToString(jArray[0]["Actor_Ref_ID"])),
                                        new Claim("Admin_Stakeholder_ID", Convert.ToString(jArray[0]["Admin_Stakeholder_ID"])),
                                        new Claim("Admin_Stakeholder_Name", Convert.ToString(jArray[0]["Admin_Stakeholder_Name"])),
                                        new Claim("Stakeholder_ID", Convert.ToString(jArray[0]["Stakeholder_ID"])),
                                        new Claim("Stakeholder_Name", Convert.ToString(jArray[0]["Stakeholder_Name"])),
                                        new Claim("Designation_ID", Convert.ToString(jArray[0]["Designation_ID"])),
                                        new Claim("Designation_Name", Convert.ToString(jArray[0]["Designation_Name"])),
                                        new Claim("Profile_Type", Convert.ToString("Actor")),
                                        new Claim("Employee_ID",  Convert.ToString(jArray[0]["Employee_ID"])),
                                        new Claim("Geographical_Operational_Level",  Convert.ToString(jArray[0]["Geographical_Operational_Level"])),
                                        new Claim("District_ID",  Convert.ToString(jArray[0]["District_ID"])),
                                        new Claim("District",  Convert.ToString(jArray[0]["District"])),
                                        new Claim("Roles", Convert.ToString(jArray[0]["Roles"])),
                                        new Claim("User_Type", Convert.ToString(jArray[0]["User_Type"])),
                                        new Claim("System_Type", Convert.ToString(jArray[0]["System_Type"])),
                                        new Claim("Office_ID",  Convert.ToString(jArray[0]["Office_ID"])),
                                        new Claim("Office_Name", Convert.ToString(jArray[0]["Office_Name"])),
                                        new Claim("Office_Address", Convert.ToString(jArray[0]["Office_Address"])),
                                        new Claim(ClaimTypes.Role,  "Actor"),
                                    };
                            #endregion

                            // Generate token behalf of login user data
                            string userToken = this.GenerateToken(claims);

                            // Set token string on server side by using IDistributedCache for cache
                            //this._distributedCache.SetString("token", userToken);

                            //_ActorLogSessionModel.Login_ID = Convert.ToInt64(_dtResp.Rows[0]["Login_ID"]);
                            //_ActorLogSessionModel.Token = Convert.ToString(userToken);

                            //InsertLogSession(_ActorLogSessionModel, HttpContext, "Actor");

                            _objResponse.response = 1;
                            _objResponse.data = model1.data;
                            _objResponse.sys_message = userToken;

                        }
                        //var ActorID = model.data;
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.sys_message = model1.sys_message;
                        return _objResponse;
                    }


                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = model.Message;
                    return _objResponse;
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = model.Message;
                return _objResponse;
            }


            return _objResponse;
        }
        #endregion

        #region
        [HttpGet("CommonAuth")]
        [AllowAnonymous]
        public ServiceResponseModel CommonAuth()
        {
            List<Claim> claims = new List<Claim>();
            _objResponse.response = 1;
            _objResponse.data = GenerateToken(claims);
            _objResponse.sys_message = "AuthToken";
            return _objResponse;
        }
        #endregion

        #region CallCenter
        [HttpPost("callcenter")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> CallcenterAuthAsync([FromBody] AuthModel InputModel)
        {
            try
            {
                InputModel.password = APICall.Decrypt(InputModel.password, _configuration["Callcenter_Encryption_Key"].ToString(), _configuration["Callcenter_Encryption_IV"].ToString());
                InputModel.password = APICall.Encrypt(InputModel.password);

                return await Auth(InputModel);
            }
            catch
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "Invalid credentials.";

                return _objResponse;
            }
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

        #region AddActorOTPDetails
        [HttpPost("AddActorOTPDetails")]
        [Authorize]
        public async Task<ServiceResponseModel> AddActorOTPDetailsAsync([FromBody] OTPRequestModel model)
        {
            //Access token from httpContext
            var _bearerToken = Request.Headers[HeaderNames.Authorization].ToString();
            model.Email_OTP = Convert.ToInt32(_objHelper.generateOTP(6));
            model.Mobile_OTP = Convert.ToInt32(_objHelper.generateOTP(6));
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", _bearerToken));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/auth/addOTPDetails", HttpMethod.Post, model, header);
            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel responseModel = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if(responseModel.response == 1)
            {
                SMSModel sms = new SMSModel();
                sms.mobileno = Convert.ToString(model.Mobile_No).Trim();
                sms.message = $"{model.Mobile_OTP} is your OTP for PGRS Punjab. Valid for next 5 mins only.";

                _= APICall.SendSMS(sms.mobileno, sms.message, "1407161545348456496");
                _ = sendEmail.SendEmail(model.Email, "Verification OTP", $"{model.Email_OTP} is your OTP for PGRS Punjab. Valid for next 5 mins only.","PGRS ");
            }
            return responseModel;
        }
        #endregion

        #region VerifyOTPDetails
        [HttpPost("VerifyOTPDetails")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> VerifyOTPDetailsAsync([FromBody] VerifyOTPModel model)
        {
            //Access token from httpContext
            var _bearer_token = Request.Headers[HeaderNames.Authorization].ToString();
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", _bearer_token));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/auth/verifyOTP", HttpMethod.Post, model, header);
            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel responseModel = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            return responseModel;
        }
        #endregion

    }
}
