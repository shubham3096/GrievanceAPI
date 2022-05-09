using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("api/GetNFSA")]

    public class NFSADashboardController : Controller
    {
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private MSSQLGateway _MSSQLGateway;
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private NFSAtoPGRSResponseModel _nfsaobjResponse = new NFSAtoPGRSResponseModel();

        private CommonFunctions _commonFunction;
        private string nfsaUrl;
        private string nfsaAuthString;
        private string authkey;
        private CommonHelper _objHelper = new CommonHelper();

        public NFSADashboardController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            _commonFunction = new CommonFunctions(configuration, env);
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                nfsaUrl = configuration["AppSettings_Dev:NFSA_URL"];
                nfsaAuthString = configuration["AppSettings_Dev:NFSA_Auth_String"];
                authkey = configuration["AppSettings_Dev:authkey"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                nfsaUrl = configuration["AppSettings_Stag:NFSA_URL"];
                nfsaAuthString = configuration["AppSettings_Stag:NFSA_Auth_String"];
                authkey = configuration["AppSettings_Dev:authkey"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                nfsaUrl = configuration["AppSettings_Pro:NFSA_URL"];
                nfsaAuthString = configuration["AppSettings_Pro:NFSA_Auth_String"];
                authkey = configuration["AppSettings_Dev:authkey"];
            }

        }
        #region GetNFSADashboard
        [HttpPost]
        [Route("GetNFSAdashboard")]
        public async Task<ServiceResponseModel> GetNFSA_DashboardCountAsync()
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("NFSADeshboardCount", Parameters);
            if (dtresp != null)
            {
                NFSADashboardModel nfsaDashboard = new NFSADashboardModel()
                {
                    State = "03",
                    Token = "03000" + DateTime.Now.ToString("ddMMyyyyHHmmss") + "12345",
                    AuthString = nfsaAuthString,
                    DateOfData = DateTime.Now.AddDays(-1).ToString("MM-dd-yyyy"),
                    PGRMSData = ConvertPGRMSDataDtToClass(dtresp)
                };
                var responseTask = await _commonFunction.PostExternalAPI(nfsaUrl + "PGRMS/dashboard_summary_data", nfsaDashboard);
                if (responseTask.IsSuccessStatusCode)
                {
                    string jsonString = await responseTask.Content.ReadAsStringAsync();

                    NFSAResponseModel nfsaResponse = JsonConvert.DeserializeObject<NFSAResponseModel>(jsonString);

                    if (nfsaResponse.Status == "ACCP")
                    {
                        _objResponse.response = 1;
                    }
                    else
                    {
                        _objResponse.response = 0;
                    }

                    _objResponse.sys_message = nfsaResponse.Remarks;
                    _objResponse.data = nfsaResponse;
                }
            }
            else
            {
                _objResponse.data = null;
                _objResponse.response = 0;
            }
            return _objResponse;
        }

        private List<PGRMSDataModel> ConvertPGRMSDataDtToClass(DataTable dt)
        {
            List<PGRMSDataModel> pgrmDataList = new List<PGRMSDataModel>();

            foreach (DataRow row in dt.Rows)
            {
                PGRMSDataModel pgrmData = new PGRMSDataModel()
                {
                    District = row["District"].ToString(),
                    GrievanceTypeCode = "OTH",
                    ModeofGrievanceCode = "P",
                    PlatformofGrievanceCode = "S",
                    CountsPendingOneWeek = row["CountsPendingOneWeek"].ToString(),
                    CountsPendingTwoWeek = row["CountsPendingTwoWeek"].ToString(),
                    CountsPendingThreeWeek = row["CountsPendingThreeWeek"].ToString(),
                    CountsPendingMoreThanThreeWeek = row["CountsPendingMoreThanThreeWeek"].ToString(),
                    CountsResolvedOneWeek = row["CountsResolvedOneWeek"].ToString(),
                    CountsResolvedTwoWeek = row["CountsResolvedTwoWeek"].ToString(),
                    CountsResolvedThreeWeek = row["CountsResolvedThreeWeek"].ToString(),
                    CountsResolvedMoreThanThreeWeek = row["CountsResolvedMoreThanThreeWeek"].ToString()
                };
                pgrmDataList.Add(pgrmData);
            }

            return pgrmDataList;
        }
        #endregion

        #region NFSatoPGRSAddnew
        [HttpPost]
        [Route("NFSAtoPGRSAddNew")]
        public async Task<NFSAtoPGRSResponseModel> SaveNFSADetailstoPGRS([FromBody] NFSAtoPGRSAddNewGrievanceModel model)
        {
            List<ResponseDetailsModel> resDetailObj = new List<ResponseDetailsModel>();
            if (ModelState.IsValid)
            {
                bool isValidtoToken = compareToken(model.Token);

                if (model.AuthString == authkey && model.State == "03" && isValidtoToken == true)
                {
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("District", model.GrievanceData.District));
                    DataTable dt = _MSSQLGateway.ExecuteProcedure("getNFSAtoDistrictbyCode", param);
                    model.GrievanceData.District = dt.Rows[0]["District"].ToString();
                    int DistrictId =Convert.ToInt32(dt.Rows[0]["District_ID"]);
                    GrievanceModel grievanceModel = new GrievanceModel()
                    {
                        Citizen_Email = model.GrievanceData.Email,
                        Citizen_Name = model.GrievanceData.Name,
                        Citizen_Mobile_No = model.GrievanceData.Mobile,
                        Citizen_Address = model.GrievanceData.Address,
                        Citizen_Tehsil = model.GrievanceData.Tahsil,
                        Citizen_State = "Punjab",
                        Citizen_District = model.GrievanceData.District,
                        Application_Description = model.GrievanceData.GrievanceDetails,
                        Flow_Type = "I",
                        Citizen_EA_User_ID = 01,
                        Application_Department = 122,
                        Application_Title = model.GrievanceData.GrievanceDetails,
                        Citizen_District_ID = Convert.ToInt32(dt.Rows[0]["District_ID"]),
                        Citizen_State_ID = 7,
                        LocationType ="1",
                        System_type = "EA",
                        Citizen_Type = "All Other Individuals",
                        Application_Department_Name = "FOOD CIVIL SUPPLIES AND CONSUMER AFFAIRS",
                        Application_District_Name = model.GrievanceData.District,
                        Sub_Category_ID = 902,
                        Category_ID = 191,
                        Application_District = DistrictId,
                        NFSAGrievanceId = model.GrievanceData.NFSAGrievanceId
                    };

                    MediaFileModel mediaFileModel = new MediaFileModel()
                    {
                        files = new List<InputFile>
                        {
                            new InputFile
                            {
                                base64 = model.AttachmentDetails.Select(x => x.Base64Data).FirstOrDefault(),
                                filename = model.AttachmentDetails.Select(x => x.AttachmentName).FirstOrDefault(),
                                filetype = model.AttachmentDetails.Select(x => x.AttachmentFileType).FirstOrDefault(),
                                doc_id = "",
                                filesize = model.AttachmentDetails.Select(x => x.Base64Data).FirstOrDefault().Length
                            }
                        }
                    };
                    DocumentController documentController = new DocumentController(_configuration, _env);
                    var mediaResponse = await documentController.UploadDocument(mediaFileModel);
                    GrievanceController grievanceController = new GrievanceController(_configuration, _env);
                    var response = await grievanceController.CreateGrievanceAsync(grievanceModel);
                    List<GrievanceIdModel> grievanceIdModel = new List<GrievanceIdModel>();
                    if (response.response == 0)
                    {
                        resDetailObj.Add(
                            new ResponseDetailsModel()
                            {
                                StateGrievanceId = null,
                                GrievanceDate = model.GrievanceData.GrievanceDate,
                                NFSAGrievanceId = model.GrievanceData.NFSAGrievanceId,
                                Status = "REJC",
                                Remarks = "Rejected"
                            }
                          );
                        _nfsaobjResponse = new NFSAtoPGRSResponseModel()
                        {
                            State = model.State,
                            Token = model.Token,
                            AckCode = "REJC" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                            Status = "REJ",
                            Remarks = "Rejected,This Type of Grievance already Submitted",
                            ServiceDate = DateTime.Now.ToString("dd/MM/yyyy"),
                            ResponseDetails = resDetailObj
                        };
                    }
                    else if (response != null)
                    {
                        grievanceIdModel = JsonConvert.DeserializeObject<List<GrievanceIdModel>>(JsonConvert.SerializeObject(response.data));
                        List<SqlParameter> ParametersUnmatchData = new List<SqlParameter>();
                        ParametersUnmatchData.Add(new SqlParameter("NFSAGrievanceId", model.GrievanceData.NFSAGrievanceId));
                        ParametersUnmatchData.Add(new SqlParameter("GrievanceDate", model.GrievanceData.GrievanceDate));
                        ParametersUnmatchData.Add(new SqlParameter("GrievanceTypeCode", model.GrievanceData.GrievanceTypeCode));
                        ParametersUnmatchData.Add(new SqlParameter("TentativeCompletionDate", model.GrievanceData.TentativeCompletionDate));
                        ParametersUnmatchData.Add(new SqlParameter("ModeofGrievanceCode", model.GrievanceData.ModeofGrievanceCode));
                        ParametersUnmatchData.Add(new SqlParameter("PlatformofGrievanceCode", model.GrievanceData.PlatformofGrievanceCode));
                        ParametersUnmatchData.Add(new SqlParameter("RCNo", model.GrievanceData.RCNo));
                        ParametersUnmatchData.Add(new SqlParameter("FPSNo", model.GrievanceData.FPSNo));
                        ParametersUnmatchData.Add(new SqlParameter("FPSName", model.GrievanceData.FPSName));
                        ParametersUnmatchData.Add(new SqlParameter("FPSState", model.GrievanceData.FPSState));
                        ParametersUnmatchData.Add(new SqlParameter("FPSDistrict", model.GrievanceData.FPSDistrict));
                        ParametersUnmatchData.Add(new SqlParameter("AadharNumber", model.GrievanceData.AadharNumber));
                        ParametersUnmatchData.Add(new SqlParameter("GrievanceStatus", model.GrievanceData.GrievanceStatus));

                        DataTable dtrespUnmatchedData = _MSSQLGateway.ExecuteProcedure("NFSAtoPGRSExtraData", ParametersUnmatchData);
                        resDetailObj.Add(
                            new ResponseDetailsModel()
                            {
                                StateGrievanceId = grievanceIdModel[0].Grievance_id,
                                GrievanceDate = model.GrievanceData.GrievanceDate,
                                NFSAGrievanceId = model.GrievanceData.NFSAGrievanceId,
                                Status = "ACPT",
                                Remarks = "Accepted"
                            }
                          );
                         _nfsaobjResponse = new NFSAtoPGRSResponseModel()
                        {
                            State = model.State,
                            Token = model.Token,
                            AckCode = "ACPT" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                            Status = "ACPT",
                            Remarks = "Accepted",
                            ServiceDate = DateTime.Now.ToString("dd/MM/yyyy"),
                            ResponseDetails = resDetailObj
                        };
                    }
                }
                else
                {
                    resDetailObj.Add(
                           new ResponseDetailsModel()
                           {
                               StateGrievanceId = null,
                               GrievanceDate = model.GrievanceData.GrievanceDate,
                               NFSAGrievanceId = model.GrievanceData.NFSAGrievanceId,
                               Status = "REJC",
                               Remarks = "Rejected"
                           }
                         );
                    _nfsaobjResponse = new NFSAtoPGRSResponseModel()
                    {
                        State = model.State,
                        Token =model.Token,
                        AckCode = "REJC" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                        Status = "REJ",
                        Remarks = "StateCode / Token / AuthString Is Invalid",
                        ServiceDate = DateTime.Now.ToString("dd/MM/yyyy"),
                        ResponseDetails = resDetailObj
                    };                    
                }
            }
            else
            {
                resDetailObj.Add(
                            new ResponseDetailsModel()
                            {
                                StateGrievanceId = null,
                                GrievanceDate = model.GrievanceData.GrievanceDate,
                                NFSAGrievanceId = model.GrievanceData.NFSAGrievanceId,
                                Status = "REJC",
                                Remarks = "Rejected"
                            }
                          );
                _nfsaobjResponse = new NFSAtoPGRSResponseModel()
                {
                    State = model.State,
                    Token =model.Token,
                    AckCode = "REJC" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                    Status = "REJ",
                    Remarks = "Rejected",
                    ServiceDate = DateTime.Now.ToString("dd/MM/yyyy"),
                    ResponseDetails = resDetailObj
                };
            }
            return _nfsaobjResponse;
        }
        #endregion

        #region Createauth
        [Route("CreateAuthstring")]
        public ServiceResponseModel Gettokencountasync()
        {
            string AuthString = authkey;
            _objResponse.data = "AuthString : " + AuthString;
            _objResponse.response = 1;
            _objResponse.sys_message = "Success";

            return _objResponse;
        }
        #endregion


        public static bool compareToken(string token)
        {
            try
            {
                string str = token;
                string str2 = str.Split(new string[] { "03000" }, 3, StringSplitOptions.None)[1];
                var str3 = str2.Substring(0, str2.Length - 11);
                var res = str3.Substring(0, 2) + "/" + str3.Substring(2);
                res = res.Substring(0, 5) + "/" + res.Substring(5);
                DateTime dt = DateTime.ParseExact(res, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (DateTime.Now >= dt && DateTime.Now.AddDays(-1) < dt)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch ( Exception)
            {
                return false;
            }
        }
    }
}