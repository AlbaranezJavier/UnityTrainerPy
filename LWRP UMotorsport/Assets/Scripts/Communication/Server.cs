using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

internal class Server : MonoBehaviour
{
    /*
     * Class in charge of managing communications with other environments
     */
    public int _port = 12345;
    public string _ip_address = "192.168.1.1";
    public ServerUser _serverUser;
    public bool raiseServer = false;

    private bool connectionEstablished = false;
    private static bool isShutdown = false;
    private static Socket clientSocket;

    private void Update()
    {
        if (!connectionEstablished && raiseServer)
        {
            BuildServer();
            StartServer();
        } 
    }

    public void BuildServer()
    {
        /*
         * Raise the server on a specific port on this machine
         */
        IPAddress ipAddr = IPAddress.Parse(_ip_address);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, _port);
        Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(50);
            clientSocket = listener.Accept();
            connectionEstablished = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            raiseServer = false;
        }
    }

    /*
     Manages the message received from the client
         */
    private void ReceiveMsg(string data, byte[] bytes, int numByte, Socket clientSocket)
    {
        data += Encoding.ASCII.GetString(bytes, 0, numByte);
        if (data.Equals(""))
            Shutdown(clientSocket);
        else
        {
            _serverUser.Receive_msg(data);
        }
    }

    /*
     Return a message to the client
         */
    public void SendMsg(Socket clientSocket)
    {
        if (!isShutdown)
        {
            clientSocket.Send(_serverUser.Send_msg());
        }
    }

    /*
     Controls that the closing of communications with the client
         */
    private static void Shutdown(Socket clientSocket)
    {
        isShutdown = true;
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }


    /*
     Thread to manage communications with the client independently
         */
    private void StartServer()
    {
        Thread myServerThread = new Thread(ServerThread);
        myServerThread.Start();
    }

    private void ServerThread()
    {
        while (!isShutdown)
        {
            byte[] bytes = new byte[1024];
            string data = null;

            int numByte = clientSocket.Receive(bytes);

            ReceiveMsg(data, bytes, numByte, clientSocket);

            SendMsg(clientSocket);
        }
    }
}

/*
 Specify the minimum structure to make use of the server
     */
public abstract class ServerUser : MonoBehaviour
{
    public abstract void Receive_msg(string msg);
    public abstract byte[] Send_msg();
}
