using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    //ported from https://github.dev/awslabs/amazon-sqs-java-temporary-queues-client/blob/master/src/main/java/com/amazonaws/services/sqs/AmazonSQSRequester.java

    public class AmazonSQSRequesterClient 
    {
        public string QueuePrefix { get; set; }
        public Dictionary<string, string> QueueAttributes;

        private AmazonSQSClient _sqsClient;

        public AmazonSQSRequesterClient(AmazonSQSClient sqsClient, string queuePrefix, Dictionary<string,string> queueAttributes)
        {
            _sqsClient = sqsClient;
            QueuePrefix = queuePrefix;

            if(queueAttributes != null)
                QueueAttributes = queueAttributes;
        }


        public Message sendMessageAndGetResponse(SendMessageRequest request, int timeout, TimeUnit unit)
        {
            throw new NotImplementedException();
        }

        public async Task<string> sendMessageAndGetResponseAsync(SendMessageRequest request)
        {
            string retVal = string.Empty;

            string queueName = QueuePrefix + Guid.NewGuid().ToString();
            CreateQueueRequest createQueueRequest = new CreateQueueRequest()
            {
                QueueName = queueName,
                Attributes = QueueAttributes
            };

            CreateQueueResponse createQueueResult = await _sqsClient.CreateQueueAsync(createQueueRequest);
            string queueUrl = createQueueResult.QueueUrl;
            request.QueueUrl = queueUrl;
            SendMessageResponse response = await _sqsClient.SendMessageAsync(request);


            if(HttpStatusCode.OK == response.HttpStatusCode)
            {
                bool notFound = true;

                while(notFound)
                {
                    ReceiveMessageResponse receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(queueUrl);

                    if (HttpStatusCode.OK == receiveMessageResponse.HttpStatusCode)
                    {
                        if (receiveMessageResponse.Messages.Count > 0)
                        {
                            foreach (var message in receiveMessageResponse.Messages)
                            {
                                if (message.Body.Contains("OCR-COMPLETE"))
                                {
                                    notFound = false;

                                    Console.WriteLine("Value returned:" + message.Body);
                                    retVal = message.Body;

                                    var deleteReq = new DeleteMessageRequest
                                    {
                                        QueueUrl = queueUrl,
                                        ReceiptHandle = message.ReceiptHandle
                                    };
                                    var deleteResp = await _sqsClient.DeleteMessageAsync(deleteReq);

                                    if (deleteResp.HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        throw new Exception("Error attempting to delete message:" + message.ReceiptHandle);

                                    Console.WriteLine("Message Deleted: " + message.ReceiptHandle);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Error receiving message");
                    }

                    Thread.Sleep(5000);
                }
            }
            else
            {
                throw new Exception("failed to send message.");
            }
            //SendMessageRequest sendMessageRequest = SQSQue

            return retVal;
        }

        public void shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
