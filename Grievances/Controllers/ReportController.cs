using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("api/report")]
    [Authorize]
    public class ReportController : Controller
    { 
        #region Controller Properties
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        string mdmurl = string.Empty;
        CommonFunctions APICall;
        #endregion

        public ReportController(IConfiguration configuration, IHostingEnvironment env)
        {
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
            }
        }

        #region Get Grievance Report
        [HttpPost("getgrievance")]
        public ServiceResponseModel GetDashboard([FromBody]Report rp)
        {
            rp.Dept = rp.Dept == "null" ? null : rp.Dept;
            rp.Dept = rp.Dept == "undefined" ? null : rp.Dept;
            List<SqlParameter> Parameters = new List<SqlParameter>();            
            Parameters.Add(new SqlParameter("FROM", Convert.ToString(rp.Fromdate)));
            Parameters.Add(new SqlParameter("TO",Convert.ToString(rp.Todate)));
            Parameters.Add(new SqlParameter("DEPT",Convert.ToInt64(rp.Dept)));
            Parameters.Add(new SqlParameter("DIST",Convert.ToString(rp.Dist)));
            Parameters.Add(new SqlParameter("STATUS",Convert.ToString(rp.Status)));
            Parameters.Add(new SqlParameter("CategoryID",Convert.ToInt64(rp.CategoryID)));
            Parameters.Add(new SqlParameter("SubCategoryID",Convert.ToInt64(rp.SubCategoryID)));
            Parameters.Add(new SqlParameter("PageNumber", Convert.ToInt64(rp.pageIndex)));
            Parameters.Add(new SqlParameter("PageSize", Convert.ToInt64(rp.PageSize)));
          

            _objResponse = ReportResponse("APP_FETCH_GRIEVANCE", Parameters);

            return _objResponse;
        }
        #endregion


        #region OverduePendency 
        [HttpPost("overduependency")]
        public async Task<ServiceResponseModel> OverduePendency([FromBody]OverdueReportModel InputModel)
        {
            getactorsbydeptidanddistidModel gad = new getactorsbydeptidanddistidModel();
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
            List<SqlParameter> Parametersactors = new List<SqlParameter>();
            Parametersactors.Add(new SqlParameter("deptid", InputModel.deptid));
            Parametersactors.Add(new SqlParameter("distid", InputModel.distid));

            gad.Parm1 = InputModel.deptid;
            gad.Parm2 = InputModel.distid;
            gad.SelectBy = "GetDesignationByStakeholderAndDistrict";


          HttpResponseMessage _ResponseMessage = await APICall.PostExternalAPI(this.mdmurl+"/master/GetDesignation", gad);
            var jsonString = await _ResponseMessage.Content.ReadAsStringAsync();

            actorresponseModelMain reportList = JsonConvert.DeserializeObject<actorresponseModelMain>(jsonString);
            var dataTable = new DataTable();
            dataTable.Columns.Add("response", typeof(String));
            dataTable.Columns.Add("Actor_ID", typeof(String));
            dataTable.Columns.Add("Designation_Name", typeof(String));
          
            foreach (var record in reportList.data)
            {
                var row = dataTable.NewRow();
                row["response"] = record.response;
                row["Actor_ID"] = record.Actor_ID;
                row["Designation_Name"] = record.Designation_Name;
                dataTable.Rows.Add(row);
            }

            //Get Grievance states
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("actortable", (dataTable)));
            Parameters.Add(new SqlParameter("deptid", InputModel.deptid));
            Parameters.Add(new SqlParameter("distid", InputModel.distid));

         //   DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("overduependencies"), Parameters);
            _objResponse = ReportResponse("overduependencies", Parameters);
            return _objResponse;

         

        }
        #endregion


        #region OverduePendency department wise
        [HttpPost("overduependencydeptid")]
        public async Task<ServiceResponseModel> overduependencydeptid([FromBody]OverdueReportdeptModel InputModel)
        {
            getactorsbydeptidanddistidModel gad = new getactorsbydeptidanddistidModel();
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);
                return _objResponse;
            }
           
            List<SqlParameter> Parameters = new List<SqlParameter>();          
            Parameters.Add(new SqlParameter("deptid", InputModel.deptid));
       

            //   DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("overduependencies"), Parameters);
            _objResponse = ReportResponse("overduependenciesdeptwise", Parameters);
            return _objResponse;



        }
        #endregion

        // Non API Route Methods

        #region Report Request and Response

        private ServiceResponseModel ReportResponse(string procedureName, List<SqlParameter> sp)
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
    }
}