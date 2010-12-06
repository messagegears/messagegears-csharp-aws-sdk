using System;
namespace MessageGearsAws
{
	/// <summary>
	/// Class used to store all of the properties and credentials needed to interface with your Amazon Web Services Account.
	/// </summary>
	public class MessageGearsAwsProperties
	{
		/// <summary>
		/// The Amazon Web Services Canonical Id for the MessageGears AWS account.
		/// This value is needed to assign S3 permissions to the MessageGears account.
		/// </summary>
		public String MessageGearsAWSCanonicalId { get; set; }
		
		/// <summary>
		/// The Amazon Web Services Account Id for the MessageGears AWS account.
		/// This value is needed to assign SQS permissions to the MessageGears account so it may send messages to a queue.
		/// </summary>
		public String MessageGearsAWSAccountId { get; set; }
		
		/// <summary>
		/// Your Amazon Web Services Account Key.  This value is never sent to MessageGears and is only used to access your S3 and SQS resources in Amazon.
		/// </summary>
		public String MyAWSAccountKey { get; set; }
		
		/// <summary>
		/// Your Amazon Web Services Secret Key.  This value is never sent to MessageGears and is only used to access your S3 and SQS resources in Amazon.
		/// </summary>
		public String MyAWSSecretKey { get; set; }
		
		/// <summary>
		/// This is a very important setting.  When a batch of messages is retrieved by a poller, you will have this many seconds to process the entire batch
		/// before the messages will show up again on the queue.  It is recommended that you leave this number very large to allow for slowdowns in your system
		/// without introducing the risk of duplicate messages being received.
		/// </summary>
		public int SQSVisibilityTimeoutSecs { get; set; }

		/// <summary>
		/// Dumps out all of the properties.
		/// </summary>
		/// <returns>
		/// A string containing a list of all the properties and their associated values.
		/// </returns>
		public override String ToString()
		{
			String dump = " MessageGearsAWSCanonicalId=" + MessageGearsAWSCanonicalId;
			dump = dump + " MessageGearsAWSAccountId=" + MessageGearsAWSAccountId;
			dump = dump + " MyAWSAccountKey=" + MyAWSAccountKey;
			dump = dump + " MyAWSSecretKey=" + "<hidden>";
			return dump;
		}
	}
}

