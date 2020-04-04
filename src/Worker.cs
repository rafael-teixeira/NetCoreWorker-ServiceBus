/*
 MICROSOFT CODE DISCLAIMER
This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment.
THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code form of the Sample Code, provided that You agree: (i) to not use Our name, logo, or trademarks to market Your software product in which the Sample Code is embedded; (ii) to include a valid copyright notice on Your software product in which the Sample Code is embedded; and(iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys’ fees, that arise or result from the use or distribution of the Sample Code.
Please note: None of the conditions outlined in the disclaimer above will supersede the terms and conditions contained within the Premier Customer Services Description.
*/
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMSWorker;

//SET ApplicationInsights:InstrumentationKey=putinstrumentationkeyhere 

namespace WindowsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        const string ServiceBusConnectionString = "Endpoint=sb://sb-auxiliocovid-token-prd.servicebus.windows.net/;SharedAccessKeyName=sb-receiver;SharedAccessKey=yLix8SiYmlw91lcV8Uh8lYT0lKL8w7rftxbghkdc+Fw=;";
        const string QueueName = "queueauxiliocovidtokenprd";
        static IQueueClient queueClient;
        private TelemetryClient telemetryClient;

        public Worker(ILogger<Worker> logger, TelemetryClient _telemetryClient)
        {
            _logger = logger;
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            telemetryClient = _telemetryClient;

            RegisterOnMessageHandlerAndReceiveMessages();

            for (int i = 0; i < 100; i++)
            {
                SMSBusSender.Test();
            }
        }


        void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 5,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            RequestTelemetry requestTelemetry = new RequestTelemetry { Name = "Envio SMS para API: "};
            //var rootId = message.UserProperties["RootId"].ToString();
            //var parentId = message.UserProperties["ParentId"].ToString();
            //// Get the operation ID from the Request-Id (if you follow the HTTP Protocol for Correlation).
            //requestTelemetry.Context.Operation.Id = rootId;
            //requestTelemetry.Context.Operation.ParentId = parentId;

            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            var operation = telemetryClient.StartOperation(requestTelemetry);

            try
            {
                
               


                int i = 100 / new Random().Next(0, 5);

                //TODO: Implementar o envio de SMS
                await Task.Delay(1000);

                // Complete the message so that it is not received again.
                // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
                await queueClient.CompleteAsync(message.SystemProperties.LockToken);

                // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
                // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
                // to avoid unnecessary exceptions.
                operation.Telemetry.Success = true;
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;
                telemetryClient.TrackException(ex);
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }

        }

        Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            //Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            //var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            //Console.WriteLine("Exception context for troubleshooting:");
            //Console.WriteLine($"- Endpoint: {context.Endpoint}");
            //Console.WriteLine($"- Entity Path: {context.EntityPath}");
            //Console.WriteLine($"- Executing Action: {context.Action}");

            _logger.LogError(exceptionReceivedEventArgs.Exception, $"Erro ao receber mesangem {exceptionReceivedEventArgs.Exception}", exceptionReceivedEventArgs);

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await Task.Delay(1000, stoppingToken);
            }

        }
    }
}
