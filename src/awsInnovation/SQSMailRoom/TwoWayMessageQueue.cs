using Amazon.SQS;
using Amazon.SQS.Model;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SQSTwoWayQueue
{
    public static class TwoWayMessageQueue
    {
        private const int _tokenTimeout = 10000;
        private const int _sleepAfterSendingMessage = 1000;
        private const int _sleepBetweenQueueChecks = 1000;

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
                sendMessageRequest.MessageAttributes.Add("to", new MessageAttributeValue() { StringValue = messageDestination, DataType = "String" });
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

                if (receiveMessageResponse.Messages.Count > 0)
                {
                    foreach (Message msg in receiveMessageResponse.Messages)
                    {
                        if (msg.Attributes.TryGetValue("to", out string toValue))
                        {
                            if (toValue == messageOrigin)
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

        public static async Task<Message[]> SendMessageAndGetResponseAsync(AmazonSQSClient sqsClient, IEnvelope envelope)
        {
            string retVal = string.Empty;
            string queueURL = await CreateTempQueueAsync(sqsClient, envelope.MessageLabel.GetQueueName());
            bool messageSent = await SendMessageUsingTempQueue(sqsClient, queueURL, envelope.MessageBody, envelope.MessageLabel.FromAppName, envelope.MessageLabel.ToAppName);

            if (messageSent)
            {
               return await ListenForResponseMessage(sqsClient, queueURL, envelope.MessageLabel.FromAppName);               
            }
            else
            {
                return null;
            }
        }

        public static async Task<Dictionary<string, Message>> CheckForMessagesByApp(AmazonSQSClient sqsClient, string appName)
        {                 
            ListQueuesRequest listQueuesRequest = new ListQueuesRequest(TwoWayQueueSettings.GetPrefixByAppName(appName));
            ListQueuesResponse listQueuesResponse = await sqsClient.ListQueuesAsync(listQueuesRequest);
            Dictionary<string, Message> retVal = new Dictionary<string, Message>();

            if (listQueuesResponse?.QueueUrls?.Count == 0)
                return retVal;

            foreach (string queueURL in listQueuesResponse.QueueUrls)
            {
                ReceiveMessageResponse receiveMessageResponse = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest() { QueueUrl = queueURL, AttributeNames = new List<string> { "to", "from" }});                
                foreach (Message message in receiveMessageResponse.Messages)
                {
                    retVal.Add(queueURL, message);                 
                }
            }

            return retVal;
        }
    }
}