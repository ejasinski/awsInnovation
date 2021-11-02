using Amazon.Runtime;
using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQSTwoWayQueue
{
    public class Messenger
    {

        private AnonymousAWSCredentials _awsCreds;   
        private AmazonSQSConfig _sqsConfig;        
        private AmazonSQSClient _sqsClient;

        public Messenger()
        {

        }

    }
}
