using EnterpriseSupportLibrary;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("api/bi-lingual")]
    public class BiLingualController : Controller
    {
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private CommonHelper _objHelper = new CommonHelper();


        public BiLingualController(IConfiguration Configuration, IHostingEnvironment HostingEnvironment)
        {
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("fetch-service-labels")]
        public ServiceResponseModel FetchServiceLabels([FromBody]BiLingualServiceModel ModelData)
        {

            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion
           

            #region FETCH SERVICE LABELS
            using (StreamReader _Reader = new StreamReader("bilingual_library.json"))
            {

                List<BilingualLibraryItem> _Library = JsonConvert.DeserializeObject<List<BilingualLibraryItem>>(_Reader.ReadToEnd());
                List<BilingualResponse> _ResponseData = new List<BilingualResponse>();

                
                _ResponseData = _Library.Where(x => x.Service_ID.Equals(ModelData.ServiceID) || ModelData.ServiceType.Contains(x.Service_Type)).Select
                (
                    x => new BilingualResponse
                    {
                        Label_ID = x.Label_ID,
                        Label_Value = ModelData.Language_Code == "en" ? x.Label_Eng : x.Label_Pb
                    }
                ).ToList();
              

                DataTable _dtResponse = new DataTable();
                DataRow _dr = _dtResponse.NewRow();
                for (int i = 0; i < _ResponseData.Count; i++)
                {
                    _dtResponse.Columns.Add(_ResponseData[i].Label_ID, typeof(string));
                    _dr[i] = _ResponseData[i].Label_Value;
                }
                _dtResponse.Rows.Add(_dr);
                _objResponse.response = 1;
                _objResponse.data = _objHelper.ConvertTableToDictionary(_dtResponse);

            }
            return _objResponse;
            #endregion
        }


        /// <summary>
        /// Get New Labels
        /// </summary>
        /// <param name="ModelData"></param>
        /// <returns></returns>

        [AllowAnonymous]
        [HttpPost]
        [Route("fetch-labels")]
        public ServiceResponseModel FetchLabels([FromBody] BilingualRequestModel model)
        {

            #region VALIDATE INPUT MODEL
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            #endregion


            #region FETCH SERVICE LABELS
            using (StreamReader _Reader = new StreamReader("bilingual_library.json"))
            {

                List<BilingualLibraryItem> _Library = JsonConvert.DeserializeObject<List<BilingualLibraryItem>>(_Reader.ReadToEnd()); // All labels
                List<BilingualResponse> _ResponseData = new List<BilingualResponse>();


                _ResponseData = _Library.Where(x => model.ServiceID.Contains(x.Service_ID) || model.ServiceType.Contains(x.Service_Type)).Select
                (
                    x => new BilingualResponse

                    {
                        Label_ID = x.Label_ID,
                        Label_Value = model.Language_Code == "en" ? x.Label_Eng : x.Label_Pb,
                    }
                ).ToList();

                DataTable _dtResponse = new DataTable();
                DataRow _dr = _dtResponse.NewRow();
                for (int i = 0; i < _ResponseData.Count; i++)
                {
                    _dtResponse.Columns.Add(_ResponseData[i].Label_ID, typeof(string));
                    _dr[i] = _ResponseData[i].Label_Value;
                }
                _dtResponse.Rows.Add(_dr);
                _objResponse.response = 1;
                _objResponse.data = _objHelper.ConvertTableToDictionary(_dtResponse);

            }
            return _objResponse;
            #endregion
        }

    }
}
