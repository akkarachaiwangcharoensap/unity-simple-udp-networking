using System;

namespace UDP.Clients
{
    public class ClientCommand
    {
        /**
         * <summary>
         * Client command id
         * </summary
         */
        public ClientCommandID ID { set; get; }

        /**
         * <summary>
         * Data (Json) string
         * </summary>
         */
        public string Data { set; get; }
    }
}