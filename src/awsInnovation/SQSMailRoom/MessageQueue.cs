using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SQSTwoWayQueue
{
    public static class MessageQueue
    {
        private const int _tokenTimeout = 10000;
        private const int _sleepAfterSendingMessage = 30000;
        private const int _sleepBetweenQueueChecks = 1000;

        private static TaskFactory<List<Message>> _factory;

        /// <summary>
        /// Create a queue and return the URL
        /// </summary>
        /// <param name="sqsClient">sqsClient</param>
        /// <param name="queueName">name of the queue to create</param>
        /// <returns>URL for the created queue</returns>
        public static async Task<string> CreateTempQueueAsync(AmazonSQSClient sqsClient, string queueName)
        {
            string retVal = null;

            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                tokenSource.CancelAfter(_tokenTimeout);
                CreateQueueResponse createQueueResult = await sqsClient.CreateQueueAsync(new CreateQueueRequest() { QueueName = queueName }, tokenSource.Token);
                retVal = createQueueResult.QueueUrl;
            }

            return retVal;
        }

        private static async Task<bool> SendMessageUsingTempQueue(AmazonSQSClient sqsClient, string queueURL, string message, string messageOrigin, string messageDestination)
        {
            bool retVal = false;

            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                SendMessageRequest sendMessageRequest = new SendMessageRequest() { QueueUrl = queueURL, MessageBody = message };
                sendMessageRequest.MessageAttributes.Add("to", new MessageAttributeValue() {StringValue = messageDestination, DataType = "String" } );
                sendMessageRequest.MessageAttributes.Add("from", new MessageAttributeValue() { StringValue = messageOrigin, DataType = "String" });
                SendMessageResponse sendMessageResponse = await sqsClient.SendMessageAsync(sendMessageRequest, tokenSource.Token);
                retVal = HttpStatusCode.OK == sendMessageResponse.HttpStatusCode;
            }

            return retVal;
        }

        public static async Task<Amazon.SQS.Model.Message[]> ListenForResponseMessage(AmazonSQSClient sqsClient, string queueURL, string messageOrigin)
        {
            Thread.Sleep(_sleepAfterSendingMessage);
            bool notFound = true;
            List<Message> retVal = new List<Message>();

            while (notFound)
            {
                ReceiveMessageResponse receiveMessageResponse = await sqsClient.ReceiveMessageAsync(queueURL);
                
                if(receiveMessageResponse.Messages.Count>0)
                {
                    foreach(Message msg in receiveMessageResponse.Messages)
                    {
                        if(msg.Attributes.TryGetValue("to", out string toValue))
                        {
                            if(toValue == messageOrigin)
                            {
                                notFound = false;
                                retVal.Add(msg);                               
                            }
                        }
                    }
                }

                Thread.Sleep(_sleepBetweenQueueChecks);
            }

            return retVal.ToArray();
        }



        public static async Task<string[]> SendMessageAndGetResponseAsync(AmazonSQSClient sqsClient, IEnvelope envelope, string messageOrigin, string messageDestination)
        {
            string retVal = string.Empty;
            string queueURL = await CreateTempQueueAsync(sqsClient, envelope.MessageLabel.GetQueueName());
            bool messageSent = await SendMessageUsingTempQueue(sqsClient, queueURL, envelope.MessageBody, messageOrigin, messageDestination);

            if (messageSent)
            {
                Task<Message[]> task = ListenForResponseMessage(sqsClient, queueURL, messageOrigin);
                task.Wait();
                Message[] messages = task.Result;

                List<string> results = new List<string>();
                
                foreach(Message msg in messages)
                {
                    results.Add(msg.Body);
                }
                return results.ToArray();
             }
            else
            {
                return null;
            }
        }
           
    }
}
