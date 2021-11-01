using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;

// To interact with Amazon S3.
using Amazon.S3;
using Amazon.S3.Model;

namespace S3CreateAndList
{
    class Program
    {

        private static AnonymousAWSCredentials _awsCreds;
        private static AmazonS3Config _s3Config;
        private static AmazonS3Client _s3Client;
        private static Stack<FileInfo> _stackFiles;

        private const string BucketName = "MrBucket";
        private const int NumFiles = 20;
        private const string TmpDir = @"M:\GitHub\awsInnovation\src\awsInnovation\ClientApp\tmp\";

        private static S3Region BucketRegionEast = S3Region.USEast1;

        // Main method
        static async Task Main()
        {
            Cleanup();
            Configure();
            await CheckBucket();
            MakeTmpFiles();
            await ProcessFilesInLoop(10);
            
        }

        private static void Configure()
        {
            _awsCreds = new AnonymousAWSCredentials();
            
            _s3Config = new AmazonS3Config
            {
                ServiceURL = "http://localhost:4572/",
                ForcePathStyle = true,
                UseHttp = true,
                Timeout = TimeSpan.FromSeconds(60)
            };
            
            _s3Client = new AmazonS3Client(_awsCreds, _s3Config);
            _stackFiles = new Stack<FileInfo>();            
        }

        private static async Task CheckBucket()
        {
            ListBucketsResponse listBuckets = await _s3Client.ListBucketsAsync();
            Console.WriteLine("Number of Buckets: " + listBuckets.Buckets.Count);
            if(!listBuckets.Buckets.Exists(x=>x.BucketName == BucketName.ToLower()))
            {
                PutBucketRequest putBucket = new PutBucketRequest()
                {
                    BucketName = BucketName,
                    BucketRegion = BucketRegionEast
                };

                Console.WriteLine("Creating bucket : " + BucketName);
                PutBucketResponse resp = await _s3Client.PutBucketAsync(putBucket);
                if (resp.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception("Needed a bucket, failed to make one.");
            }
            else
            {
                Console.WriteLine("Bucket already exists: " + BucketName);
            }
        }

        private static void MakeTmpFiles()
        {
            for(int i=0; i<NumFiles; i++)
            {
                string path = string.Format("{0}testFile_{1}.txt", TmpDir, i);
                File.WriteAllText(path, path);
                _stackFiles.Push(new FileInfo(path));
            }
        }

        private static void Cleanup()
        {
            Directory.Delete(TmpDir,true);
            Directory.CreateDirectory(TmpDir);
        }

        private static Task ProcessFilesInLoop(int waitTimeSeconds)
        {
            while(true)
            {
                FileInfo fileInfo = _stackFiles.Pop();
                PutObjectRequest request = new PutObjectRequest()
                {
                    FilePath = fileInfo.FullName,
                    BucketName = BucketName,
                    Key = fileInfo.Name,
                    ContentType = "text/plain"
                };
                
                Console.WriteLine("Sending file to S3: " + fileInfo.FullName);
                Task<PutObjectResponse> response = _s3Client.PutObjectAsync(request);
                response.Wait();
                

                if (HttpStatusCode.OK != response.Result.HttpStatusCode)
                    throw new Exception("Tried to save file to S3, failed:" + fileInfo.FullName);

              
                Thread.Sleep(1000 * waitTimeSeconds);
                File.Delete(fileInfo.FullName);
            }

        }

    }
}
