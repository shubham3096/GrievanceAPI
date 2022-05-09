using EnterpriseSupportLibrary;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GrievanceService.Helpers
{
    public class CommonFunctions 
    {
        #region Controller Properties
        private ServiceResponseModelRows _objResponseRows = new ServiceResponseModelRows();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        string GetActor = string.Empty;
        string mdmurl = string.Empty;
        string Tokenkey = string.Empty;
        String public_key = string.Empty;
        public string _encKey = "lGdj50q64r00m3m3dOnlJPCSDI2J0HqZQ1T3cgerlE3Lra4eTkXWDQOWELgq7hGr";
        public string _encKeyUID = "kjbkjBBJGHfghfh3m3dOnlJPCSDKBkjbkjJKbkjb757673Lra4eTkKJKBiutt";
        public string cpkey = string.Empty;
        public string cptoken = string.Empty;
        public string Smsurl = string.Empty;
        public string SmsServerKey = string.Empty;
        #endregion

        public  CommonFunctions(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {

                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
                this.Tokenkey = this._configuration["AppSettings_Dev:Tokenkey"];
                this.public_key = this._configuration["AppSettings_Dev:public_key"];
                this.cpkey = this._configuration["AppSettings_Dev:cpkey"];
                this.cptoken = this._configuration["AppSettings_Dev:cptoken"];
                this.Smsurl = this._configuration["Smsurl"];
                this.SmsServerKey = this._configuration["SmsServerKey"];

            }
            else if (env.IsStaging())
            {

                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
                this.Tokenkey = this._configuration["AppSettings_Stag:Tokenkey"];
                this.public_key = this._configuration["AppSettings_Stag:public_key"];
                this.cpkey = this._configuration["AppSettings_Stag:cpkey"];
                this.cptoken = this._configuration["AppSettings_Stag:cptoken"];
                this.Smsurl = this._configuration["Smsurl"];
                this.SmsServerKey = this._configuration["SmsServerKey"];
            }
            else if (env.IsProduction())
            {

                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
                this.Tokenkey = this._configuration["AppSettings_Pro:Tokenkey"];
                this.public_key = this._configuration["AppSettings_Pro:public_key"];
                this.cpkey = this._configuration["AppSettings_Pro:cpkey"];
                this.cptoken = this._configuration["AppSettings_Pro:cptoken"];
                this.Smsurl = this._configuration["AppSettings_Pro:Smsurl"];
                this.SmsServerKey = this._configuration["SmsServerKey"];
            }
        }


        public async Task<HttpResponseMessage> PostExternalAPI(string URL, object PostData, List<KeyValuePair<string, string>> _Headres = null)
        {

            ServiceResponseModel accToken = GenerateAccessToken();
            string response = string.Empty;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (_Headres != null)
            {
                for (int i = 0; i < _Headres.Count; i++)
                {
                    client.DefaultRequestHeaders.Add(_Headres[i].Key, _Headres[i].Value);
                }
            }
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accToken.sys_message);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, URL);
            string _APIData = JsonConvert.SerializeObject(PostData);
            request.Content = new StringContent(_APIData, Encoding.UTF8, "application/json");
            HttpResponseMessage _ResponseMessage = await client.SendAsync(request);
            return _ResponseMessage;
        }

        #region EXTERNAL API CALL
        public async Task<HttpResponseMessage> ExecuteExternalAPI(string URL, HttpMethod method, object data = null, List<KeyValuePair<string, string>> header = null)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (header != null)
                {
                    for (int i = 0; i < header.Count; i++)
                    {
                        client.DefaultRequestHeaders.Add(header[i].Key, header[i].Value);
                    }
                }

                HttpRequestMessage request = new HttpRequestMessage(method, URL);
                if (method == HttpMethod.Post || method == HttpMethod.Put)
                {
                    string apiData = JsonConvert.SerializeObject(data);
                    request.Content = new StringContent(apiData, Encoding.UTF8, "application/json");
                }

                responseMessage = await client.SendAsync(request);
                string content = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
            }

            return responseMessage;
        }
        #endregion

        #region EXTERNAL API CALL with SSL Enable
        public async Task<HttpResponseMessage> ExecuteExternalAPIWithSSL(string URL, HttpMethod method, object data = null, List<KeyValuePair<string, string>> header = null)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            try
            {
                // if " The SSL connection could not be established " use below code this 
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                // Pass the handler to httpclient(from you are calling api)
                HttpClient client = new HttpClient(clientHandler);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (header != null)
                {
                    for (int i = 0; i < header.Count; i++)
                    {
                        client.DefaultRequestHeaders.Add(header[i].Key, header[i].Value);
                    }
                }

                HttpRequestMessage request = new HttpRequestMessage(method, URL);
                if (method == HttpMethod.Post || method == HttpMethod.Put)
                {
                    string apiData = JsonConvert.SerializeObject(data);
                    request.Content = new StringContent(apiData, Encoding.UTF8, "application/json");
                }

                responseMessage = await client.SendAsync(request);
                string content = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
            }

            return responseMessage;
        }
        #endregion
        public async Task<HttpResponseMessage> PostExternalAPI(string URL)
        {
            ServiceResponseModel accToken = GenerateAccessToken();

            string response = string.Empty;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accToken.sys_message);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URL);
            HttpResponseMessage _ResponseMessage = await client.SendAsync(request);
            return _ResponseMessage;
        }


        public async Task<HttpResponseMessage> GetExternalAPICP(string URL)
        {
            var queryString = new Dictionary<string, string>()
            {
                { "key", this.cpkey },
                { "token", this.cptoken }
            };            

            string response = string.Empty;
            HttpClient client = new HttpClient();

           // client.BaseAddress = new Uri("https://pgportal.gov.in/");
            var requestUri = QueryHelpers.AddQueryString(URL, queryString);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accToken.sys_message);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage _ResponseMessage = await client.SendAsync(request);
            return _ResponseMessage;    
        }

        public async Task<HttpResponseMessage> PostExternalAPICP(string URL, object PostData, List<KeyValuePair<string, string>> _Headres = null)
        {
            var queryString = new Dictionary<string, string>()
            {
                { "key", this.cpkey },
                { "token", this.cptoken }
            };

            string response = string.Empty;
            HttpClient client = new HttpClient();
           
            var requestUri = QueryHelpers.AddQueryString(URL, queryString);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (_Headres != null)
            {
                for (int i = 0; i < _Headres.Count; i++)
                {
                    client.DefaultRequestHeaders.Add(_Headres[i].Key, _Headres[i].Value);
                }
            }
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            string _APIData = JsonConvert.SerializeObject(PostData);
            request.Content = new StringContent(_APIData, Encoding.UTF8, "application/json");
            HttpResponseMessage _ResponseMessage = await client.SendAsync(request);
            return _ResponseMessage;
        }

        public async Task<HttpResponseMessage> PostExternalAPI(string URL, string _Headres = null)
        {
            string response = string.Empty;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _Headres);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URL);
            HttpResponseMessage _ResponseMessage = await client.SendAsync(request);
            return _ResponseMessage;
        }

        public async Task<HttpResponseMessage> GetExternalAPI(string URL)
        {
            string response = string.Empty;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
          //  client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _Headres);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URL);
            HttpResponseMessage _ResponseMessage = await client.SendAsync(request);
            return _ResponseMessage;
        }

        public ServiceResponseModel GenerateAccessToken()
        {
            string ret = "";

            ServiceResponseModel apiResponse = new ServiceResponseModel();
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.mdmurl + "/g2gapiaccess/generateToken");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "Post";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{ 'Access_Key':'" + this.Tokenkey + "','Public_Key':'" + this.public_key + "'}";
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


        public String getsha256_hash(string value)
        {

            StringBuilder Sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        public string Encrypt(string clearText, string encryptionKey = null, string encryptionIV = null)
        {
            var rj = new RijndaelManaged()
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128
            };
            byte[] encKey;
            byte[] encIV;
            if (encryptionKey != null && encryptionIV != null)
            {
                encKey = Encoding.ASCII.GetBytes(encryptionKey);
                encIV = Encoding.ASCII.GetBytes(encryptionIV);
            }
            else
            {
                encKey = Encoding.ASCII.GetBytes(_configuration["Encryption_Key"].ToString());
                encIV = Encoding.ASCII.GetBytes(_configuration["Encryption_IV"].ToString());
            }

            var encryptor = rj.CreateEncryptor(encKey, encIV);

            var msEncrypt = new MemoryStream();
            var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

            var toEncrypt = Encoding.ASCII.GetBytes(clearText);

            csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
            csEncrypt.FlushFinalBlock();

            var encrypted = msEncrypt.ToArray();

            return (Convert.ToBase64String(encrypted));
        }

        public string Decrypt(string cipherTextstring, string encryptionKey = null, string encryptionIV = null)
        {
            string plaintext = null;
            byte[] cipherText = Convert.FromBase64String(cipherTextstring);
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                byte[] encKey;
                byte[] encIV;
                if (encryptionKey != null && encryptionIV != null)
                {
                    encKey = Encoding.ASCII.GetBytes(encryptionKey);
                    encIV = Encoding.ASCII.GetBytes(encryptionIV);
                }
                else
                {
                    encKey = Encoding.ASCII.GetBytes(_configuration["Encryption_Key"]);
                    encIV = Encoding.ASCII.GetBytes(_configuration["Encryption_IV"]);
                }

                // Create AesManaged    
                using (AesManaged aes = new AesManaged())
                {
                    // Create a decryptor    
                    ICryptoTransform decryptor = aes.CreateDecryptor(encKey, encIV);
                    // Create the streams used for decryption.    
                    using (MemoryStream ms = new MemoryStream(cipherText))
                    {
                        // Create crypto stream    
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            // Read crypto stream    
                            using (StreamReader reader = new StreamReader(cs))
                                plaintext = reader.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }


        public string DecryptREFKEYUID(string cipherText)
        {

            string EncryptionKey = _encKeyUID;
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
        }


        public string EncryptUID(string clearText)
        {
            string EncryptionKey = _encKeyUID;
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;

        }

        #region SP call
        public ServiceResponseModel GrievanceResponse(string procedureName, List<SqlParameter> sp)
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

        //For SMS
        public string SendSMS(string mobilenumber, string msg, string templateId)
        {
            ServiceResponseModel accToken = GenerateAccessToken();
            string ret = "";
            try
            {
                //var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.Smsurl + "/SMS/v1");
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.Smsurl);
                httpWebRequest.ContentType = "application/json";
                //httpWebRequest.Headers.Add("Authorization", "Bearer " + accToken.sys_message);
                httpWebRequest.Headers.Add("Server-Key", this.SmsServerKey);
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
        
    }
}
