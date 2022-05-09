using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseSupportLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using GrievanceService.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using System.Security.Claims;

namespace GrievanceService.Controllers
{

    [Produces("application/json")]
    [Route("documentupload")]
    [Authorize]
    public class DocumentController : Controller
    {
        #region Controller Properties
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        private CloudBlobContainer _container;
        #endregion

        string FileUpload = string.Empty;

        public DocumentController(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {


            var storageCredentials = new StorageCredentials(Environment.GetEnvironmentVariable("BLOB_NAME"), Environment.GetEnvironmentVariable("BLOB_KEY"));
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            this._container = cloudBlobClient.GetContainerReference("filestorage");
            this._configuration = configuration;

            this._env = hostingEnvironment;
            if (hostingEnvironment.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
            }
            else if (hostingEnvironment.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
            }
            else if (hostingEnvironment.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
            }
        }


        #region Upload Document
        [HttpPost("uploaddocument")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> UploadDocument([FromBody] MediaFileModel mf)
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

                if (mf.files.Count > 0)
                {
                    //Create Datatable 
                    DataTable _dtSqlParameter = new DataTable();
                    _dtSqlParameter.Columns.Add("Doc_Url");
                    _dtSqlParameter.Columns.Add("Doc_Type_Master_ID");
                    _dtSqlParameter.Columns.Add("Doc_Ref_Transaction_ID");
                    _dtSqlParameter.Columns.Add("Doc_Mime_Type");
                    _dtSqlParameter.Columns.Add("Doc_Size");
                    _dtSqlParameter.Columns.Add("Created_By");

                    bool _flagMimePass = GetMimeTypeCheck(mf);

                    if (_flagMimePass)
                    {
                        for (int i = 0; i < mf.files.Count; i++)
                        {
                            var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png", "image/gif", "application/doc", "application/docx", "application/zip",
                                "application/pdf", "audio/mp3", "video/mp4", "video/mpeg", "video/JVBE", "application/rar", "rtf", "srt", "video/avi", "video/wmv", "video/wma"};
                            if (!supportedTypes.Contains(mf.files[i].filetype.ToLower()))
                            {
                                _objResponse.response_code = "0";
                                _objResponse.sys_message = "File type not matched! ";
                                return _objResponse;
                            }

                            if (mf.files[i].filesize > 9074287)
                            {
                                _objResponse.response = 0;
                                _objResponse.response_code = "0";
                                _objResponse.sys_message = "File size too large!";
                                return _objResponse;
                            }

                            mf.files[i].filename = Guid.NewGuid().ToString();
                            string dataSerialize = JsonConvert.SerializeObject(mf.files[i]);
                            byte[] imgBytes = Convert.FromBase64String(mf.files[i].base64);
                            string _getminetype = GetFileExtension(mf.files[i].base64.Substring(0, 5));

                            string path = System.DateTime.Now.ToString("ddMMyyyy") + "/" + _getminetype.ToLower().Split("/")[1];

                            // if (!Directory.Exists(path))
                            // {
                            //     Directory.CreateDirectory(path);
                            // }

                            DataRow _rw = _dtSqlParameter.NewRow();
                            _rw["Doc_Url"] = $"{System.DateTime.Now.ToString("ddMMyyyy") + "/" + _getminetype.ToLower().Split("/")[1] + "/"}{mf.files[i].filename}.{_getminetype.ToLower().Split("/")[1]}";
                            _rw["Doc_Type_Master_ID"] = "0";
                            _rw["Doc_Ref_Transaction_ID"] = "0";
                            _rw["Doc_Mime_Type"] = mf.files[i].filetype;
                            _rw["Doc_Size"] = mf.files[i].filesize;
                            _rw["Created_By"] = "0";
                            _dtSqlParameter.Rows.Add(_rw);

                            path = path + "/" + mf.files[i].filename + "." + _getminetype.ToLower().Split("/")[1];

                            // Upload on BLOB
                            var buffer = Convert.FromBase64String(mf.files[i].base64);
                            var newBlob = _container.GetBlockBlobReference(_rw["Doc_Url"].ToString());
                            await newBlob.UploadFromByteArrayAsync(buffer, 0, buffer.Length);
                            newBlob.Properties.ContentType = _rw["Doc_Mime_Type"].ToString();
                            await newBlob.SetPropertiesAsync();
                        }
                    }
                    else
                    {
                        _objResponse.response_code = "0";
                        _objResponse.sys_message = "File type (.jpeg, .png, .pdf) or base64 file extension not matched!";
                        return _objResponse;
                    }

                    List<SqlParameter> param = new List<SqlParameter>();

                    SqlParameter Parameter = new SqlParameter();
                    Parameter.ParameterName = "@TBL_INSERT_DOCUMENT_UPLOAD";
                    Parameter.SqlDbType = SqlDbType.Structured;
                    Parameter.Value = _dtSqlParameter;
                    param.Add(Parameter);
                    DataTable dtresp = _MSSQLGateway.ExecuteProcedure("MD_INSERT_DOCUMENT_UPLOAD", param);
                    if (_objHelper.checkDBResponse(dtresp))
                    {
                        if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) > 0)
                        {
                            _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                            _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                            _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                        }
                    }

                }
                else
                {
                    _objResponse.response_code = "0";
                    _objResponse.sys_message = "File is empty!";
                }
            }
            catch (Exception ex)
            {
                _objResponse.response_code = "0";
                _objResponse.sys_message = ex.Message;
            }

            //_objResponse.response = 1;
            //_objResponse.response_code = "200";
            //_objResponse.sys_message = $"{mf.files.Count} files have been uploaded.";

            return _objResponse;
        }
        #endregion


        #region Get Document By Document ID
        [HttpGet("getdocument/{document_id}")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetDocument(int? document_id)
        {
            List<SqlParameter> param = new List<SqlParameter>();

            param.Add(new SqlParameter("Doc_ID", Convert.ToInt32(document_id)));


            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("APP_FETCH_DOCUMENT_UPLOAD"), param);
            Dictionary<string, string> _file = new Dictionary<string, string>();
            if (dtresp.Rows.Count > 0)
            {

                var newBlob = _container.GetBlockBlobReference(dtresp.Rows[0][2].ToString());
                MemoryStream stream = new MemoryStream();
                await newBlob.DownloadToStreamAsync(stream);
                string base64 = Convert.ToBase64String(stream.ToArray());
                _file.Add("base64", base64);
                _file.Add("mime", Convert.ToString(dtresp.Rows[0][5]));
                _file.Add("doc_id", Convert.ToString(document_id));
                _objResponse.response = 1;
                _objResponse.data = _file;


            }
            return _objResponse;
        }
        #endregion

        #region Upload Document FOR ESEWA USER
        [HttpPost("uploaddocumentforesewauser")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> UploadDocumentForeSewaUser([FromBody] MediaFileModel mf)
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

                if (mf.files.Count > 0)
                {
                    //Create Datatable 
                    DataTable _dtSqlParameter = new DataTable();
                    _dtSqlParameter.Columns.Add("Doc_Url");
                    _dtSqlParameter.Columns.Add("Doc_Type_Master_ID");
                    _dtSqlParameter.Columns.Add("Doc_Ref_Transaction_ID");
                    _dtSqlParameter.Columns.Add("Doc_Mime_Type");
                    _dtSqlParameter.Columns.Add("Doc_Size");
                    _dtSqlParameter.Columns.Add("Created_By");

                    bool _flagMimePass = GetMimeTypeCheck(mf);

                    if (_flagMimePass)
                    {
                        for (int i = 0; i < mf.files.Count; i++)
                        {
                            var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png", "image/gif", "application/doc", "application/docx", "application/zip",
                                "application/pdf", "audio/mp3", "video/mp4", "video/mpeg", "video/JVBE", "application/rar", "rtf", "srt", "video/avi", "video/wmv", "video/wma"};
                            if (!supportedTypes.Contains(mf.files[i].filetype.ToLower()))
                            {
                                _objResponse.response_code = "0";
                                _objResponse.sys_message = "File type not matched! ";
                                return _objResponse;
                            }

                            if (mf.files[i].filesize > 9074287)
                            {
                                _objResponse.response = 0;
                                _objResponse.response_code = "0";
                                _objResponse.sys_message = "File size too large!";
                                return _objResponse;
                            }

                            mf.files[i].filename = Guid.NewGuid().ToString();
                            string dataSerialize = JsonConvert.SerializeObject(mf.files[i]);
                            byte[] imgBytes = Convert.FromBase64String(mf.files[i].base64);
                            string _getminetype = GetFileExtension(mf.files[i].base64.Substring(0, 5));

                            string path = System.DateTime.Now.ToString("ddMMyyyy") + "/" + _getminetype.ToLower().Split("/")[1];

                            // if (!Directory.Exists(path))
                            // {
                            //     Directory.CreateDirectory(path);
                            // }

                            DataRow _rw = _dtSqlParameter.NewRow();
                            _rw["Doc_Url"] = $"{System.DateTime.Now.ToString("ddMMyyyy") + "/" + _getminetype.ToLower().Split("/")[1] + "/"}{mf.files[i].filename}.{_getminetype.ToLower().Split("/")[1]}";
                            _rw["Doc_Type_Master_ID"] = "0";
                            _rw["Doc_Ref_Transaction_ID"] = "0";
                            _rw["Doc_Mime_Type"] = mf.files[i].filetype;
                            _rw["Doc_Size"] = mf.files[i].filesize;
                            _rw["Created_By"] = "0";
                            _dtSqlParameter.Rows.Add(_rw);

                            path = path + "/" + mf.files[i].filename + "." + _getminetype.ToLower().Split("/")[1];

                            // Upload on BLOB
                            var buffer = Convert.FromBase64String(mf.files[i].base64);
                            var newBlob = _container.GetBlockBlobReference(_rw["Doc_Url"].ToString());
                            await newBlob.UploadFromByteArrayAsync(buffer, 0, buffer.Length);
                            newBlob.Properties.ContentType = _rw["Doc_Mime_Type"].ToString();
                            await newBlob.SetPropertiesAsync();

                            #region DB PARAMETERS
                            List<SqlParameter> Parameters = new List<SqlParameter>();
                            int? Citizen_Ref_ID = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Citizen_Ref_ID"));
                            Parameters.Add(new SqlParameter("docid", 0));
                            Parameters.Add(new SqlParameter("grievanceID", Citizen_Ref_ID));
                            Parameters.Add(new SqlParameter("doc_file", ""));
                            Parameters.Add(new SqlParameter("filetype", mf.files[i].filetype));
                            Parameters.Add(new SqlParameter("Isactive", "Y"));
                            Parameters.Add(new SqlParameter("blobFileReference", _rw["Doc_Url"].ToString()));
                            Parameters.Add(new SqlParameter("blobContainer", ""));
                            Parameters.Add(new SqlParameter("created_by", Citizen_Ref_ID));
                            #endregion

                            #region DB CALL & BUSINESS LOGIC
                            DataTable _dt = _MSSQLGateway.ExecuteProcedure("INSERT_COMMON_CITIZEN_PICTURE", Parameters);
                            if (_objHelper.checkDBResponse(_dt))
                            {
                                if (Convert.ToInt32(Convert.ToString(_dt.Rows[0]["response"])) > 0)
                                {
                                    _objResponse.response = 1;
                                    _objResponse.data = _objHelper.ConvertTableToDictionary(_dt);
                                }
                                else
                                {
                                    _objResponse.response = 1;
                                    _objResponse.sys_message = Convert.ToString(_dt.Rows[0]["sys_message"]);
                                }
                            }
                            else
                            {
                                _objResponse.response = 1;
                                _objResponse.sys_message = Convert.ToString(_dt.Rows[0]["sys_message"]);
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        _objResponse.response_code = "0";
                        _objResponse.sys_message = "File type (.jpeg, .png, .pdf) or base64 file extension not matched!";
                        return _objResponse;
                    }
                }
                else
                {
                    _objResponse.response_code = "0";
                    _objResponse.sys_message = "File is empty!";
                }
            }
            catch (Exception ex)
            {
                _objResponse.response_code = "0";
                _objResponse.sys_message = ex.Message;
            }

            //_objResponse.response = 1;
            //_objResponse.response_code = "200";
            //_objResponse.sys_message = $"{mf.files.Count} files have been uploaded.";

            return _objResponse;
        }
        #endregion

        #region Get Document By Document ID FOR ESEWA USER
        [HttpGet("getdocumentforesewauser/{document_id}")]
        [AllowAnonymous]
        public async Task<ServiceResponseModel> GetDocumentforesewauser(Int64? document_id)
        {
            List<SqlParameter> param = new List<SqlParameter>();

            param.Add(new SqlParameter("docid", Convert.ToInt32(document_id)));


            DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("APP_FETCH_DOCUMENT_UPLOAD_FOR_ESEWA"), param);
            Dictionary<string, string> _file = new Dictionary<string, string>();
            if (dtresp.Rows.Count > 0)
            {

                var newBlob = _container.GetBlockBlobReference(dtresp.Rows[0][2].ToString());
                MemoryStream stream = new MemoryStream();
                await newBlob.DownloadToStreamAsync(stream);
                string base64 = Convert.ToBase64String(stream.ToArray());
                _file.Add("base64", base64);
                _file.Add("mime", Convert.ToString(dtresp.Rows[0]["filetype"]));
                _file.Add("doc_id", Convert.ToString(document_id));
                _objResponse.response = 1;
                _objResponse.data = _file;
            }
            return _objResponse;
        }
        #endregion


        public bool GetMimeTypeCheck(MediaFileModel mf)
        {
            bool _result = true;
            for (int i = 0; i < mf.files.Count; i++)
            {
                string _getminetype = GetFileExtension(mf.files[i].base64.Substring(0, 5));
                // HANDLE JPEG CASE
                if (mf.files[i].filetype.ToLower() == "image/jpg" || mf.files[i].filetype.ToLower() == "image/jpeg")
                {
                    mf.files[i].filetype = "image/jpg";
                }
                else if (mf.files[i].filetype.ToLower() == "application/x-zip-compressed")
                {
                    mf.files[i].filetype = "application/zip";
                }

                if (_getminetype.ToLower() != mf.files[i].filetype.ToLower())
                {
                    _result = false;
                }
                //string _getminetype = GetFileExtension(mf.files[i].base64.Substring(0, 5));
                //if (mf.files[i].filetype.Split("/")[1] != _getminetype)
                //{
                //    if (mf.files[i].filetype.Split("/")[0] == "image")
                //    {
                //        if (mf.files[i].filetype.Split("/")[1] == "jpeg" && _getminetype == "jpg")
                //        {
                //            _result = true;
                //        }
                //        else
                //        {
                //            _result = false;
                //        }
                //    }
                //    else
                //    {
                //        _result = false;
                //    }
                //}
            }
            return _result;
        }
        public string GetFileExtension(string base64String)
        {
            var data = base64String.Substring(0, 4);
            switch (data.ToUpper())
            {
                case "IVBO":
                    return "image/png";
                case "/9J/":
                    return "image/jpg";
                case "AAAA":
                    return "video/mp4";
                case "SUQZ":
                    return "audio/mp3";
                case "/+N":
                    return "video/mpeg";
                case "0M8R":
                    return "application/doc";
                case "UESD":
                    return "application/zip";
                case "JVBE":
                    return "application/pdf";
                case "OEJQ":
                    return "application/psd";
                case "R0lG":
                    return "image/gif";
                case "GkXf":
                    return "video/mvk";
                case "RkxW":
                    return "flv";
                case "AAAB":
                    return "ico";
                case "UMFY":
                    return "rar";
                case "E1XY":
                    return "rtf";
                case "U1PK":
                    return "application/txt";
                case "MQOW":
                case "77U/":
                    return "srt";

                //case "IVBOR":
                //    return "png";
                //case "/9J/4":
                //    return "jpg";
                //case "AAAAF":
                //    return "mp4";
                //case "/+NI":
                //    return "mpeg";
                //case "SUQzB":
                //    return "mp3";
                //case "JVBER":
                //    return "pdf";
                //case "AAABA":
                //    return "ico";
                //case "UMFYI":
                //    return "rar";
                //case "E1XYD":
                //    return "rtf";
                //case "U1PKC":
                //    return "txt";
                //case "MQOWM":
                //case "77U/M":
                //    return "srt";
                default:
                    return string.Empty;
            }
        }
    }
}
