using Amazon.SQS.Model;
using Amazon.SQS;
using System.Threading.Tasks;

namespace Shared
{
    public interface IAmazonSQSRequesterClient
    {
        //AmazonSQS getAmazonSQS();


        /**
         * Sends a message and waits the given amount of time for
         * the response message.
         */
        public Message sendMessageAndGetResponse(SendMessageRequest request,
                int timeout, TimeUnit unit);

        /**
         * Sends a message and returns a <tt>CompletableFuture</tt> 
         * that will be completed with the response message when it arrives.
         */
        public Task<Message> sendMessageAndGetResponseAsync(SendMessageRequest request,
                int timeout, TimeUnit unit);

        public void shutdown();


    }
}
