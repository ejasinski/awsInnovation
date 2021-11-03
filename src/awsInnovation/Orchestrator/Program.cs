using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator
{
    class Program
    {
        private static AnonymousAWSCredentials _awsCreds;
        private static AmazonSQSConfig _sqsConfig;
        private static AmazonSQSClient _sqsClient;

        static async Task Main(string[] args)
        {
            Configure();
            await ReadMessages();
        }



        public static async Task ReadMessages()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Dictionary<string, Message> keyValuePairs = await SQSTwoWayQueue.TwoWayMessageQueue.CheckForMessagesByApp(_sqsClient, SQSTwoWayQueue.TwoWayQueueSettings.appNameOrchestration);

            }
        }

        private static void Configure()
        {
            _awsCreds = new AnonymousAWSCredentials();

            _sqsConfig = new AmazonSQSConfig
            {
                ServiceURL = Shared.Constants.ServiceUrl,
                Timeout = TimeSpan.FromSeconds(30)
            };
            _sqsClient = new AmazonSQSClient(_awsCreds, _sqsConfig);


        }
    }
}
