using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPInfrastructure.Implementations
{
    public class AWS3Service
    {
        private readonly IAmazonS3 _awsClient;
        private readonly string _bucketName;

        public AWS3Service(IAmazonS3 awsClient, string bucketName)
        {
            _awsClient = awsClient;
            _bucketName = bucketName;
        }
    }
}
