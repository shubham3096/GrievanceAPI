using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Controllers
{   
    [Produces("application/json")]
    [Route("CitizenFeedback")]
    [Authorize]
    public class CitizenFeedbackController : Controller
    {
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonFunctions _commonFunction;
        private MSSQLGateway _MSSQLGateway;
        private CommonHelper _objHelper = new CommonHelper();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();

        public CitizenFeedbackController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            _commonFunction = new CommonFunctions(configuration, env);
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
               
            }

        }

        #region Feedback
        [HttpPost]
        [Route("Feedback")]
        [Authorize]
        public ServiceResponseModel SaveCitizenFeedback([FromBody] CitizenFeedbackModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "Data Not Sufficient";

                    return _objResponse;
                }
                List<SqlParameter> param1 = new List<SqlParameter>();

                param1.Add(new SqlParameter("Grievance_ID",Convert.ToInt64(model.Grievance_ID)));
                param1.Add(new SqlParameter("Action_Taken_By",Convert.ToInt64(model.Action_Taken_By)));
                param1.Add(new SqlParameter("Remarks", model.Remarks));

                DataTable dtresp = _MSSQLGateway.ExecuteProcedure("CitizenFeedbackByCallcenter", param1);
                _objResponse.response = Convert.ToInt32(dtresp.Rows[0]["response"]);
                if (_objResponse.response == 1)
                {
                  
                    _objResponse.sys_message = "Successfully Inserted";
                }
                else
                {
                    _objResponse.sys_message = "Grievance Already Resolved";
                }
            }

            catch (Exception exc)
            {
                _objResponse.response = 0;
                _objResponse.sys_message = exc.Message;
            }

            return _objResponse;    
        }
        #endregion
    }
}
