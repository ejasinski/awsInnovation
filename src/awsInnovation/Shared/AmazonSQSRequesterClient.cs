using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    //ported from https://github.dev/awslabs/amazon-sqs-java-temporary-queues-client/blob/master/src/main/java/com/amazonaws/services/sqs/AmazonSQSRequester.java

    class AmazonSQSRequesterClient : IAmazonSQSRequesterClient
    {
        public string QueuePrefix { get; set; }
        public Dictionary<string, string> QueueAttributes;

        private AmazonSQSClient _sqsClient;

        public AmazonSQSRequesterClient(AmazonSQSClient sqsClient, string queuePrefix, Dictionary<string,string> queueAttributes)
        {
            _sqsClient = sqsClient;
            QueuePrefix = queuePrefix;
            QueueAttributes = queueAttributes;
        }


        public Message sendMessageAndGetResponse(SendMessageRequest request, int timeout, TimeUnit unit)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> sendMessageAndGetResponseAsync(SendMessageRequest request, int timeout, TimeUnit unit)
        {
            string queueName = QueuePrefix + Guid.NewGuid().ToString();
            CreateQueueRequest createQueueRequest = new CreateQueueRequest()
            {
                QueueName = queueName,
                Attributes = QueueAttributes
            };

            CreateQueueResponse createQueueResult = await _sqsClient.CreateQueueAsync(createQueueRequest);
            string queueUrl = createQueueResult.QueueUrl;

            //SendMessageRequest sendMessageRequest = SQSQue

        }

        public void shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
