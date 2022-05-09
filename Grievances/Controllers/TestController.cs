using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseSupportLibrary;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GrievanceService.Controllers
{
    [Produces("application/json")]
    [Route("api/rpt")]
    public class TestController : Controller
    {
        #region Controller Properties
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private IHostingEnvironment _env;
        #endregion

        public TestController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Test"));

            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Test"));

            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Test"));

            }
        }

        #region Get Grievance Report
        [HttpGet("getdata/{Stakeholder_ID?}/{Dept_ID?}/{Designation_ID?}/{Office_ID?}")]
        public ServiceResponseModel GetDashboard(int? Stakeholder_ID, int? Dept_ID, int? Designation_ID, int? Office_ID)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();

            Parameters.Add(new SqlParameter("Stakeholder_ID", Stakeholder_ID));
            Parameters.Add(new SqlParameter("Dept_ID", Dept_ID));
            Parameters.Add(new SqlParameter("Designation_ID", Designation_ID));
            Parameters.Add(new SqlParameter("Office_ID", Office_ID));

            _objResponse = ReportResponse("GETDATA", Parameters);

            return _objResponse;
        }
        #endregion

        #region Get Department
        [HttpGet("getDepartment")]
        public ServiceResponseModel GetSubDepartment()
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();

            _objResponse = ReportResponse("GET_DEPARTMENTS", Parameters);

            return _objResponse;
        }
        #endregion

        #region Get Sub Department
        [HttpGet("getSubDepartment/{Stakeholder_ID?}")]
        public ServiceResponseModel GetSubDepartment(int? Stakeholder_ID)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();

            Parameters.Add(new SqlParameter("Stakeholder_ID", Stakeholder_ID));

            _objResponse = ReportResponse("GET_SUB_DEPARTMENTS", Parameters);

            return _objResponse;
        }
        #endregion

        #region Get Office Level
        [HttpGet("getOfficeLevel/{Stakeholder_ID?}")]
        public ServiceResponseModel GetOfficeLevel(int? Stakeholder_ID)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();

            Parameters.Add(new SqlParameter("Stakeholder_ID", Stakeholder_ID));

            _objResponse = ReportResponse("GET_OFFICELEVELS", Parameters);

            return _objResponse;
        }
        #endregion

        #region Get Offices
        [HttpGet("getOffices/{Office_Level?}/{Stakeholder_ID?}")]
        public ServiceResponseModel GetOffices(int? Office_Level, int? Stakeholder_ID)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();

            Parameters.Add(new SqlParameter("Office_Level", Office_Level));
            Parameters.Add(new SqlParameter("Stakeholder_ID", Stakeholder_ID));

            _objResponse = ReportResponse("GET_OFFICES", Parameters);

            return _objResponse;
        }
        #endregion

        #region Get Designation
        [HttpGet("getDesignation/{Stakeholder_ID?}/{Office_ID?}")]
        public ServiceResponseModel GetDesignation(int? Stakeholder_ID, int? Office_ID)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();

            Parameters.Add(new SqlParameter("Stakeholder_ID", Stakeholder_ID));
            Parameters.Add(new SqlParameter("Office_ID", Office_ID));

            _objResponse = ReportResponse("GET_DESIGNATION", Parameters);

            return _objResponse;
        }
        #endregion

        #region Get Master
        [AllowAnonymous]
        [HttpPost]
        public ServiceResponseModel Post([FromBody] MasterModel am)
        {
            // Check validation
            if (!ModelState.IsValid)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = _objHelper.GetModelErrorMessages(ModelState);

                return _objResponse;
            }

            List<SqlParameter> Parameters = new List<SqlParameter>();

            Parameters.Add(new SqlParameter("SelectBy", am.SelectBy));
            Parameters.Add(new SqlParameter("Parm1", am.Parm1));
            Parameters.Add(new SqlParameter("Parm2", am.Parm2));
            Parameters.Add(new SqlParameter("Parm3", am.Parm3));

            _objResponse = ReportResponse("APP_FETCH_MASTER", Parameters);
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