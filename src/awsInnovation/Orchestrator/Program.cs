using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
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
                Thread.Sleep(250);
                ListQueuesResponse listQueuesResponse = null;
                try
                {
                    ListQueuesRequest listQueuesRequest = new ListQueuesRequest("pre");
                    listQueuesResponse = await _sqsClient.ListQueuesAsync(listQueuesRequest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("uh oh");
                }

                if (listQueuesResponse?.QueueUrls?.Count == 0)
                    continue;

                foreach (string queueURL in listQueuesResponse.QueueUrls)
                {

                    var receiveMessageRequest = new ReceiveMessageRequest();
                    receiveMessageRequest.QueueUrl = queueURL;

                    ReceiveMessageResponse receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);
                    if (receiveMessageResponse.Messages.Count == 0)
                    {
                        continue;
                    }

                    foreach (var message in receiveMessageResponse.Messages)
                    {
                        if (!message.Body.Contains("OCR-COMPLETE"))
                        {
                            Console.WriteLine(message.Body);
                            string response = message.Body.Replace("OCR", "OCR-COMPLETE");

                            SendMessageRequest messageRequest = new SendMessageRequest()
                            {
                                MessageBody = response,
                                QueueUrl = queueURL
                            };
                            SendMessageResponse sendMessageResponse = await _sqsClient.SendMessageAsync(messageRequest);

                            if (HttpStatusCode.OK == sendMessageResponse.HttpStatusCode)
                            {
                                Console.WriteLine("Response returned");
                            }
                        }
                        /*
                        var deleteResp = await _sqsClient.DeleteMessageAsync(deleteReq);

                        if (deleteResp.HttpStatusCode != System.Net.HttpStatusCode.OK)
                            throw new Exception("Error attempting to delete message:" + message.ReceiptHandle);

                        Console.WriteLine("Message Deleted: " + message.ReceiptHandle);
                        */
                    }
                }
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
