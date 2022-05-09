using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrievanceService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using EnterpriseSupportLibrary;
using GrievanceService.Helpers;
using System.Data;
using Newtonsoft.Json;
using RabbitMQservice;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace GrievanceService
{
    public class Startup
    {
        private CommonFunctions _objHelperLoc;
        private MSSQLGateway _MSSQLGateway;
        CommonFunctions APICall;
        private IConfiguration _configuration;
        private CommonHelper _objHelper;
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            //RabbitMQCont rbq = new RabbitMQCont(configuration, env);
            //rbq.GetConnection();
            //StartBackgroundConsumer(rbq);

            _objHelperLoc = new CommonFunctions(configuration, env);
            APICall = new CommonFunctions(configuration, env);
            _objHelper = new CommonHelper();
            this._configuration = configuration;
             new CommonHelper();

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
            List<SqlParameter> Parameters = new List<SqlParameter>();
            DataTable _DTResponse = _MSSQLGateway.ExecuteProcedure("Md_Service_Lingual_Labels_Select", Parameters);
            if (_objHelper.checkDBResponse(_DTResponse))
            {   
                string config_output = string.Empty;
                var _ResponseData = _objHelper.ConvertTableToDictionary(_DTResponse);
                config_output = JsonConvert.SerializeObject(_ResponseData);
                System.IO.File.WriteAllText("bilingual_library.json", config_output);
            }


            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region JWT Auth Initialization - DO NOT EDIT -> EDIT PARAMS IN `appsettings.json`
          services.AddHostedService<ConsumeRabbitMQHostedService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = this.Configuration["JWTSetting:Issuer"],
                    ValidAudience = this.Configuration["JWTSetting:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["JWTSetting:Key"]))
                };
            });
            #endregion
            services.AddCors();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Call Exception 
                app.UseExceptionHandler(
                options =>
                {
                    options.Run(
                    async context =>
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";
                        var ex = context.Features.Get<IExceptionHandlerFeature>();
                        if (ex != null)
                        {
                            var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace }";

                            if (!Directory.Exists(Path.Combine(env.ContentRootPath, $"Logs")))
                            {
                                Directory.CreateDirectory(Path.Combine(env.ContentRootPath, $"Logs"));
                            }
                            StreamWriter sw = new StreamWriter(Path.Combine(env.ContentRootPath, $"Logs/{DateTimeOffset.Now.ToString("yyyyMMMdd")}.txt"), true);
                            sw.WriteLine("---------------------" + DateTime.Now.ToString("dd-MMM-yyyy hh:mm ss tt") + "---------------------");
                            sw.WriteLine("[Log Type]: - System Exception");
                            sw.WriteLine("=================================================================");
                            sw.WriteLine(ex.Error.Message.ToString());
                            sw.WriteLine(ex.Error.StackTrace.ToString());
                            sw.WriteLine(ex.Error.Source.ToString());

                            sw.WriteLine("=================================================================");
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                            string message = ex.Error.Message.ToString() + "~" + env.EnvironmentName;
                            await context.Response.WriteAsync("{response: 0, data: null, sys_message: '" + message + "'}").ConfigureAwait(false);
                        }
                    });
                }
            );
            }
            app.UseCors(
              options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );
            //app.Use(async (context, next) =>
            //{
            //    //context.Response.Headers.Add("X-Frame-Options", "DENY"); // This
            //    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN"); // Or this
            //    await next();
            //});
            app.UseAuthentication();
            app.UseMvc();
        }
        // BACKGROUND TAS
        // INITIALIZES RABBIT MQ CONNECTION 

        // INITIATE BACKGROUND TASK FOR RABBIT MQ CONSUMER

        //private static void StartBackgroundConsumer(RabbitMQCont rbq)
        //{
        //    Thread worker = new Thread(rbq.StartConsumer);
        //    worker.IsBackground = true;
        //    worker.Start();
        //}
    }
}
