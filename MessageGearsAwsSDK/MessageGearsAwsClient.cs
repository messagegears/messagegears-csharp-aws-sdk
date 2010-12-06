using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Auth;
using Amazon.Auth.AccessControlPolicy;

using log4net;

using System;

namespace MessageGearsAws
{
	/// <summary>
	/// MessageGears C# SDK main entry point. 
	/// </summary>
	public class MessageGearsAwsClient
	{
		static readonly ILog log = LogManager.GetLogger(typeof(MessageGearsAwsClient));
		MessageGearsAwsProperties properties = null;
		AmazonSQS sqs = null;
		AmazonS3 s3 = null;
		
		/// <summary>
		/// Used to create a new instance of the MessageGears client.
		/// </summary>
		/// <param name="props">
		/// Contains the credentials needed to access MessageGears, Amazon S3, and Amazon SQS.<see cref="MessageGearsProperties"/>
		/// </param>
		public MessageGearsAwsClient(MessageGearsAwsProperties props)
		{
			this.properties = props;
			this.sqs = new AmazonSQSClient(properties.MyAWSAccountKey, properties.MyAWSSecretKey);
			this.s3 = new AmazonS3Client(properties.MyAWSAccountKey, properties.MyAWSSecretKey);
			log.Info("MessageGears AWS client initialized");
		}
						
		/// <summary>
		/// Copies a file to Amazon S3 and grants READ-ONLY access to MessageGears.
		/// </summary>
		/// <param name="fileName">
		/// The fully qualified name of the file to be copied to S3.
		/// </param>
		/// <param name="bucketName">
		/// The name of the S3 bucket where the file will be copied.
		/// </param>
		/// <param name="key">
		/// The S3 key of the file to be created.
		/// </param>
		public void PutS3File(String fileName, String bucketName, String key)
		{
			// Check to see if the file already exists in S3
			ListObjectsRequest listRequest = new ListObjectsRequest().WithBucketName(bucketName).WithPrefix(key);
			ListObjectsResponse listResponse = s3.ListObjects(listRequest);
			if(listResponse.S3Objects.Count > 0)
			{
				String message = "File " + fileName + " already exists.";
				log.Warn("PutS3File failed: " + message);
				throw new ApplicationException(message);
			}
			
			// Copy a file to S3
			PutObjectRequest request = new PutObjectRequest().WithKey(key).WithFilePath(fileName).WithBucketName(bucketName);
			s3.PutObject(request);
			
			// Get the ACL for the file and retrieve the owner ID (not sure how to get it otherwise).
			GetACLRequest getAclRequest = new GetACLRequest().WithBucketName(bucketName).WithKey(key);
			GetACLResponse aclResponse = s3.GetACL(getAclRequest);
			Owner owner = aclResponse.AccessControlList.Owner;
			
			// Create a grantee as the MessageGears account
			S3Grantee grantee = new S3Grantee().WithCanonicalUser(properties.MessageGearsAWSCanonicalId, "MessageGears");

			// Create an new ACL for the file and give MessageGears Read-only access, and the owner full control.
			S3AccessControlList acl = new S3AccessControlList().WithOwner(owner);
			acl.AddGrant(grantee, S3Permission.READ);
			grantee = new S3Grantee().WithCanonicalUser(owner.Id, "MyAWSId");
			acl.AddGrant(grantee, S3Permission.FULL_CONTROL);
			SetACLRequest aclRequest = new SetACLRequest().WithACL(acl).WithBucketName(bucketName).WithKey(key);
			s3.SetACL(aclRequest);
			
			log.Info("PutS3File successful: " + fileName);
		}
		
		/// <summary>
		/// Deletes a file from Amazon S3.
		/// </summary>
		/// <param name="bucketName">
		/// The name of the bucket where the file resides.
		/// </param>
		/// <param name="key">
		/// The key of the file to be deleted.
		/// </param>
		public void DeleteS3File(String bucketName, String key)
		{
			// Copy a file to S3
			DeleteObjectRequest request = new DeleteObjectRequest().WithBucketName(bucketName).WithKey(key);
			s3.DeleteObject(request);
			log.Info("DeleteS3File successful: " + bucketName + "/" + key);

		}
		
		/// <summary>
		/// Creates a new queue in Amazon SQS and grants SendMessage only access to MessageGears.
		/// </summary>
		/// <param name="queueName">
		/// The name of the queue to be created.
		/// </param>
		/// <returns>
		/// The full url of the newly created queue.
		/// </returns>
		public String CreateQueue(String queueName)
		{
			CreateQueueRequest request = new CreateQueueRequest()
				.WithQueueName(queueName)
				.WithDefaultVisibilityTimeout(properties.SQSVisibilityTimeoutSecs);

			CreateQueueResponse response = sqs.CreateQueue(request);
			
			addQueuePermission(response.CreateQueueResult.QueueUrl);
			
			log.Info("Create queue successful: " + queueName);
			
			return response.CreateQueueResult.QueueUrl;
		}
		
		private void addQueuePermission(String queueUrl)
		{
			AddPermissionRequest permissionRequest = new AddPermissionRequest()
				.WithActionName("SendMessage")
				.WithAWSAccountId(properties.MessageGearsAWSAccountId)
				.WithLabel("MessageGears Send Permission")
				.WithQueueUrl(queueUrl);
		
			sqs.AddPermission(permissionRequest);
		}
	}		
}