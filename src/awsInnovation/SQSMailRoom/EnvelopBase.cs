using System;
using System.Collections.Generic;
using System.Text;

namespace SQSTwoWayQueue
{
    public abstract class EnvelopeBase : IEnvelope
    {
        public MessageLabel MessageLabel { get; set; }

        public string MessageBody { get; set; }

        public void ExecuteAfterSend()
        {
            Console.WriteLine("Sending message.");
        }

        public void ExecuteBeforeSend()
        {
            Console.WriteLine("Sending message complete.");
        }

        public void ExecuteOnReceive()
        {
            Console.WriteLine("Message received.");
        }
    }
}
