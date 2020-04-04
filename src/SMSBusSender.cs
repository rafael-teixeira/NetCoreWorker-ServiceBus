using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SMSWorker
{
    public static class SMSBusSender
    {
        const string ServiceBusConnectionString = "Endpoint=sb://sb-auxiliocovid-token-prd.servicebus.windows.net/;SharedAccessKeyName=sb-sender;SharedAccessKey=BcxRxgCq3uxVILGrRxyAFgQPERfZzUxTvPpRSVW9ZkY=;";
        const string QueueName = "queueauxiliocovidtokenprd";
        static IQueueClient queueClient;

        static SMSBusSender()
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
        }

        static async Task SendMessagesAsync(string messageId, byte[] messageBuffer)
        {
            try
            {
                // Create a new message to send to the queue
                var message = new Message(messageBuffer)
                {
                    ContentType = "application/json",
                    Label = "SMS",
                    MessageId = messageId,
                    TimeToLive = TimeSpan.FromHours(1)
                };

                // Send the message to the queue
                await queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                //Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        public static async Task Test()
        {
            EnvioSMS envio = new EnvioSMS();
            envio.destination = "+5511985356274";
            envio.messageText = "Codigo";
            envio.correlationId = Guid.NewGuid().ToString();
            var conteudo = JsonConvert.SerializeObject(envio);
            var buffer = System.Text.Encoding.UTF8.GetBytes(conteudo);
            await SendMessagesAsync(Guid.NewGuid().ToString(), buffer);
        }
    }
}
