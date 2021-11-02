using System;
using System.Collections.Generic;
using System.Text;

namespace SQSTwoWayQueue
{
    public class MessageLabel
    {
        public string ToAppName { get; set; }

        public string FromAppName { get; set; }

        public DateTime? TimeSent { get; set; }

        public DateTime? TimeReceived { get; set; }

        public bool Completed { get; set; }

        public MessageLabel()
        {
            ToAppName = null;
            FromAppName = null;
            Completed = false;
            TimeSent = null;
            TimeReceived = null;           
        }

        public string GetQueueName()
        {
            if(TwoWayQueueSettings.GetAppPrefixMap().TryGetValue(ToAppName, out string val))
            {
                return val + Guid.NewGuid().ToString();
            }
            else
            {
                throw new Exception("Could not find a prefix for ToAppName : " + ToAppName);
            }
        }

    }
}
