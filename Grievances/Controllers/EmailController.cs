using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Security.Claims;
using EnterpriseSupportLibrary;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace EmailService.Controllers
{
    //[ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/email")]
    [Authorize]
    public class EmailController : Controller
    {
        #region Controller Properties
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;

        // Sendgrid Variables
        private SendGridClient _sendGrid;
        private EmailAddress _fromEmail;
        List<SqlParameter> param = new List<SqlParameter>();
        Int32 Application_ID = 0;
        public string EmailServerKey = string.Empty;
        public string Emailurl = string.Empty;
        #endregion

        public EmailController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection"));

            // Sendgrid Initialization
            this._sendGrid = new SendGridClient("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");
            this._fromEmail = new EmailAddress("grievances.pb@punjab.gov.in", "Punjab Enterprise Architecture");
            this.EmailServerKey = this._configuration["EmailServerKey"]; ;
            this.Emailurl = this._configuration["Emailurl"];

        }
        
        #region Send Multiple Email
        [HttpPost("send-emails")]
      //  [Authorize(Roles = "Department")]
        public async Task<ServiceResponseModel> SendMultipleEmail([FromBody]Emails Emails)
        {
            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }

            // Add parameter of eMail
            List<SqlParameter> param = new List<SqlParameter>();

            // Fetch Template
            string emailTemplate;
            using (StreamReader streamReader = new StreamReader(_env.ContentRootPath + "/EmailTemplates/BasicTemplate.html"))

            {
                emailTemplate = streamReader.ReadToEnd();
            }

            // Replace Variable Data In Template 
            emailTemplate = emailTemplate.Replace("{{body}}", Emails.body);


            for (int i = 0; i < Emails.emails.Count; i++)
            {
                param.Add(new SqlParameter("Email", Emails.emails[i].emailId));
                param.Add(new SqlParameter("Email_Sub", Emails.emails[i].subject));
                param.Add(new SqlParameter("Email_Msg", Emails.body));

                try
                {
                    // Send eMail
                    EmailAddress toAddress = new EmailAddress(Emails.emails[i].emailId,"");
                    SendGridMessage msg = MailHelper.CreateSingleEmail(_fromEmail, toAddress, Emails.emails[i].subject, "", emailTemplate);
                    var abc = await _sendGrid.SendEmailAsync(msg);

                    // Add status of eMail
                    param.Add(new SqlParameter("Email_Status", "Sent"));
                }
                catch (Exception ex)
                {
                    // Add status of eMail
                    param.Add(new SqlParameter("Email_Status", ex.Message));
                }
               // _objResponse = LogEmailRequest(param);

                param.Clear();

                _objResponse.response = 1;
                _objResponse.sys_message = "Email has been send successfuly.";
            }
            return _objResponse;
        }
        #endregion

        #region Log Email Request
        private ServiceResponseModel LogEmailRequest(List<SqlParameter> sp)
        {
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("MD_INSERT_EMAIL", sp);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "Unable to send email.";
                }
                else
                {
                    _objResponse.response = 1;
                    _objResponse.sys_message = "Email has been sent successfully.";
                }
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "Record not saved!";
            }
            return _objResponse;
        }
        #endregion

       

        public async Task getApplicationIDAsync()
        {
            try
            {
                var context = HttpContext; //Current
                string EA_Token = Convert.ToString(context.Request.Headers["Authorization"]);
                var handler = new JwtSecurityTokenHandler();
                string authHeader = EA_Token;
                authHeader = authHeader.Replace("Bearer ", "");
                var jsonToken = handler.ReadToken(authHeader);
                var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;

                string Access_Token = tokenS.Claims.First(claim => claim.Type == "Access_Token").Value;

                TokenData token = new TokenData();
                token.Token = Access_Token;
                //Serialize model to string
                string clmModelSerialize = JsonConvert.SerializeObject(token);
                ServiceResponseModel resAccessKey = await _objHelper.UserWebRequestWithBody(this._configuration["Orchestration:EmailService_Check-access-token-status"], "POST", clmModelSerialize);
                if (this._objHelper.checkResponseModel(resAccessKey))
                {
                    ServiceResponseModel reponse = (ServiceResponseModel)resAccessKey.data;
                    if (this._objHelper.checkResponseModel(reponse))
                    {
                        if (reponse.response <= 0)
                        {
                            Application_ID = 0;
                        }
                        else
                        {
                            JArray _obj = (JArray)reponse.data;
                            if (this._objHelper.checkJArray(_obj))
                            {
                                Application_ID = Convert.ToInt32(((Newtonsoft.Json.Linq.JValue)_obj[0]["Application_ID"]).Value);
                            }
                            else
                            {
                                Application_ID = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Application_ID = 0;
            }
        }

        

        public  string SendEmail(string recipient_email, string subject, string body, string sender)
        {
            string ret = "";
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.Emailurl);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Server-Key", this.EmailServerKey);
                //httpWebRequest.Headers.Add("Authorization", "Bearer " + accToken.sys_message);
                httpWebRequest.Method = "Post";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{ 'recipient_email':'" + recipient_email + "','subject':'" + subject + "','body':'" + body + "','sender':'" + sender + "'}";
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

    }
}