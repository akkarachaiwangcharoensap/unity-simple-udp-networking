using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Threading;

using UDP.Clients;
using UDP.Utilities.Threads;
using UDP.Servers;

namespace UDP.Networks
{
    public class UDPGameObject : MonoBehaviour
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
         * Game object data
         * </summary>
         */
        public UDPGameObjectData Data { set; get; }

        /**
         * <summary>
         * UDP client
         * </summary>
         */
        private UdpClient client;

        /**
         * <summary>
         * Is the response from the server validated
         * </summary>
         */
        private bool validated = false;

        // Use this for initialization
        void Start()
        {
            this.client = new UdpClient(9051);
            //this.client.Connect(IPAddress.Any, 9050);

            this.listenerThread = new Thread(new ThreadStart(this.Listener));
            this.listenerThread.Start();
        }

        // Update is called once per frame
        private void Update ()
        {
            this.OnKeypress();
        }

        /**
         * <summary>
         * On keypress
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        private void OnKeypress()
        {
            // Move up
            if (Input.GetKey(KeyCode.W))
            {
                this.SendMoveCommand("up", 1);
            }

            // Move left
            else if (Input.GetKey(KeyCode.A))
            {
                this.SendMoveCommand("left", 1);
            }

            // Move down
            else if (Input.GetKey(KeyCode.S))
            {
                this.SendMoveCommand("down", 1);

            }

            // Move right
            else if (Input.GetKey(KeyCode.D))
            {
                this.SendMoveCommand("right", 1);
            }
        }

        /**
         * <summary>
         * Send a move command to the server
         * </summary>
         *
         * <param name="direction"></param>
         * <param name="speed"></param>
         * 
         * <returns>
         * void
         * </returns>
         */
        private void SendMoveCommand(string direction, int speed)
        {
            // Make connection to the server
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 9050);

            try
            {
                //string json = "[{\"command\": \"move-left\"}]";
                ClientCommand command = new ClientCommand();

                // Building client data object
                ClientMoveData clientData = new ClientMoveData();

                clientData.Position = this.transform.position;

                clientData.Direction = direction;
                clientData.Speed = speed;

                // Convert data into json string
                string data = JsonConvert.SerializeObject(
                    clientData,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }
                );

                // Convert command into json string
                command.ID = ClientCommandID.MOVE;
                command.Data = data;

                string json = JsonConvert.SerializeObject(command);

                // Convert json string to bytes and send over to the server.
                Byte[] sendBytes = Encoding.ASCII.GetBytes(json);
                this.client.Send(sendBytes, sendBytes.Length, endPoint);
                //this.client.Send(sendBytes, sendBytes.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        /**
         * <summary>
         * Listen from the server response
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        private void Listener()
        {
            lock (lockObject)
            {
                try
                {
                    while (true)
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9051);
                        Debug.Log("WAITING TO RECEIVE");

                        Byte[] receiveBytes = this.client.Receive(ref endPoint);
                        Debug.Log("RECEIVING...");

                        string response = Encoding.ASCII.GetString(receiveBytes);

                        //bool validate = Validator.IsValidJson(response);
                        // Is response validated.
                        //if (!validate)
                        //{
                        //    return;
                        //}

                        // Convert json to object
                        ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response);

                        // Process the response
                        this.ProcessServerResponse(serverResponse);

                        //System.Threading.Thread.Sleep(20);
                        Debug.Log("Updating the game object properties...");

                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    this.listenerThread.Abort();
                    this.client.Close();
                }
            }
        }

        /**
         * <summary>
         * Process the server response to the client
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        private void ProcessServerResponse(ServerResponse response)
        {
            switch (response.ID)
            {
                case ServerResponseID.POSITION:

                    ServerMoveDataResponse positionData = JsonConvert.DeserializeObject<ServerMoveDataResponse>(response.Data);
                    Dispatcher.RunOnMainThread(delegate() { this.SyncPosition(positionData.Position); });

                    break;
            }
        }

        /**
         * <summary>
         * Synchronize the position based on the response receiving from the server
         * </summary>
         *
         * <param name="position"></param>
         * 
         * <returns>
         * void
         * </returns>
         */
        private void SyncPosition (Vector3 position)
        {
            this.transform.position = position;
        }

        /**
         * <summary>
         * On application quit
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        private void OnApplicationQuit()
        {
            this.client.Close();
            this.listenerThread.Abort();
        }
    }
}
