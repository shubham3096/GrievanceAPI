using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using EnterpriseSupportLibrary;
using Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace GrievanceService.Controllers
{

    [Produces("application/json")]
    [Route("api/menus")]
    [Authorize]
    public class MenuController : Controller
    {
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private SQLGateway _MSSQLGateway;
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonHelper _objHelper = new CommonHelper();

        public MenuController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new SQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new SQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new SQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
            }

        }

        #region  Navigation By Actor ID
        [HttpGet("getnavigationmenubyactorid")]
        [Authorize]
        public ServiceResponseModel GetNavigationSubMenuAsync()
        {
            int? actor_id = Convert.ToInt32(_objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "Actor_Ref_ID"));
            DataTable dtresp = new DataTable();
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("Actor_ID", Convert.ToString(actor_id)));
            dtresp = _MSSQLGateway.ExecuteProcedure("APP_FETCH_MENU_BY_ROLEID", param);
            if (_objHelper.checkDBResponse(dtresp))
            {
                if (Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"])) > 0)
                {
                    _objResponse.response = Convert.ToInt32(Convert.ToString(dtresp.Rows[0]["response"]));
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                    // _objResponse.sys_message = Convert.ToString(dtresp.Rows[0]["message"].ToString());
                }
                else {
                    _objResponse.response = -2;
                    _objResponse.data = _objHelper.ConvertTableToDictionary(dtresp);
                }
            }
            else {
                _objResponse.response = -1;
                _objResponse.sys_message = _env.EnvironmentName;
            }
            return _objResponse;
        }
        #endregion



    }
}
