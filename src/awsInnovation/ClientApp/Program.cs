using Amazon.Runtime;

// To interact with Amazon S3.
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Shared;
using SQSTwoWayQueue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace S3CreateAndList
{
    internal class Program
    {
        private static AnonymousAWSCredentials _awsCreds;
        private static AmazonS3Config _s3Config;
        private static AmazonSQSConfig _sqsConfig;
        private static AmazonS3Client _s3Client;
        private static AmazonSQSClient _sqsClient;


        private static Stack<FileInfo> _stackFiles;
        private static S3Region BucketRegionEast = S3Region.USEast1;

        private static string QueueURL = null;

        // Main method
        private static async Task Main()
        {
            Cleanup();
            Configure();
            await CheckBucket();
            await CheckSQSQueue();
            MakeTmpFiles();
            await ProcessFilesInLoop(10);
        }

        private static void Configure()
        {
            _awsCreds = new AnonymousAWSCredentials();

            _s3Config = new AmazonS3Config
            {
                ServiceURL = Shared.Constants.ServiceUrl,
                ForcePathStyle = true,
                UseHttp = true,
                Timeout = TimeSpan.FromSeconds(60)
            };
            _s3Client = new AmazonS3Client(_awsCreds, _s3Config);

            _sqsConfig = new AmazonSQSConfig
            {
                ServiceURL = Shared.Constants.ServiceUrl
            };
            _sqsClient = new AmazonSQSClient(_awsCreds, _sqsConfig);
            _stackFiles = new Stack<FileInfo>();
        }

        private static async Task CheckBucket()
        {
            ListBucketsResponse listBuckets = null;

            bool existsBucket = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, Shared.Constants.BucketName);

            if (!existsBucket)
                await CreateDefaultBucket();

            existsBucket = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, Shared.Constants.BucketName);

            if (!existsBucket)
            {
                throw new Exception("Failed to create the default bucket.");
            }
        }

        private static async Task CreateDefaultBucket()
        {
            PutBucketRequest putBucket = new PutBucketRequest()
            {
                BucketName = Shared.Constants.BucketName,
                BucketRegion = BucketRegionEast,
                BucketRegionName = "us-east-1"
            };

            Console.WriteLine("Creating bucket : " + Shared.Constants.BucketName);
            PutBucketResponse resp = await _s3Client.PutBucketAsync(putBucket);

            if (resp.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception("Needed a bucket, failed to make one.");
        }

        private static void MakeTmpFiles()
        {
            for (int i = 0; i < Shared.Constants.NumFiles; i++)
            {
                string path = string.Format("{0}testFile_{1}.txt", Shared.Constants.TmpDir, i);
                File.WriteAllText(path, path);
                _stackFiles.Push(new FileInfo(path));
            }
        }

        private static void Cleanup()
        {
            Directory.Delete(Shared.Constants.TmpDir, true);
            Directory.CreateDirectory(Shared.Constants.TmpDir);
        }

        private static async Task CheckSQSQueue()
        {
            var createQueueRequest = new CreateQueueRequest();
            try
            {
                GetQueueUrlResponse getQueueUrlResponse = await _sqsClient.GetQueueUrlAsync(Shared.Constants.SQSQueueName);
                QueueURL = getQueueUrlResponse.QueueUrl;
            }
            catch
            {
                try
                {
                    // create channel
                    createQueueRequest.QueueName = Shared.Constants.SQSQueueName;
                    var createQueueResponse = await _sqsClient.CreateQueueAsync(createQueueRequest);
                    Console.WriteLine("QueueUrl : " + createQueueResponse.QueueUrl);
                    QueueURL = createQueueResponse.QueueUrl;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static async Task ProcessFilesInLoop(int waitTimeSeconds)
        {
            while (true)
            {
                FileInfo fileInfo = _stackFiles.Pop();
                PutObjectRequest request = new PutObjectRequest()
                {
                    FilePath = fileInfo.FullName,
                    BucketName = Shared.Constants.BucketName,
                    Key = fileInfo.Name,
                    ContentType = "text/plain"
                };

                Console.WriteLine("Sending file to S3: " + fileInfo.FullName);
                Task<PutObjectResponse> response = _s3Client.PutObjectAsync(request);
                response.Wait();

                if (HttpStatusCode.OK != response.Result.HttpStatusCode)
                    throw new Exception("Tried to save file to S3, failed:" + fileInfo.FullName);
                else
                {
                    Console.WriteLine("File saved to S3 Successfully: " + fileInfo.FullName);

                    //was working
                    // string sequenceNum = await SendSQSMessage(response.Result.ETag);


                    ModelS3Upload modelS3Upload = new ModelS3Upload()
                    {
                        ProcessJob = "OCR"
                    };

                    string json = JsonConvert.SerializeObject(modelS3Upload);

                    Envelope envelope = new Envelope()
                    {
                        MessageLabel = new MessageLabel()
                        {
                            FromAppName = TwoWayQueueSettings.appNameClientApp,
                            ToAppName = TwoWayQueueSettings.appNameOrchestration
                        },
                        MessageBody = json
                    };

                    Message[] responseMessages = await TwoWayMessageQueue.SendMessageAndGetResponseAsync(_sqsClient, envelope);
                   


                }

                Thread.Sleep(1000 * waitTimeSeconds);
                File.Delete(fileInfo.FullName);
            }
        }

        private static async Task<string> SendSQSMessage(string message)
        {
            SendMessageRequest sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = QueueURL,
                MessageBody = message
            };



            SendMessageResponse sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest);

            if (sendMessageResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to send message to SQS");
            }

            return sendMessageResponse.MessageId;

        }

       
    }
}