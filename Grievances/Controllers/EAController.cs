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
    [Route("actor")]
    [Authorize]
    public class EAController : Controller
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
        #endregion


        public EAController(IConfiguration configuration, IHostingEnvironment env)
        {
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this.GetActor = this._configuration["AppSettings_Dev:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.GetActor = this._configuration["AppSettings_Stag:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.GetActor = this._configuration["AppSettings_Pro:GetActor"];
                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
            }


        }


        #region Get Getactor
        [HttpGet("getactor/{actor_id}")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> getactor(int actor_id)
        {
            HttpResponseMessage _ResponseMessage;

            ServiceResponseModel accToken = APICall.GenerateAccessToken();

            _ResponseMessage = await APICall.PostExternalAPI(GetActor + actor_id, accToken.sys_message);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 0;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


                }
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

        #region Get Getactors (array)
        [HttpPost("getactors")]
      [AllowAnonymous]
        public async Task<ServiceResponseModel> getactors([FromBody] List<string> Actor_List)
        {
            HttpResponseMessage _ResponseMessage;

           // ServiceResponseModel accToken = APICall.GenerateAccessToken();

            _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/actor/v1/getactors", Actor_List);
            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 0;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


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


        #region Get Department
        [HttpPost("master")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> master([FromBody]EAapiModel InputModel)
        {
            //HttpResponseMessage _ResponseMessage;

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
            #region DB CALL & BUSINESS LOGIC
           // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("getstakeholders"), Parameters);
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




            //List<SqlParameter> Parameters = new List<SqlParameter>();
            // Parameters.Add(new SqlParameter("SelectBy", InputModel.SelectBy));


            //_ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/master", InputModel);

            //var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            //ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            //if (model != null && model.data != null)
            //{
            //    // Declare JArray for getting data from response model
            //    JArray jArray = JArray.FromObject(model.data);
            //    if (this._objHelper.checkJArray(jArray))
            //    {

            //        _objResponse.response = 1;
            //        _objResponse.data = jArray;
            //        _objResponse.sys_message = "succuss";


            //    }
            //var ActorID = model.data;
            //}
            //else
            //{
            //    _objResponse.response = 0;
            //    _objResponse.sys_message = model.sys_message;
            //    return _objResponse;
            //}


          //  return _objResponse;
        }
        #endregion


        #region GET Offices by dept id

        [HttpPost("getofficesbydeptid")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> getofficesbydeptid([FromBody] getoffice InputModel)
        {
            HttpResponseMessage _ResponseMessage;

            _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/master", InputModel);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


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

        #region Get Districts
        [HttpGet("getsgddistrictbystateid/{state_id}")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> getsgddistrictbystateid(int state_id)
        {
            HttpResponseMessage _ResponseMessage;

            _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/region/v1/getsgddistrictbystateid/" + state_id);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


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


        #region Get Districts
        [HttpGet("getsgddistrict/{dist_id}")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> getsgddistrict(int dist_id)
        {
         //   AadharAuth _AadharAuth = new AadharAuth();
           //_AadharAuth = InvokeService("872401207079");


            HttpResponseMessage _ResponseMessage;

            ServiceResponseModel accToken = APICall.GenerateAccessToken();

            _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/region/v1/getsgddistrict/" + dist_id);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


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

        public HttpWebRequest CreateSOAPWebRequest()
        {
            //Making Web Request    
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(@"http://52.172.45.18:82/Auth_Other.asmx");
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

        public AadharAuth InvokeService(string Aadhaar)
        {
            AadharAuth _AadharAuth = new AadharAuth();
            _AadharAuth.Aadhaar = Aadhaar;

            //Calling CreateSOAPWebRequest method    
            HttpWebRequest request = CreateSOAPWebRequest();

            XmlDocument SOAPReqBody = new XmlDocument();
            //SOAP Body Request    
            SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>  
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-   instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">  
             <soap:Body>  
                <OTPRequest xmlns=""http://tempuri.org/"">  
                  <Aadhaar>" + Aadhaar + @"</Aadhaar> 
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
                    XmlNodeList parentNode = xmlDoc.GetElementsByTagName("OTPRequestResponse");
                    // string xpath = "soap:Envelope/soap:Body/OTPRequestResponse";
                    //  var nodes = xmlDoc.SelectNodes(xpath);

                    foreach (XmlNode childrenNode in parentNode)
                    {
                        _AadharAuth.OTPRequestResponse = ((System.Xml.XmlElement)childrenNode).InnerText;
                    }
                }
            }
            return _AadharAuth;
        }

        #region Get Districts
        [HttpGet("stakeholder/{stake_id}")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> stakeholder(int stake_id)
        {
            HttpResponseMessage _ResponseMessage;

            ServiceResponseModel accToken = APICall.GenerateAccessToken();

            _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/stakeholder/" + stake_id);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


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


        #region password change
        [HttpPost("actor-change-password")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> actorchangepassword([FromBody]ChangepasswordModel InputModel)
        {


            HttpResponseMessage _ResponseMessage;
            String Actor_Ref_ID = _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID");
            InputModel.Actor_id = Actor_Ref_ID;

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

            _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/actorauthlogin/actor-change-password/", InputModel);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


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

        #region Username change
        [HttpPost("actor-change-username")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> actorchangeUsername([FromBody]ChangeusernameModel InputModel)
        {


            HttpResponseMessage _ResponseMessage;
            String Actor_Ref_ID = _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID");
            String Employee_ID = _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Employee_ID");
            InputModel.Actor_ID = Actor_Ref_ID;
            InputModel.Employee_ID = Employee_ID;

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

            ServiceResponseModel accToken = APICall.GenerateAccessToken();

            List<KeyValuePair<string,string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + accToken.sys_message));

            _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/actorauthlogin", HttpMethod.Put, InputModel, header);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = "succuss";


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

        #region Username change
        [HttpPost("check-avial-user")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> checkUsername([FromBody]checkusernameModel InputModel)
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

            _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl + "/actorauthlogin/check-actor-username-availability", InputModel);

            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(jsonString);
            if (model != null && model.data != null)
            {
                // Declare JArray for getting data from response model
                JArray jArray = JArray.FromObject(model.data);
                if (this._objHelper.checkJArray(jArray))
                {

                    _objResponse.response = 1;
                    _objResponse.data = jArray;
                    _objResponse.sys_message = model.sys_message;


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

        #region Get ROLE MASTER
        [HttpGet("getRolesMaster")]
        [Authorize]
        public async Task<ServiceResponseModel> getRolesMaster()
        {
            //HttpResponseMessage _ResponseMessage;

            List<SqlParameter> Parameters = new List<SqlParameter>();
           // Parameters.Add(new SqlParameter("isactive", '1'));
            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("APP_FETCH_ROLES_MASTER"), Parameters);
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

        #region Get ROLES BY ACTOR ID
        [HttpGet("getActorRoles/{actorid}")]
        [Authorize]
        public async Task<ServiceResponseModel> getActorRoles(Int64 actorid)
        {
            //HttpResponseMessage _ResponseMessage;

            List<SqlParameter> Parameters = new List<SqlParameter>();
             Parameters.Add(new SqlParameter("Actor_ID", actorid));
            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("APP_FETCH_ACTOR_ROLES"), Parameters);
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

        #region Remove ASSIGN ROLE TO ACTOR
        [HttpGet("unassignRolefromactor/{mappingid}")]
        [Authorize]
        public async Task<ServiceResponseModel> unassignRolefromactor(Int32 mappingid)
        {
            
            Int32 unassignedby  = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            //HttpResponseMessage _ResponseMessage;
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("mappingid", mappingid));
            Parameters.Add(new SqlParameter("unassignedby", unassignedby));
            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("REMOVE_ROLE_FROM_ACTOR"), Parameters);
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

        #region Add Categories
        [HttpPost("AddCategories")]
        [Authorize]
        public async Task<ServiceResponseModel> AddCategories([FromBody]CategoriesModel InputModel)
        {           
            //HttpResponseMessage _ResponseMessage;
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("Category_Label", InputModel.Category_Label));
            Parameters.Add(new SqlParameter("Stakeholder_ID", InputModel.Stakeholder_ID));
            Parameters.Add(new SqlParameter("Category_Label_ll", InputModel.Category_Label_ll));
            Parameters.Add(new SqlParameter("Parent_ID", InputModel.Parent_ID));
            Parameters.Add(new SqlParameter("Is_Active", InputModel.Is_Active));
            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("ADD_CATEGORIES"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {

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


        #region Get Categories BY STAKEHOLDERID
        [HttpPost("getCategories")]
        [Authorize]
        public async Task<ServiceResponseModel> getCategories([FromBody]getCategoriesModel InputModel)
        {
            //HttpResponseMessage _ResponseMessage;
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();

           
            Parameters.Add(new SqlParameter("Stakeholder_ID", InputModel.Stakeholder_ID));

            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("GET_CATEGORIES"), Parameters);
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

        #region Get Categories BY STAKEHOLDERID
        [HttpPost("getCategorieslist")]
        [Authorize]
        public async Task<ServiceResponseModel> getCategorieslist([FromBody]getCategoriesModellist InputModel)
        {
            //HttpResponseMessage _ResponseMessage;
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();

            
            Parameters.Add(new SqlParameter("Category_ID", InputModel.Category_ID));
           

            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("GET_CATEGORIES_LIST"), Parameters);
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



        #region Edit categories
        [HttpPost("editCategories")]
        [Authorize]
        public async Task<ServiceResponseModel> editCategories([FromBody]editCategoriesModel InputModel)
        {
            //HttpResponseMessage _ResponseMessage;
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();

            Parameters.Add(new SqlParameter("Category_ID", InputModel.Category_ID));
            Parameters.Add(new SqlParameter("Category_Label", InputModel.Category_Label)); 
            Parameters.Add(new SqlParameter("Category_Label_ll", InputModel.Category_Label_ll));
            Parameters.Add(new SqlParameter("Parent_ID", InputModel.Parent_ID));
            Parameters.Add(new SqlParameter("Is_Active", InputModel.Is_Active));

            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("EDIT_CATEGORIES"), Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToString(dtresp.Rows[0]["response"]) == "1")
                {

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

        #region Get Categories BY STAKEHOLDERID
        [HttpPost("getactorbyroleid")]
        [Authorize]
        public async Task<ServiceResponseModel> getactorbyroleid([FromBody]getactorbyrole InputModel)
        {
            //HttpResponseMessage _ResponseMessage;
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();


            Parameters.Add(new SqlParameter("roleid", InputModel.Role_ID));


            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("GET_ACTOR_BY_ROLEID"), Parameters);
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
        #region Get ON BOARDED DEPARTMENT
        [HttpPost("getonboardeddepart")]
        [Authorize]
        public async Task<ServiceResponseModel> getonboardeddepart([FromBody]getCategoriesModel InputModel)
        {
            //HttpResponseMessage _ResponseMessage;
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();


            Parameters.Add(new SqlParameter("stakeholderid", InputModel.Stakeholder_ID));
            Parameters.Add(new SqlParameter("Onboard", InputModel.Onboard));


            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("APP_FETCH_ONBOARDED_DEPT"), Parameters);
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
        #region Get ON Setstatusdept DEPARTMENT
        [HttpPost("Setstatusdept")]
        [Authorize]
        public async Task<ServiceResponseModel> Setstatusdept([FromBody]deptstatusModel InputModel)
        {
            //HttpResponseMessage _ResponseMessage;
            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion

            List<SqlParameter> Parameters = new List<SqlParameter>();


            Parameters.Add(new SqlParameter("stakeholderid", InputModel.Stakeholder_ID));
            Parameters.Add(new SqlParameter("active_force", InputModel.Active_Force));
            Parameters.Add(new SqlParameter("status", InputModel.Status));


            #region DB CALL & BUSINESS LOGIC
            // _objResponse = APICall.GrievanceResponse("getstakeholders", Parameters);
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("APP_ACTIVE_DEPT"), Parameters);
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


        public class Envelope
        {
            public string _string { get; set; }
        }

        [HttpGet("GetImportantLinks/{type}")]
        [AllowAnonymous]
        public ServiceResponseModel GetImportantLinksAsync(string type)
        {
            #region DB CALL & BUSINESS LOGIC
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("type", type));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("Get_Important_Links", parameters);
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

        #region GetServices
        [HttpPost("GetServices")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetServicesAsync([FromBody] GetServiceRequestModel serviceModel)
        {
            ServiceResponseModel accToken = APICall.GenerateAccessToken();
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + accToken.sys_message));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/grievance/getservicelist" , HttpMethod.Post, serviceModel, header);
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
                _objResponse.sys_message ="no data";
                _objResponse.response_code = "200";
                return _objResponse;
            }
            return _objResponse;
        }
        #endregion

        #region GetServices
        [HttpGet("updateserviceclickcount/{serviceId}")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> UpdateServiceCount(string serviceId)
        {
            ServiceResponseModel accToken = APICall.GenerateAccessToken();
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + accToken.sys_message));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/grievance/updateserviceclickcount/" + serviceId, HttpMethod.Get,null, header);
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

        #region GetServiceDocument
        [HttpGet("getServiceDocument/{documentId}")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetServiceDocument(long documentId)
        {
            ServiceResponseModel accToken = APICall.GenerateAccessToken();
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + accToken.sys_message));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/documentupload/getdocument/" + documentId, HttpMethod.Get, null, header);
            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();
            
            ServiceResponseModel model = JsonConvert.DeserializeObject<ServiceResponseModel>(JsonConvert.DeserializeObject(jsonString).ToString());
            if (model != null && model.data != null)
            {
                    _objResponse.response = 1;
                    _objResponse.data = model.data;
                    _objResponse.sys_message = "successful";
                    _objResponse.response_code = "200";
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


        #region GetImportantDocuments
        [HttpPost("GetImportantDocuments")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetImportantDocuments([FromBody] GetImportantLinksModel linkModel)
        {
            ServiceResponseModel accToken = APICall.GenerateAccessToken();
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + accToken.sys_message));
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPI(this.mdmurl + "/grievance/getimportantdocuments", HttpMethod.Post, linkModel, header);
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


        #region Covidexgratia
        [HttpPost("GetExgratiaLoginToken")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetExgratiaLoginToken([FromBody] EXGratiaModel grmodel)
        {
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            HttpResponseMessage _ResponseMessage = await APICall.ExecuteExternalAPIWithSSL("https://covidexgratia.punjab.gov.in/loginbypass.aspx/GetLoginToken", HttpMethod.Post, grmodel, null);
            if (_ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string result = _ResponseMessage.Content.ReadAsStringAsync().Result;
                _objResponse.response = 1;
                _objResponse.sys_message = "Successful";
                _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                _objResponse.response_code = "200";

            }
            else
            {
                _objResponse.response = 0;
                _objResponse.data = null;
                _objResponse.sys_message = "Service Unavailable";
                _objResponse.response_code = "503";

            }
            return _objResponse;
        }
        #endregion

        #region CovidexgratiaPage
        [HttpPost("GetExgratiaLoginPage")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetExgratiaLoginPage([FromBody] EXGratiaPageModel grmodel)
        {
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            HttpResponseMessage response = await APICall.ExecuteExternalAPIWithSSL("https://covidexgratia.punjab.gov.in/loginbypass.aspx/AuthLoginByPass", HttpMethod.Post, grmodel, header);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                _objResponse.response = 1;
                _objResponse.sys_message = "Successful";
                _objResponse.data = JsonConvert.DeserializeObject<dynamic>(result);
                _objResponse.response_code = "200";
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.data = null;
                _objResponse.sys_message = "Service Unavailable";
                _objResponse.response_code = "503";
            }
            return _objResponse;
        }
        #endregion

    }
}
