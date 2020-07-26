using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

using UDP.Clients;
using UDP.Utilities;
using UDP.Utilities.Threads;

namespace UDP.Servers
{
    public class UDPServer : MonoBehaviour
    {
        /**
         * <summary>
         * Lock object
         * </summary>
         */
        static readonly object lockObject = new object();

        /**
         * <summary>
         * Listener thread
         * </summary>
         */
        private Thread listenerThread;

        /**
         * <summary>
         * UDP Server
         * </summary>
         */
        private UdpClient server;

        private void Awake()
        {
            this.listenerThread = new Thread(new ThreadStart(this.Listener));
            this.listenerThread.Start();
        }

        // Start is called before the first frame update
        private void Start()
        {
            //this.listenerThread = new Thread(new ThreadStart(this.Listener));
            //this.listenerThread.Start();
        }

        // Update is called once per frame
        private void Update()
        {

        }

        /**
         * <summary>
         * Start listening for broadcast
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        private void Listener()
        {
            int port = 9050;
            this.server = new UdpClient(port);

            lock (lockObject)
            {
                try
                {
                    while (true)
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

                        Debug.Log("Waiting for broadcast...");
                        Byte[] receiveBytes = this.server.Receive(ref endPoint);

                        string response = Encoding.ASCII.GetString(receiveBytes);
                        bool validate = Validator.IsValidJson(response);

                        // Is response validated.
                        if (!validate)
                        {
                            return;
                        }

                        ClientCommand command = JsonConvert.DeserializeObject<ClientCommand>(response);
                        Debug.Log($"Receiving {response} from : {endPoint}");

                        this.ProcessClientCommand(command);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    this.server.Close();
                    this.listenerThread.Abort();
                }
            }
        }

        /**
         * <summary>
         * Sending a new server response to the client
         * </summary>
         *
         * <param name="response"></param>
         * 
         * <returns>
         * void
         * </returns>
         */
        private void SendToClient (ServerResponse response)
        {
            // Client end point
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9051);

            try
            {
                string newUpdates = JsonConvert.SerializeObject(response);

                // Convert json string into bytes and send to the client
                Byte[] sendBytes = Encoding.ASCII.GetBytes(newUpdates);
                this.server.Send(sendBytes, sendBytes.Length, endPoint);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        /**
         * <summary>
         * Process the client commands
         * </summary>
         *
         * <param name="command"></param>
         * 
         * <returns>
         * void
         * </returns>
         */
        private void ProcessClientCommand (ClientCommand command)
        {
            switch (command.ID)
            {
                case ClientCommandID.MOVE:

                    ClientMoveData data = JsonConvert.DeserializeObject<ClientMoveData>(command.Data);

                    Vector3 position = data.Position;
                    string direction = data.Direction;
                    int speed = data.Speed;

                    // Move the game object
                    //this.Move(position, direction, speed);
                    Dispatcher.RunOnMainThread(delegate () { this.Move(position, direction, speed); });

                    break;
            }
        }

        /**
         * <summary>
         * Move the game object by the given direction and speed
         * </summary>
         *
         * <param name="position"></param>
         * <param name="direction"></param>
         * <param name="speed"></param>
         * 
         * <returns>
         * void
         * </returns>
         */
        private void Move (Vector3 position, string direction, int speed)
        {
            // Move left
            if (direction.Equals("left"))
            {
                 position = new Vector2(
                    position.x - speed * Time.deltaTime,
                    position.y
                );
            }
            // Move right
            else if (direction.Equals("right"))
            {
                position = new Vector2(
                    position.x + speed * Time.deltaTime,
                    position.y
                );
            }
            // Move up
            else if (direction.Equals("up"))
            {
                position = new Vector2(
                    position.x,
                    position.y + speed * Time.deltaTime
                );
            }
            // Move down
            else if (direction.Equals("down"))
            {
                position = new Vector2(
                    position.x,
                    position.y - speed * Time.deltaTime
                );
            }

            // Building move data data
            ServerMoveDataResponse data = new ServerMoveDataResponse();
            data.Position = position;

            // Building response object
            ServerResponse response = new ServerResponse();
            response.ID = ServerResponseID.POSITION;

            response.Data = JsonConvert.SerializeObject(
                data,
                Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }
            );

            this.SendToClient(response);
        }

        /**
         * <summary>
         * On application finished. Clear threads and cleanups.
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        private void OnApplicationQuit()
        {
            this.server.Close();
            this.listenerThread.Abort();
        }
    }
}