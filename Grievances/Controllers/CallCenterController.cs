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
    [Route("callcenter/api")]
    [Authorize]

    public class CallCenterController : Controller
    {
        #region Controller Properties
        private ServiceResponseModelRows _objResponseRows = new ServiceResponseModelRows();
        private ServiceResponseModel _objResponse = new ServiceResponseModel();
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MSSQLGateway _MSSQLGateway;
        private MSSQLGateway _MSSQLGateway_CP;
        private IHostingEnvironment _env;
        CommonFunctions APICall;
        string Getreminders = string.Empty;
        string sendresponse = string.Empty;
        string returnresponse = string.Empty;
        string GetGrievances = string.Empty;
        string mdmurl = string.Empty;
        #endregion

        public CallCenterController(IConfiguration configuration, IHostingEnvironment env)
        {
            APICall = new CommonFunctions(configuration, env);
            this._configuration = configuration;
            this._env = env;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP"));
                this.Getreminders = this._configuration["AppSettings_Dev:Getreminders"];
                this.sendresponse = this._configuration["AppSettings_Dev:sendresponse"];
                this.returnresponse = this._configuration["AppSettings_Dev:returnresponse"];
                this.GetGrievances = this._configuration["AppSettings_Dev:GetGrievances"];
                this.mdmurl = this._configuration["AppSettings_Dev:MDMUrl"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP_stag"));
                this.Getreminders = this._configuration["AppSettings_Stag:Getreminders"];
                this.sendresponse = this._configuration["AppSettings_Dev:sendresponse"];
                this.returnresponse = this._configuration["AppSettings_Dev:returnresponse"];
                this.GetGrievances = this._configuration["AppSettings_Stag:GetGrievances"];
                this.mdmurl = this._configuration["AppSettings_Stag:MDMUrl"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this._MSSQLGateway_CP = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev_CP_pro"));
                this.Getreminders = this._configuration["AppSettings_Pro:Getreminders"];
                this.sendresponse = this._configuration["AppSettings_Pro:sendresponse"];
                this.returnresponse = this._configuration["AppSettings_Pro:returnresponse"];
                this.GetGrievances = this._configuration["AppSettings_Pro:GetGrievances"];
                this.mdmurl = this._configuration["AppSettings_Pro:MDMUrl"];
            }


        }
        #region ADD CITIZEN PROFILE
        [HttpPost("addCitzenProfile")]
        
        public ServiceResponseModel AddCitzenProfile([FromBody] AddCitizenModel InputModel)
        {
                List<SqlParameter> Parameterrb = new List<SqlParameter>();
                Parameterrb.Add(new SqlParameter("First_Name", InputModel.First_Name));
                Parameterrb.Add(new SqlParameter("Middle_Name", InputModel.Middle_Name));
                Parameterrb.Add(new SqlParameter("Last_Name", InputModel.Last_Name));
                Parameterrb.Add(new SqlParameter("Father_Name", InputModel.Father_Name));
                Parameterrb.Add(new SqlParameter("Mother_Name", InputModel.Mother_Name));
                Parameterrb.Add(new SqlParameter("Email_ID", InputModel.Email_ID));
                Parameterrb.Add(new SqlParameter("Phone_No", InputModel.Phone_No));
                Parameterrb.Add(new SqlParameter("Date_Of_Birth", InputModel.Date_Of_Birth));
                Parameterrb.Add(new SqlParameter("Gender", InputModel.Gender));               
                Parameterrb.Add(new SqlParameter("address_Line_1", InputModel.address_Line_1));
                Parameterrb.Add(new SqlParameter("address_Line_2", InputModel.address_Line_2));
                Parameterrb.Add(new SqlParameter("district_ID", InputModel.district_ID));
                Parameterrb.Add(new SqlParameter("tehsil_ID", InputModel.tehsil_ID));
                Parameterrb.Add(new SqlParameter("village_ID", InputModel.village_ID));
                Parameterrb.Add(new SqlParameter("village_Name", InputModel.village_Name));
                Parameterrb.Add(new SqlParameter("Municipality_ID", InputModel.Municipality_ID));
                Parameterrb.Add(new SqlParameter("Created_By", InputModel.Operator_ID));


            DataTable dtrespcp = _MSSQLGateway.ExecuteProcedure("Add_User_Profile_CallCenter", Parameterrb);
            if (dtrespcp.Rows.Count > 0)
            {
                _objResponse.response = 1;
                _objResponse.sys_message = "successfully inserted";
                _objResponse.data = dtrespcp;
                _objResponse.response_code = "ACPT.";
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "Failed";
                _objResponse.data = "Data Not Found";
                _objResponse.response_code = "REJ.";
            }
            return _objResponse;
        }
        #endregion

        #region UPDATE CITIZEN PROFILE
        [HttpPost("updateCitzenProfile")]
        
        public ServiceResponseModel UpdateCitzenProfile([FromBody] UpdateCitizenModel InputModel)
        {
            List<SqlParameter> Parameterrb = new List<SqlParameter>();
            Parameterrb.Add(new SqlParameter("Citizen_ID", InputModel.Citizen_ID));
            Parameterrb.Add(new SqlParameter("First_Name", InputModel.First_Name));
            Parameterrb.Add(new SqlParameter("Middle_Name", InputModel.Middle_Name));
            Parameterrb.Add(new SqlParameter("Last_Name", InputModel.Last_Name));
            Parameterrb.Add(new SqlParameter("Father_Name", InputModel.Father_Name));
            Parameterrb.Add(new SqlParameter("Mother_Name", InputModel.Mother_Name));
            Parameterrb.Add(new SqlParameter("Email_ID", InputModel.Email_ID));
            Parameterrb.Add(new SqlParameter("Phone_No", InputModel.Phone_No));
            Parameterrb.Add(new SqlParameter("Date_Of_Birth", InputModel.Date_Of_Birth));
            Parameterrb.Add(new SqlParameter("Gender", InputModel.Gender));
            Parameterrb.Add(new SqlParameter("address_Line_1", InputModel.address_Line_1));
            Parameterrb.Add(new SqlParameter("address_Line_2", InputModel.address_Line_2));
            Parameterrb.Add(new SqlParameter("District_Ref_ID", InputModel.district_ID));
            Parameterrb.Add(new SqlParameter("Tehsil_Ref_ID", InputModel.tehsil_ID));
            Parameterrb.Add(new SqlParameter("village_ID", InputModel.village_ID));
            Parameterrb.Add(new SqlParameter("village_Name", InputModel.village_Name));
            Parameterrb.Add(new SqlParameter("Municipality_ID", InputModel.Municipality_ID));
            Parameterrb.Add(new SqlParameter("Modified_By", InputModel.Operator_ID));


            DataTable dtrespcp = _MSSQLGateway.ExecuteProcedure("Update_User_Profile_CallCenter", Parameterrb);

            if (dtrespcp.Rows.Count > 0)
            {
                _objResponse.response = 1;
                _objResponse.sys_message = "successfully updated";
                _objResponse.data = dtrespcp;
                _objResponse.response_code = "ACPT.";
            }
            else
            {
                _objResponse.response = 0;
                _objResponse.sys_message = "Failed";
                _objResponse.data = "Data Not Found";
                _objResponse.response_code = "REJ.";
            }

            return _objResponse;
        }
        #endregion



        #region GetCitizenDetailsByPhoneNumber
        [HttpGet("GetCitizenDetail/{Phone_No}")]
        public ServiceResponseModel GetCitizen(long Phone_No)
        {
            try
            {
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("Phone_No", (Phone_No)));
                DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("get_CitizenDetails_ByPhoneNumber"), param);
                if (dtresp.Rows.Count > 0)
                {
                    _objResponse.response = 1;
                    _objResponse.sys_message = "success";
                    _objResponse.response_code = "ACPT.";
                    _objResponse.data = dtresp;

                    return _objResponse;
                }
                else
                {
                    _objResponse.response = 0;
                    _objResponse.sys_message = "Failed";
                    _objResponse.data = "Data Not Found";
                    _objResponse.response_code = "REJ.";
                    return _objResponse;
                }
            }
            catch (Exception ex)
            {

                _objResponse.response = 1;
                _objResponse.sys_message = "Error";
                _objResponse.data = ex.Message;
                return _objResponse;
            }


        }
        #endregion

        #region ListOfGrievance
        [HttpGet("GetGrievanceList/{Phone_No}")]
        
        public ServiceResponseModel GetGrievanceList(long Phone_No)
        {
            try
            {
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("Phone_No", Phone_No));
                DataTable dtresp = _MSSQLGateway.ExecuteProcedure(Convert.ToString("get_NumberofGrievances_ByPhoneNumber"), param);

                if (dtresp.Rows.Count > 0)
                {
                    _objResponse.response = 1;
                    _objResponse.sys_message = "success";
                    _objResponse.data = dtresp;
                    _objResponse.response_code = "ACPT.";
                    return _objResponse;
                }
                else
                {
                    _objResponse.response = 1;
                    _objResponse.sys_message = "failed";
                    _objResponse.data = "Data not Found" ;
                    _objResponse.response_code = "REJC.";
                    return _objResponse;
                }

            }
            catch (Exception ex)
            {

                _objResponse.response = 1;
                _objResponse.sys_message = "Fail";
                _objResponse.data = ex.Message;
                _objResponse.response_code = "REJC.";
                return _objResponse;
            }
        }
        #endregion
    }
}