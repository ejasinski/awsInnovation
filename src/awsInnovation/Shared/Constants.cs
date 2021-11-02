using System;

namespace Shared
{
    public class Constants
    {
        public const string BucketName = "mrbucket";

        public const string SQSQueueName = "mrqueue";
        public const string SQSUrl = "http://localhost:4566/000000000000/mrqueue";

        public const int NumFiles = 20;
        public const string TmpDir = @"M:\GitHub\awsInnovation\src\awsInnovation\ClientApp\tmp\";
        public const string ServiceUrl = "http://localhost:4566/";




        public const string RESPONSE_QUEUE_URL_ATTRIBUTE_NAME = "ResponseQueueUrl";
        public const string VIRTUAL_QUEUE_HOST_QUEUE_ATTRIBUTE = "HostQueueUrl";
        public const string IDLE_QUEUE_RETENTION_PERIOD = "IdleQueueRetentionPeriodSeconds";
        public const int MINIMUM_IDLE_QUEUE_RETENTION_PERIOD_SECONDS = 1;
        public const int HEARTBEAT_INTERVAL_SECONDS_DEFAULT = 5;
        public const int HEARTBEAT_INTERVAL_SECONDS_MIN_VALUE = 1;

    }
}
