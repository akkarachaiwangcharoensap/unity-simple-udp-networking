using UnityEngine;
using System.Collections;
using System.Net;
using UDP.Clients;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Net.Sockets;

public class UDPKeyboard : MonoBehaviour
{
    /**
     * <summary>
     * UDP client 
     * </summary>
     */
    private UdpClient client;

    // Use this for initialization
    void Start()
    {
        this.client = new UdpClient(9051);
    }

    // Update is called once per frame
    void Update()
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
    private void OnKeypress ()
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
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

        try
        {
            //string json = "[{\"command\": \"move-left\"}]";
            ClientCommand command = new ClientCommand();

            // Convert data into json string
            ClientMoveData clientData = new ClientMoveData();
            string data = JsonConvert.SerializeObject(clientData);

            // Convert command into json string
            command.ID = ClientCommandID.MOVE;
            command.Data = data;

            string json = JsonConvert.SerializeObject(command);

            // Convert json string to bytes and send over to the server.
            Byte[] sendBytes = Encoding.ASCII.GetBytes(json);
            this.client.Send(sendBytes, sendBytes.Length, endPoint);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    /**
     * <summary>
     * On application quit
     * </summary>
     */
    private void OnApplicationQuit()
    {
        this.client.Close();
    }
}
