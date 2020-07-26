using System;

namespace UDP.Servers
{
    public class ServerResponse
    {
        /**
         * <summary>
         * Server response ID
         * </summary
         */
        public ServerResponseID ID { set; get; }

        /**
         * <summary>
         * Data (Json) string
         * </summary>
         */
        public string Data { set; get; }
    }
}