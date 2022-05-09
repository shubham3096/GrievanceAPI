namespace RabbitMQservice
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using EnterpriseSupportLibrary;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class ConsumeRabbitMQHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private MSSQLGateway _MSSQLGateway;
        private CommonHelper _objHelper = new CommonHelper();
        string RBQ;
        private IConfiguration _configuration;
        public ConsumeRabbitMQHostedService(ILoggerFactory loggerFactory, IHostingEnvironment env, IConfiguration configuration)
        {
            
           
            this._configuration = configuration;
            if (env.IsDevelopment())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Dev"));
                this.RBQ = this._configuration["AppSettings_Dev:RBQ"];
            }
            else if (env.IsStaging())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Stag"));
                this.RBQ = this._configuration["AppSettings_Stag:RBQ"];
            }
            else if (env.IsProduction())
            {
                this._MSSQLGateway = new MSSQLGateway(this._configuration.GetConnectionString("Connection_Pro"));
                this.RBQ = this._configuration["AppSettings_Pro:RBQ"];
            }
                this._logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedService>();
            InitRabbitMQ();
        } 

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = "13.71.125.187", Port = 30183, Password = "eaadmin", UserName = "guest" };

            // create connection
            _connection = factory.CreateConnection();

            // create channel
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("grievancepass", ExchangeType.Topic);
            _channel.QueueDeclare(this.RBQ, true, false, false, null);
            // _channel.QueueBind("Grievance_Queue_Dev", "grievancepass", "demo.queue.*", null);
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message
                var content = System.Text.Encoding.UTF8.GetString(ea.Body);

                // handle the received message
                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(this.RBQ, false, consumer);
            return Task.CompletedTask;
        }

        private void HandleMessage(string content)
        {
            // we just print this message 
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("grievanceID", Convert.ToString(content.Trim())));
            DataTable dtresp = _MSSQLGateway.ExecuteProcedure("APP_SHIFT_GRIEVANCES_QUEUE_TO_MAIN", Parameters);
            if (_objHelper.checkDBResponse(dtresp))
            {
                System.Console.WriteLine(" [x] Done");


            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"connection shut down {e.ReplyText}");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            //  _logger.LogInformation($"consumer cancelled {e.ConsumerTag}");
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
            //  _logger.LogInformation($"consumer unregistered {e.ConsumerTag}");
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
            //   _logger.LogInformation($"consumer registered {e.ConsumerTag}");
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"consumer shutdown {e.ReplyText}");
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
