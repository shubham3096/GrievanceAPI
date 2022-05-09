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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Http;
using EmailService.Controllers;
using EmailService.Models;

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("citizendetail")]
    [Authorize]
    public class EACitizenController : Controller
    {


        #region Controller Properties
        private ServiceResponseModelRows _objResponseRows = new ServiceResponseModelRows();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private MSSQLGateway _MSSQLGateway_Aadhaar;
        private IHostingEnvironment _env;
        CommonFunctions APICall;
        private IDistributedCache _distributedCache;
        string GetActor = string.Empty;
        string mdmurl = string.Empty;
        private ActorLogSessionModel _ActorLogSessionModel = new ActorLogSessionModel();
        List<SqlParameter> param = new List<SqlParameter>();
        EmailController sendEmail;
        string SendEmailEndpoint = string.Empty;
        Emails _emailobjList = new Emails();

        private string SMSOTP = string.Empty; 
        private string SMSOTPTemplateId = string.Empty;
        string ssoCitizenServerkey = string.Empty;
        string ssoCitizenServerUrl = string.Empty;
        #endregion


        public EACitizenController(IConfiguration configuration, IHostingEnvironment env)
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
            }
        }


        #region Get insertcitizen
        [HttpPost("insertcitizen")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> getactorAsync([FromBody] citizenModel cm)
        {
            ServiceResponseModel _ResponseMessage;


            //_ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/userdetail/insertcitizen/", cm);
            _ResponseMessage = await CitizenRegAsync(cm);

            //var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            //ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (_ResponseMessage.response == 1)
            {
                // Declare JArray for getting data from response model
                //JArray jArray = JArray.FromObject(model.data);
                //if (this._objHelper.checkJArray(jArray))
                //{


                var objRes = (_ResponseMessage.data as IEnumerable<dynamic>).ToList();
                string citizen_Mobile= (objRes[0] as IDictionary<string, string>)["Phone_No"].ToString();
                
                if (cm.Registered_From == "SSO")
                {
                    List<SqlParameter> param1 = new List<SqlParameter>
                {
                    new SqlParameter("Mobile_no",citizen_Mobile)
                };

                    DataTable dtresp = _MSSQLGateway.ExecuteProcedure("get_Citizen_Details_By_MobileNo", param1);
                    if (_objHelper.checkDBResponse(dtresp))
                    {
                        List<SqlParameter> paramet = new List<SqlParameter>
                        {
                        new SqlParameter("User_Name", Convert.ToString(dtresp.Rows[0]["User_Name"])),
                        new SqlParameter("pass", Convert.ToString(dtresp.Rows[0]["User_Password"]))
                        };
                        _objResponse = CitizenFetchDetails(paramet);
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.sys_message = "";
                        return _objResponse;
                    }
                }
                else
                {
                    _objResponse.response = 1;
                    _objResponse.data = _ResponseMessage.data;
                    _objResponse.sys_message = "succuss";
                }


                //}
                //var ActorID = model.data;
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _ResponseMessage.sys_message;
                return _objResponse;
            }


            return _objResponse;
        }
        #endregion

        #region Get citizen otp
        [HttpPost("verify-login-access")]
        [AllowAnonymous]
        public ServiceResponseModel verifyloginaccess([FromBody] citizenOTPModel com)
        {
            ServiceResponseModel _ResponseMessage;


            _ResponseMessage = VerifyLoginAccess(com);

            //var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            //ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (_ResponseMessage.response == 1)
            {
                // Declare JArray for getting data from response model
                //JArray jArray = JArray.FromObject(model.data);
                //if (this._objHelper.checkJArray(jArray))
                //{

                _objResponse.response = 1;
                _objResponse.data = _ResponseMessage.data;
                _objResponse.sys_message = "succuss";


                //}
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

        #region Generate Citizen OTP By Using GenerateCitizenOTP Model
        [HttpPost("generatecitizenOTP")]
        //[Authorize(Roles = "Department, Citizen, Super Admin")]
        [AllowAnonymous]
        public ServiceResponseModel GenerateCitizenOTP([FromBody] GenerateCitizenOTP clm)
        {
            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                return _objResponse;
            }
            var context = HttpContext; //Current
            //string EA_Token = Convert.ToString(context.Request.Headers["Authorization"]);
            //if (!string.IsNullOrEmpty(EA_Token))
            //{
                // HTTP REQUEST FOR GETTING CITIZEN DATA
                DataTable _resCitizen = GetCitizenMobile(clm.User_Name, clm.Mobile);
                if (this._objHelper.checkDBNullResponse(_resCitizen))
                {
                    DataTable _dtResp = new DataTable();
                    // Add new columns in respnse model
                    _dtResp.Columns.Add("Citizen_ID");
                    _dtResp.Columns.Add("OTP");

                    //ServiceResponseModel _rmCitizen = (ServiceResponseModel)_resCitizen.data;
                    //if (this._objHelper.checkResponseModel(_rmCitizen))
                    //{
                    // Declare JArray for getting data from response model
                    //JArray _obj = (JArray)_rmCitizen.data;
                    //if (this._objHelper.checkJArray(_obj))
                    //{
                    if (Convert.ToString(_resCitizen.Rows[0]["response"]) == "1")
                    {
                        DataRow toInsert = _dtResp.NewRow();
                        // insert in the desired place
                        _dtResp.Rows.InsertAt(toInsert, 0);

                        _dtResp.Rows[0]["Citizen_ID"] = Convert.ToString(_resCitizen.Rows[0]["Citizen_ID"]);
                        _dtResp.Rows[0]["OTP"] = Convert.ToString(_objHelper.generateOTP(6));

                        List<SqlParameter> param = new List<SqlParameter>();
                        param.Add(new SqlParameter("User_Name", clm.User_Name));
                        param.Add(new SqlParameter("OTP", Convert.ToString(_dtResp.Rows[0]["OTP"])));

                        DataTable dtsd = _MSSQLGateway.ExecuteProcedure("MD_UPDATE_CITIZEN_OTP", param);
                        if (this._objHelper.checkDBResponse(dtsd))
                        {
                            SMSModel sms = new SMSModel();
                            sms.mobileno = Convert.ToString(clm.Mobile).Trim();
                        //    sms.message = "OTP for your request: " + Convert.ToString(_dtResp.Rows[0]["OTP"]);
                        
                        sms.message = SMSOTP.Replace("[OTP]", Convert.ToString(_dtResp.Rows[0]["OTP"]));
                        //Serialize model to string
                        string clmModelSerialize = JsonConvert.SerializeObject(sms);

                            #region HTTP REQUEST FOR SEND MESSAGE
                            APICall.SendSMS(sms.mobileno, sms.message, SMSOTPTemplateId);
                            #endregion

                            _objResponse.response = 1;
                            _objResponse.data = _objHelper.ConvertTableToDictionary(_dtResp);
                            _objResponse.sys_message = "OTP sent successfully.";
                        }
                        else
                        {
                            _objResponse.response = 0;
                            _objResponse.sys_message = "OTP has been not generated.";
                        }
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.sys_message = Convert.ToString(_dtResp.Rows[0]["message"]);
                    }
                    //}
                    //else
                    //{
                    //    _objResponse.response = 0;
                    //    _objResponse.sys_message = _rmCitizen.sys_message;
                    //}

                }
            else
            {
                _objResponse.response = 0;
                _objResponse.data = null;
                _objResponse.sys_message = "This username/ mobile number is not registered with PGRS";
            }
            //}
            return _objResponse;
        }
        #endregion

        #region Get Citizen By {email} and {mobile}        
        public DataTable GetCitizenMobile(string email_id, string mobile)
        {
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("Email_ID", Convert.ToString(email_id)));
            param.Add(new SqlParameter("Phone_No", Convert.ToString(mobile)));

            DataTable dataTable = _MSSQLGateway.ExecuteProcedure("APP_FETCH_USER_BY_EMAIL_AND_PHONE_NO", param);
            return dataTable;
        }
        #endregion



        #region Get citizen resetpassword
        [HttpPost("changepassword")]
        [AllowAnonymous]
        public ServiceResponseModel changepassword([FromBody] changepasswordModel com)
        {
            ServiceResponseModel _ResponseMessage;


            _ResponseMessage = ChangePasswordwithotp(com);

            //var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            //ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);

            if (_ResponseMessage.response == 1)
            {
                // Declare JArray for getting data from response model


                _objResponse.response = 1;
                _objResponse.data = null;
                _objResponse.sys_message = "succuss";



                //var ActorID = model.data;
            }
            else
            {

                _objResponse.response = 0;
                // _objResponse.data = jArray;
                _objResponse.sys_message = _ResponseMessage.sys_message;
                return _objResponse;
            }


            return _objResponse;
        }
        #endregion

        #region Change Password with otp

        public ServiceResponseModel ChangePasswordwithotp(changepasswordModel ccpm)
        {

            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                return _objResponse;
            }

            param.Add(new SqlParameter("New_Password", _objHelper.ConvertToSHA512(ccpm.NewPassword)));
            //param.Add(new SqlParameter("Old_Password", _objHelper.ConvertToSHA512(ccpm.OldPassword)));
            param.Add(new SqlParameter("Citizen_ID", ccpm.Citizen_ID));
            param.Add(new SqlParameter("OTP", ccpm.OTP));

            DataTable dts = _MSSQLGateway.ExecuteProcedure("MD_UPDATE_CITIZEN_PASSWORD", param);
            _objResponse.response = Convert.ToInt16(dts.Rows[0]["response"]);
            _objResponse.sys_message = Convert.ToString(dts.Rows[0]["message"]);
            return _objResponse;
        }
        #endregion



        #region aadhaar card auth otp
        [HttpPost("aadhaarcardauthotp")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> Aadhaarcardauthotp([FromBody]AadharAuth InputModel)
        {


            AadharAuth _AadharAuth = new AadharAuth();
            _AadharAuth.Aadhaar = InputModel.Aadhaar;

            //Calling CreateSOAPWebRequest method    
            HttpWebRequest request = CreateSOAPWebRequest();

            XmlDocument SOAPReqBody = new XmlDocument();
            //SOAP Body Request    
            SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>  
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-   instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">  
             <soap:Body>  
                <OTPRequest xmlns=""http://tempuri.org/"">  
                  <Aadhaar>" + InputModel.Aadhaar + @"</Aadhaar> 
                </OTPRequest>  
              </soap:Body>  
            </soap:Envelope>");


            using (Stream stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }
            //Geting response from request    

            using (WebResponse Serviceres = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                {
                    //reading stream  
                    //string myXML = "<?xml version=\"1.0\" encoding=\"utf-16\"?><myDataz xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><listS><sog><field1>123</field1><field2>a</field2><field3>b</field3></sog><sog><field1>456</field1><field2>c</field2><field3>d</field3></sog></listS></myDataz>";
                    string ServiceResult = rd.ReadToEnd();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ServiceResult);
                    XmlNodeList parentNode = xmlDoc.GetElementsByTagName("OTPRequestResult");
                    // string xpath = "soap:Envelope/soap:Body/OTPRequestResponse";
                    //  var nodes = xmlDoc.SelectNodes(xpath);

                    foreach (XmlNode childrenNode in parentNode)
                    {

                        var parentnode = ((System.Xml.XmlElement)childrenNode).InnerText;
                        XmlDocument xmlDoc1 = new XmlDocument();
                        xmlDoc1.LoadXml(parentnode);
                        XmlNodeList childnode = xmlDoc1.GetElementsByTagName("root");
                        foreach (XmlNode childrenNodeindder in childnode)
                        {
                            _AadharAuth.OTPRequestResponse = ((System.Xml.XmlElement)childrenNodeindder.SelectSingleNode("RespMsg")).InnerText;
                            _AadharAuth.rescode = ((System.Xml.XmlElement)childrenNodeindder.SelectSingleNode("RespCode")).InnerText;
                        }

                    }
                    if (_AadharAuth.rescode == "1")
                    {
                        _objResponse.response = 1;
                        _objResponse.data = _AadharAuth;
                        _objResponse.sys_message = "succuss";
                    }
                    else
                    {
                        _objResponse.response = 0;
                        _objResponse.data = null;
                        _objResponse.sys_message = _AadharAuth.OTPRequestResponse;

                    }
                }
            }
            return _objResponse;
        }

        public HttpWebRequest CreateSOAPWebRequest()
        {
            //Making Web Request    
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(@"http://52.172.179.160:82/Auth_Other.asmx");
            //SOAPAction    
            Req.Headers.Add(@"SOAPAction:http://tempuri.org/OTPRequest");
            //Content_type    
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            //HTTP method    
            Req.Method = "POST";
            //return HttpWebRequest    
            return Req;
        }
        #endregion
        #region aadhaar card auth otp
        [HttpPost("aadhaarcardauthauth")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> Aadhaarcardauthauth([FromBody]AadharcardAuth InputModel)
        {


            AadharcardAuth _AadharAuth = new AadharcardAuth();
            _AadharAuth.Aadhaar = InputModel.Aadhaar;

            //Calling CreateSOAPWebRequest method    
            HttpWebRequest request = CreateSOAPWebRequestauth();

            XmlDocument SOAPReqBody = new XmlDocument();
            //SOAP Body Request    
            SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>  
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-   instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">  
             <soap:Body>  
                <OTPBasedAuth xmlns=""http://tempuri.org/"">  
                  <Aadhaar>" + InputModel.Aadhaar + @"</Aadhaar>
                  <OTP>" + InputModel.OTP + @"</OTP>
                   <txn>" + InputModel.txn + @"</txn>
                </OTPBasedAuth>  
              </soap:Body>  
            </soap:Envelope>");


            using (Stream stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }
            //Geting response from request    
            using (WebResponse Serviceres = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                {
                    //reading stream  
                    //string myXML = "<?xml version=\"1.0\" encoding=\"utf-16\"?><myDataz xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><listS><sog><field1>123</field1><field2>a</field2><field3>b</field3></sog><sog><field1>456</field1><field2>c</field2><field3>d</field3></sog></listS></myDataz>";
                    string ServiceResult = rd.ReadToEnd();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ServiceResult);
                    XmlNodeList parentNode = xmlDoc.GetElementsByTagName("OTPBasedAuthResponse");
                    // string xpath = "soap:Envelope/soap:Body/OTPRequestResponse";
                    //  var nodes = xmlDoc.SelectNodes(xpath);

                    foreach (XmlNode childrenNode in parentNode)
                    {
                        var parentnode = ((System.Xml.XmlElement)childrenNode).InnerText;
                        XmlDocument xmlDoc1 = new XmlDocument();
                        xmlDoc1.LoadXml(parentnode);
                        XmlNodeList childnode = xmlDoc1.GetElementsByTagName("root");
                        foreach (XmlNode childrenNodeindder in childnode)
                        {
                            _AadharAuth.OTPAuthResponse = ((System.Xml.XmlElement)childrenNodeindder.SelectSingleNode("RespMsg")).InnerText;
                            _AadharAuth.rescode = ((System.Xml.XmlElement)childrenNodeindder.SelectSingleNode("RespCode")).InnerText;
                        }
                    }

                    if (_AadharAuth.rescode == "1")
                    {
                        List<SqlParameter> Parameters = new List<SqlParameter>();
                        Parameters.Add(new SqlParameter("aadhaarnumber", APICall.EncryptUID(_AadharAuth.Aadhaar)));


                        DataTable dtresp = _MSSQLGateway_Aadhaar.ExecuteProcedure("INSERT_CITIZEN_AADHAAR", Parameters);
                        if (_objHelper.checkDBResponse(dtresp))
                        {
                            if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                            {
                                _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                                _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]); ;
                            }
                            else
                            {
                                _AadharAuth.aadhaarrefid = Convert.ToInt64(dtresp.Rows[0]["aadhaar_ref_id"]);
                                _objResponse.response = 1;
                                _objResponse.data = _AadharAuth;
                                _objResponse.sys_message = "succuss";


                            }
                        }
                    }
                    else
                    {

                        _objResponse.response = 0;
                        _objResponse.data = null;
                        _objResponse.sys_message = _AadharAuth.OTPAuthResponse;
                    }

                }
            }
            return _objResponse;
        }

        public HttpWebRequest CreateSOAPWebRequestauth()
        {
            //Making Web Request    
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(@"http://52.172.179.160:82/Auth_Other.asmx");
            //SOAPAction    
            Req.Headers.Add(@"SOAPAction:http://tempuri.org/OTPBasedAuth");
            //Content_type    
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            //HTTP method    
            Req.Method = "POST";
            //return HttpWebRequest    
            return Req;
        }
        #endregion
        #region aadhaar card auth demo
        [HttpPost("aadhaarcarddemograph")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> Aadhaarcarddemograph([FromBody]AadharcardAuthDemo InputModel)
        {


            AadharcardAuth _AadharAuth = new AadharcardAuth();
            _AadharAuth.Aadhaar = InputModel.Aadhaar;

            //Calling CreateSOAPWebRequest method    
            HttpWebRequest request = CreateSOAPWebRequestauthdemo();

            XmlDocument SOAPReqBody = new XmlDocument();
            //SOAP Body Request    
            SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>  
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-   instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">  
             <soap:Body>  
                <Demographic xmlns=""http://tempuri.org/"">  
                  <Aadhaar>" + InputModel.Aadhaar + @"</Aadhaar>
                  <Name>" + InputModel.Name + @"</Name>
                </Demographic>  
              </soap:Body>  
            </soap:Envelope>");


            using (Stream stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }
            //Geting response from request
            try
            {
                using (WebResponse Serviceres = request.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                    {
                        //reading stream  
                        //string myXML = "<?xml version=\"1.0\" encoding=\"utf-16\"?><myDataz xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><listS><sog><field1>123</field1><field2>a</field2><field3>b</field3></sog><sog><field1>456</field1><field2>c</field2><field3>d</field3></sog></listS></myDataz>";
                        string ServiceResult = rd.ReadToEnd();
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(ServiceResult);
                        XmlNodeList parentNode = xmlDoc.GetElementsByTagName("DemographicResult");
                        // string xpath = "soap:Envelope/soap:Body/OTPRequestResponse";
                        //  var nodes = xmlDoc.SelectNodes(xpath);

                        foreach (XmlNode childrenNode in parentNode)
                        {
                            var parentnode = ((System.Xml.XmlElement)childrenNode).InnerText;
                            XmlDocument xmlDoc1 = new XmlDocument();
                            xmlDoc1.LoadXml(parentnode);
                            XmlNodeList childnode = xmlDoc1.GetElementsByTagName("root");
                            foreach (XmlNode childrenNodeindder in childnode)
                            {
                                _AadharAuth.OTPAuthResponse = ((System.Xml.XmlElement)childrenNodeindder.SelectSingleNode("RespMsg")).InnerText;
                                _AadharAuth.rescode = ((System.Xml.XmlElement)childrenNodeindder.SelectSingleNode("RespCode")).InnerText;
                            }
                        }

                        if (_AadharAuth.rescode == "1")
                        {
                            List<SqlParameter> Parameters = new List<SqlParameter>();
                            Parameters.Add(new SqlParameter("aadhaarnumber", APICall.EncryptUID(_AadharAuth.Aadhaar)));


                            DataTable dtresp = _MSSQLGateway_Aadhaar.ExecuteProcedure("INSERT_CITIZEN_AADHAAR", Parameters);
                            if (_objHelper.checkDBResponse(dtresp))
                            {
                                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                                {
                                    _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]); ;
                                }
                                else
                                {
                                    _AadharAuth.aadhaarrefid = Convert.ToInt64(dtresp.Rows[0]["aadhaar_ref_id"]);
                                    _objResponse.response = 1;
                                    _objResponse.data = _AadharAuth;
                                    _objResponse.sys_message = "succuss";


                                }
                            }
                        }
                        else
                        {

                            _objResponse.response = 0;
                            _objResponse.data = null;
                            _objResponse.sys_message = _AadharAuth.OTPAuthResponse;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                _objResponse.response = 0;
                _objResponse.data = null;
                _objResponse.sys_message = e.Message;
            }
            return _objResponse;
        }

        public HttpWebRequest CreateSOAPWebRequestauthdemo()
        {
            //Making Web Request    
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(@"http://52.172.179.160:82/Auth_Other.asmx");
            //SOAPAction    
            Req.Headers.Add(@"SOAPAction:http://tempuri.org/Demographic");
            //Content_type    
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            //HTTP method    
            Req.Method = "POST";
            //return HttpWebRequest    
            return Req;
        }
        #endregion


        //EA Imported API of citizen
        //====================================================== Citizen ======================================================

        #region Insert Citizen By Using Citizen Model

        private async Task<ServiceResponseModel> CitizenRegAsync(citizenModel cm)
        {

            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                return _objResponse;
            }
            //await getApplicationIDAsync();
            //ServiceResponseModel _getAccessToken = await GetAccessToken(acc);
            //if (_getAccessToken.response > 0)
            //{
            param.Add(new SqlParameter("First_Name", Convert.ToString(cm.First_Name)));
            param.Add(new SqlParameter("Middle_Name", Convert.ToString(cm.Middle_Name)));
            param.Add(new SqlParameter("Last_Name", Convert.ToString(cm.Last_Name)));
            param.Add(new SqlParameter("Father_Name", Convert.ToString(cm.Father_Name)));
            param.Add(new SqlParameter("Mother_Name", Convert.ToString(cm.Mother_Name)));
            param.Add(new SqlParameter("Email_ID", Convert.ToString(cm.Email_ID)));
            param.Add(new SqlParameter("Phone_No", Convert.ToString(cm.Phone_No)));
            param.Add(new SqlParameter("Date_Of_Birth", Convert.ToDateTime(cm.Date_Of_Birth).ToString("yyyy-MM-dd")));
            param.Add(new SqlParameter("Gender", Convert.ToString(cm.Gender)));
            param.Add(new SqlParameter("Profile_Type", Convert.ToString("Citizen")));
            param.Add(new SqlParameter("Address_Type", Convert.ToString(cm.Address_Type)));
            param.Add(new SqlParameter("Address_Line_1", Convert.ToString(cm.Address_Line_1)));
            param.Add(new SqlParameter("Address_Line_2", Convert.ToString(cm.Address_Line_2)));
            param.Add(new SqlParameter("State_Ref_ID", "3"));
            param.Add(new SqlParameter("District_Ref_ID", Convert.ToString(cm.District_Ref_ID)));
            param.Add(new SqlParameter("Tehsil_Ref_ID", Convert.ToInt64(cm.Tehsil_Ref_ID)));
            param.Add(new SqlParameter("Village_ID", Convert.ToInt64(cm.Village_ID)));
            param.Add(new SqlParameter("Village_Name", Convert.ToString(cm.Village_Name)));
            param.Add(new SqlParameter("Municipality_ID", Convert.ToInt64(cm.Municipality_ID)));
            param.Add(new SqlParameter("Pincode", Convert.ToString(cm.Pincode)));
            param.Add(new SqlParameter("Aadhaar_Ref_ID", Convert.ToString(cm.Aadhaar_Ref_ID)));
            param.Add(new SqlParameter("Created_By", 0));
            param.Add(new SqlParameter("Registered_From", cm.Registered_From));

            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("MD_INSERT_CITIZENS", param);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                }
                else
                {
                    _objResponse.response = 1;
                    string generateOTP = _objHelper.generateOTP(6);

                    CitizenLoginModel clm = new CitizenLoginModel();
                    clm.User_Name = Convert.ToString(cm.Phone_No).Trim().ToLower();
                    clm.User_Password = Convert.ToString(_objHelper.ConvertToSHA512(cm.Password));
                    clm.Citizen_Ref_ID = Convert.ToInt32(dtresp.Rows[0]["Citizen_ID"]);
                    clm.OTP = generateOTP;
                    clm.OTP_Verified_From = cm.Registered_From;

                    var context = HttpContext; //Current
                                               //string EA_Token = Convert.ToString(context.Request.Headers["Authorization"]);

                    //Serialize model to string
                    string clmModelSerialize = JsonConvert.SerializeObject(clm);
                    // Http request for insert citizen login
                    //ServiceResponseModel resCitizen = await _objHelper.UserWebRequestWithBody(this._configuration["Orchestration:MasterDataManagementService_insertcitizenlogin"], "POST", $"{EA_Token}", clmModelSerialize);
                    //if (this._objHelper.checkResponseModel(resCitizen))
                    //{
                    //    ServiceResponseModel rmCitizen = (ServiceResponseModel)resCitizen.data;
                    //    if (this._objHelper.checkResponseModel(rmCitizen))
                    //    {
                    //        // Declare JArray for getting data from response model
                    //        JArray objCitizen = (JArray)rmCitizen.data;
                    //        if (this._objHelper.checkJArray(objCitizen))
                    //        {
                    //            if (Convert.ToInt32(Convert.ToString(objCitizen[0]["response"])) <= 0)
                    //            {
                    //                _objResponse.response = Convert.ToInt32(Convert.ToString(objCitizen[0]["response"]));
                    //                _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                    //            }
                    //            else
                    //            {

                    //            }
                    //        }
                    //    }
                    //}
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);



                    SMSModel sms = new SMSModel();
                    sms.mobileno = Convert.ToString(cm.Phone_No).Trim();
                  //  sms.message = "Please verify your login access, OTP for your request: " + generateOTP;
                    sms.message = SMSOTP.Replace("[OTP]", Convert.ToString(generateOTP));
                    if(cm.Registered_From != "SSO")
                    {
                        APICall.SendSMS(sms.mobileno, sms.message, SMSOTPTemplateId);
                    }
                    
                    List<EmailData> _EmailData = new List<EmailData>();
                    EmailData _ed = new EmailData();
                    _ed.emailId = cm.Email_ID;
                    _ed.subject = "Citizen OTP";
                    _EmailData.Add(_ed);

                    _emailobjList.body = sms.message;
                    _emailobjList.emails = _EmailData;
                    if (cm.Registered_From != "SSO")
                    {
                        ServiceResponseModel _ServiceResponseModel = await sendEmail.SendMultipleEmail(_emailobjList);
                    }



                    //Serialize model to string
                    string smsSerialize = JsonConvert.SerializeObject(sms);
                    InsertCitizenLogin(clm);
                    //#region HTTP REQUEST FOR SEND MESSAGE
                    //await _objHelper.UserWebRequestWithBody(this._configuration["Orchestration:MasterDataManagementService_send_message_on_mobile/v1"], "POST", $"{EA_Token}", smsSerialize);
                    //#endregion

                    _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["response"]);
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);

                }
            }
            //}
            //else
            //{
            //    _objResponse.response = _getAccessToken.response;
            //    _objResponse.sys_message = _getAccessToken.sys_message;
            //}
            return _objResponse;
        }
        #endregion
        public ServiceResponseModel InsertCitizenLogin(CitizenLoginModel clm)
        {
            try
            {
                // Check validation
                if (!ModelState.IsValid)
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                    return _objResponse;
                }
                List<SqlParameter> param1 = new List<SqlParameter>();

                param1.Add(new SqlParameter("User_Name", clm.User_Name));
                param1.Add(new SqlParameter("User_Password", clm.User_Password));
                param1.Add(new SqlParameter("Citizen_Ref_ID", clm.Citizen_Ref_ID));
                param1.Add(new SqlParameter("OTP", clm.OTP));
                if(clm.OTP_Verified_From == "SSO")
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
                _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
            }

            catch (Exception r)
            {
                string ed = r.ToString();
            }

            return _objResponse;

        }

        public ServiceResponseModel VerifyLoginAccess([FromBody] citizenOTPModel clm)
        {
            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                return _objResponse;
            }

            param.Add(new SqlParameter("Citizen_Ref_ID", clm.Citizen_Ref_ID));
            param.Add(new SqlParameter("OTP", clm.OTP));

            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("MD_UPDATE_CITIZEN_OTP_BY_CITIZEN_ID", param);
            _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
            _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);

            return _objResponse;
        }

        #region Citizen Login Method
        [AllowAnonymous]
        [HttpPost("citizenlogin")]
        public ServiceResponseModel CitizenLogin([FromBody] LoginModel login)
        {
            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                return _objResponse;
            }

            param.Add(new SqlParameter("User_Name", login.username));
            param.Add(new SqlParameter("pass", _objHelper.ConvertToSHA512(login.password)));
            //param.Add(new SqlParameter("ip", Request.Headers["X-Forwarded-For"]));
            //param.Add(new SqlParameter("useragent", Request.Headers["User-Agent"]));

            _objResponse = CitizenFetchDetails(param); 

            return _objResponse;
        }

        public ServiceResponseModel CitizenFetchDetails(List<SqlParameter> sqlParameters)
        {
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
                        _dtResp.Columns.Add("eSewa_id");



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
                        _dtResp.Rows[0]["eSewa_id"] = Convert.ToString(_resCitizen.Rows[0]["eSewa_id"]);
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
                        new Claim("Aadhaar_Ref_ID", Convert.ToString(_dtResp.Rows[0]["Aadhaar_Ref_ID"])),
                        new Claim("eSewa_id", Convert.ToString(_dtResp.Rows[0]["eSewa_id"])),

                        new Claim(ClaimTypes.Role,  "Citizen"),
                    };

                    // Generate token behalf of login user data
                    _objResponse.sys_message = this.GenerateToken(claims);

                    // Set token string on server side by using IDistributedCache for cache
                    // this._distributedCache.SetString("token", _objResponse.sys_message);
                    //string a = this._distributedCache.GetString("token");

                    _objResponse.response = 1;
                    // Convert datatable (data) to dictionary
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
        #region Change Password
        [HttpPost("citizen-change-password")]
        //[Authorize(Roles = "Department, Citizen, Super Admin")]
        public ServiceResponseModel ChangePassword([FromBody] CitizenChangePassModel ccpm)
        {
            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                return _objResponse;
            }

            param.Add(new SqlParameter("Old_Password", _objHelper.ConvertToSHA512(ccpm.OldPassword)));
            param.Add(new SqlParameter("New_Password", _objHelper.ConvertToSHA512(ccpm.NewPassword)));
            param.Add(new SqlParameter("Citizen_Ref_ID", ccpm.Citizen_ID));

            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("MD_UPDATE_CITIZEN_CHANGE_PASSWORD", param);
            _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
            _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
            _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
            return _objResponse;
        }
        #endregion
        private string GetUser_IP()
        {
            string remoteIpAddress = null;
            remoteIpAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                remoteIpAddress = Request.Headers["X-Forwarded-For"];
            return remoteIpAddress;
        }

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
        #region Get Citizen By {citizen_id}       
        public DataTable GetCitizen(int? citizen_id)
        {
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("citizen_id", Convert.ToInt32(citizen_id)));

            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("APP_FETCH_CITIZEN", param);
            return dtresp;
        }
        #endregion

       
        #region Profile Get
        [HttpGet("getprofile")]
        [Authorize]
        public DataTable GetProfile()
        {
            string Citizen_ID = HttpContext.User.Claims.First(x => x.Type == "Citizen_Ref_ID").Value;
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("Citizen_ID", Convert.ToString(Citizen_ID)));
            DataTable dataTable = _MSSQLGateway.ExecuteProcedure("APP_FETCH_CITIZEN", param);
            dataTable.Columns.Add("User_Type");
            dataTable.Columns.Add("System_Type");
            dataTable.Rows[0]["User_Type"] = "Citizen";
            dataTable.Rows[0]["System_Type"] = "EA";
            return dataTable;
        }
        #endregion

        #region Profile Update
        [HttpPost("profile")]
        [Authorize]
        public ServiceResponseModel Profile([FromBody] ProfileModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                    return _objResponse;
                }
                string Citizen_ID = HttpContext.User.Claims.First(x => x.Type == "Citizen_Ref_ID").Value;
                param.Add(new SqlParameter("Citizen_ID", Citizen_ID));
                param.Add(new SqlParameter("firstName", model.firstname));
                param.Add(new SqlParameter("middleName", model.middlename));
                param.Add(new SqlParameter("lastName", model.lastname));
                param.Add(new SqlParameter("email", model.email));
                param.Add(new SqlParameter("address", model.address));
                param.Add(new SqlParameter("addressdistrict", Convert.ToInt32(model.addressdistrict)));
                param.Add(new SqlParameter("addresstehsil", Convert.ToInt32(model.addresstehsil)));
                param.Add(new SqlParameter("addressvillage", Convert.ToInt32(model.addressvillage)));
                param.Add(new SqlParameter("addressmunicipality", Convert.ToInt32(model.addressmunicipality)));

                DataTable dts = _MSSQLGateway.ExecuteProcedure(Convert.ToString("MD_UPDATE_CITIZEN_PROFILE"), param);
                _objResponse.response = Convert.ToInt16(dts.Rows[0]["response"]);
                _objResponse.sys_message = Convert.ToString(dts.Rows[0]["message"]);
            }
            catch (Exception ex)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = ex.Message;
            }
            return _objResponse;
        }
        #endregion

        #region CitizenRegister
        [HttpPost("CitizenRegistration")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> CitizenRegistrationAsync([FromBody] CitizenRegisterModel cm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                    return _objResponse;
                }
                param.Add(new SqlParameter("First_Name", Convert.ToString(cm.First_Name)));
                param.Add(new SqlParameter("Middle_Name", Convert.ToString(cm.Middle_Name)));
                param.Add(new SqlParameter("Last_Name", Convert.ToString(cm.Last_Name)));
                param.Add(new SqlParameter("Father_Name", Convert.ToString(cm.Father_Name)));
                param.Add(new SqlParameter("Mother_Name", Convert.ToString(cm.Mother_Name)));
                param.Add(new SqlParameter("Email_ID", Convert.ToString(cm.Email_ID)));
                param.Add(new SqlParameter("Phone_No", Convert.ToString(cm.Phone_No)));
                param.Add(new SqlParameter("Date_Of_Birth", Convert.ToDateTime(cm.Date_Of_Birth)));
                param.Add(new SqlParameter("Gender", Convert.ToString(cm.Gender)));
                param.Add(new SqlParameter("Profile_Type", Convert.ToString("Citizen")));
                param.Add(new SqlParameter("Address_Type", Convert.ToString(cm.Address_Type)));
                param.Add(new SqlParameter("Address_Line_1", Convert.ToString(cm.Address_Line_1)));
                param.Add(new SqlParameter("Address_Line_2", Convert.ToString(cm.Address_Line_2)));
                param.Add(new SqlParameter("State_Ref_ID", "3"));
                param.Add(new SqlParameter("District_Ref_ID", Convert.ToString(cm.District_Ref_ID)));
                param.Add(new SqlParameter("Tehsil_Ref_ID", Convert.ToInt64(cm.Tehsil_Ref_ID)));
                param.Add(new SqlParameter("Village_ID", Convert.ToInt64(cm.Village_ID)));
                param.Add(new SqlParameter("Village_Name", Convert.ToString(cm.Village_Name)));
                param.Add(new SqlParameter("Municipality_ID", Convert.ToInt64(cm.Municipality_ID)));
                param.Add(new SqlParameter("Pincode", Convert.ToString(cm.Pincode)));
                param.Add(new SqlParameter("Aadhaar_Ref_ID", Convert.ToString(cm.Aadhaar_Ref_ID)));
                param.Add(new SqlParameter("Created_By", 0));

                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("MD_INSERT_CITIZENS", param);
                if (_objHelper.checkDBResponse(dtresp))
                {
                    if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) <= 0)
                    {
                        _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                        _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"]);
                    }
                    else
                    {
                        _objResponse.response = 1;
                        string generateOTP = _objHelper.generateOTP(6);

                        CitizenLoginModel clm = new CitizenLoginModel();
                        clm.User_Name = Convert.ToString(cm.Phone_No).Trim().ToLower();
                        clm.User_Password = Convert.ToString(_objHelper.ConvertToSHA512(cm.Password));
                        clm.Citizen_Ref_ID = Convert.ToInt32(dtresp.Rows[0]["Citizen_ID"]);
                        clm.OTP = generateOTP;

                        SMSModel sms = new SMSModel();
                        sms.mobileno = Convert.ToString(cm.Phone_No).Trim();
                        //  sms.message = "Please verify your login access, OTP for your request: " + generateOTP;
                        sms.message = SMSOTP.Replace("[OTP]", Convert.ToString(generateOTP));

                        APICall.SendSMS(sms.mobileno, sms.message, SMSOTPTemplateId);


                        List<EmailData> _EmailData = new List<EmailData>();

                        EmailData _ed = new EmailData();
                        _ed.emailId = cm.Email_ID;
                        _ed.subject = "Citizen OTP";
                        _EmailData.Add(_ed);

                        _emailobjList.body = sms.message;
                        _emailobjList.emails = _EmailData;
                        ServiceResponseModel _ServiceResponseModel = await sendEmail.SendMultipleEmail(_emailobjList);

                        string smsSerialize = JsonConvert.SerializeObject(sms);
                        InsertCitizenLogin(clm);

                        _objResponse.sys_message ="Registeration Successfully";
                        dtresp.Columns.Add("UserName", typeof(System.String), clm.User_Name);
                        dtresp.Columns.Add("Password", typeof(System.Int32),cm.Password);

                        _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);

                    }
                }

                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "Citizen Registration Failed";
                }
            }
            catch (Exception ex)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = ex.Message;
            }
            return _objResponse;
        }
        #endregion


        #region SSOCitizenLogin
        [HttpPost("Ssoauth")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> Ssoauth([FromBody] SSOAuthModel InputModel)
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

            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Server-Key", this.ssoCitizenServerkey));

            _ResponseMessage = await APICall.PostExternalAPI(this.ssoCitizenServerUrl + "/auth/citizen/validate-login-token", InputModel, header);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            EASSO_Response_Model model = JsonConvert.DeserializeObject<EASSO_Response_Model>(jsonString);
            if (model != null && model.Status)
            {
                EASSO_Employee_Details empDetails = JsonConvert.DeserializeObject<EASSO_Employee_Details>(JsonConvert.SerializeObject(model.Data));
                List<SqlParameter> param1 = new List<SqlParameter>
                {
                    new SqlParameter("Mobile_no", empDetails.Mobile_no)
                };

                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("get_Citizen_Details_By_MobileNo", param1);
                if (_objHelper.checkDBResponse(dtresp))
                {
                    List<SqlParameter> paramet = new List<SqlParameter>
                        {
                        new SqlParameter("User_Name", Convert.ToString(dtresp.Rows[0]["User_Name"])),
                        new SqlParameter("pass", Convert.ToString(dtresp.Rows[0]["User_Password"]))
                        };
                    _objResponse = CitizenFetchDetails(paramet);
                }
                else
                {
                    _objResponse.response = 2;
                    _objResponse.sys_message = "You are not registered with PGRS, Please register on PGRS Portal.";
                    _objResponse.data = empDetails;
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


        #region GetCitizenDetailsByPhoneNumber
        [HttpGet("GetCitizenDetailsByPhoneNumber/{Phone_no}")]
        [AllowAnonymous]
        public ServiceResponseModel GetGrievanceCounts(long Phone_no)
        {
            #region DB CALL & BUSINESS LOGIC
         
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Phone_no", Phone_no.ToString()));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("GetCitizenDetailsByPhoneNumber", Parameters);
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
                _objResponse.response_code = "503";
            }
            #endregion
            return _objResponse;
        }
        #endregion


    }
}

