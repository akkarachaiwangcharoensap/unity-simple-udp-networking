using System;
using UnityEngine;

namespace UDP.Clients
{
    public class ClientMoveData
    {
        /**
         * <summary>
         * Direction
         * </summary>
         */
        public string Direction { set; get; }

        /**
         * <summary>
         * Speed
         * </summary>
         */
        public int Speed { set; get; }

        /**
         * <summary>
         * Position
         * </summary>
         */
        public Vector3 Position { set; get; }
    }
}