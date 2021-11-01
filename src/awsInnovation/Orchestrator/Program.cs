using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
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
            var receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = Shared.Constants.SQSUrl;
            while (true)
            {
                receiveMessageRequest.WaitTimeSeconds = 15;
                ReceiveMessageResponse receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);
                if (receiveMessageResponse.Messages.Count == 0)
                {
                    continue;
                }

                foreach (var message in receiveMessageResponse.Messages)
                {
                    Console.WriteLine(message.Body);
                    var deleteReq = new DeleteMessageRequest
                    {
                        QueueUrl = Shared.Constants.SQSUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };
                    var deleteResp = await _sqsClient.DeleteMessageAsync(deleteReq);

                    if (deleteResp.HttpStatusCode != System.Net.HttpStatusCode.OK)
                        throw new Exception("Error attempting to delete message:" + message.ReceiptHandle);

                    Console.WriteLine("Message Deleted: " + message.ReceiptHandle);
                }
            }
        }

        private static void Configure()
        {
            _awsCreds = new AnonymousAWSCredentials();

            _sqsConfig = new AmazonSQSConfig
            {
                ServiceURL = Shared.Constants.ServiceUrl
            };
            _sqsClient = new AmazonSQSClient(_awsCreds, _sqsConfig);
        }
    }
}
