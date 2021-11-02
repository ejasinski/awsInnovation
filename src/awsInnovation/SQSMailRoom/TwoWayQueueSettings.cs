using System.Collections.Generic;

namespace SQSTwoWayQueue
{
    public class TwoWayQueueSettings
    {
        public const string appNameClientApp = "client";
        public const string appNameOrchestration = "orchestration";
        public const string appNameOCR = "ale";

        public const string prefixClientApp = "client_";
        public const string prefixOrchestration = "orch_";
        public const string prefixOCR = "ocr_";

        public static Dictionary<string, string> GetAppPrefixMap()
        {
            return new Dictionary<string, string>() {
                { appNameClientApp, prefixClientApp },
                { appNameOrchestration, prefixOrchestration },
                { appNameOCR, prefixOCR }
                };
        }
    }
}