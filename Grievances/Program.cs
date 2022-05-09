using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using GrievanceService.Models;
using EnterpriseSupportLibrary;
using RabbitMQservice;
using System.Threading;

namespace GrievanceService
{ 
    public class Program 
    {
        public static string env = string.Empty;
        public static string url = string.Empty;
        
        public static void Main(string[] args)
        {
           
            env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                        
          //  StartBackgroundConsumer(env);

            if (env == "Production")
            {
                url = "http://0.0.0.0:5000";
            }
            else if (env == "Staging")
            {
                url = "http://0.0.0.0:9008"; 
            }
            else if (env == "Development")
            {
                url = "http://0.0.0.0:9007";
            }
            System.Console.WriteLine(env);
            BuildWebHost(args).Run();
        }


        public static IWebHost BuildWebHost(string[] args) =>
            
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()            
               //.UseUrls(url)
                .Build();


        private static void StartBackgroundConsumer(string env)
        {
            //RabbitMQCont rbq = new RabbitMQCont(env);
            //Thread worker = new Thread(rbq.StartConsumer)
            //{
                
            //    IsBackground = true
            //};
            //worker.Start();
        }

    }


}
