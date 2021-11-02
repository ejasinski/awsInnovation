using System;

namespace SQSTwoWayQueue
{
    public interface IEnvelope
    {
        MessageLabel MessageLabel { get; set; }
        string MessageBody { get; set; }


        void ExecuteBeforeSend();

        void ExecuteAfterSend();

        void ExecuteOnReceive();

    }
}
