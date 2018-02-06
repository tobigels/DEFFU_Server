using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public enum DataEventType_Server {
    DATA = 1,
    NAMERESPONSE = 2,
    ASKGAMESTART = 3
}

public enum DataEventType_Client {
    DATA = 1,
    ASKNAME = 2,
    NEWCLIENT = 3,
    DISCONNECT = 4,
    GAMETURN = 5
}

public class ConnectionManager{

    #region FIELDS

    private PlayerManager playerManager;
    private SerializationUnit serializationUnit;
    private int port;
    private int hostId;
    private int reliableChannel;
    private int unreliableChannel;
    private bool isStarted;
    private byte error;
    private string localIp;

    public readonly int MAX_CONNECTIONS = 4;

    #endregion

    #region METHODS

    // --------------------------------------- Private methods ---------------------------------------

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    private void OnConnection(int connectionId) {
        ServerMessage m = new ServerMessage();


        m.Content = serializationUnit.SerializeHelper(playerManager.AllPlayers);
        m.Type = (int)DataEventType_Client.ASKNAME;
        m.Origin = connectionId;
        SendMessage(m, true, connectionId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    private void OnDisconnect(int connectionId) {
        if (playerManager.RemovePlayer(connectionId)) {
            SendDisconnect(connectionId);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="m"></param>
    private void OnDataEvent(int connectionId, Message m) {
        DataEventType_Server eventType = (DataEventType_Server)m.Type;

        switch (eventType) {
            case DataEventType_Server.DATA:
                Debug.Log("- " + connectionId + " - DATA");
                SendData(connectionId, m.Content);
                break;
            case DataEventType_Server.NAMERESPONSE:
                string name = (string)serializationUnit.DeserializeHelper(m.Content, m.Content.Length);
                Debug.Log("- " + connectionId + " - NAMERESPONSE " + name);
                if (playerManager.AddPlayer(connectionId, name)) {
                    SendNewClient(connectionId, name);
                }
                break;
            case DataEventType_Server.ASKGAMESTART:
                Debug.Log("- " + connectionId + " - ASKGAMESTART");
                playerManager.GameStarted = true;
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="m"></param>
    /// <param name="reliable"></param>
    /// <param name="connectionId"></param>
    private void SendMessage(ServerMessage m, bool reliable, int connectionId) {
        int bufferSize = 1024;
        byte error;
        byte[] recBuffer = new byte[1024];

        recBuffer = serializationUnit.SerializeHelper(m);

        if (reliable) {
            NetworkTransport.Send(hostId, connectionId, reliableChannel, recBuffer, bufferSize, out error);
        } else {
            NetworkTransport.Send(hostId, connectionId, unreliableChannel, recBuffer, bufferSize, out error);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    private void SendNewClient(int id, string name) {
        ServerMessage m = new ServerMessage();

        m.Content = serializationUnit.SerializeHelper(name);
        m.Type = (int)DataEventType_Client.NEWCLIENT;
        m.Origin = id;
        SendAllClientsButOne(m, true, id);
    }

    /// <summary>
    /// 
    /// </summary>
    private void SendGameFrame() {
        ServerMessage m = new ServerMessage();

        m.Content = null;
        m.Type = (int)DataEventType_Client.GAMETURN;
        m.Origin = playerManager.GameFrame;
        SendAllClients(m, true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    private void SendDisconnect(int id) {
        ServerMessage m = new ServerMessage();

        m.Content = null;
        m.Type = (int)DataEventType_Client.DISCONNECT;
        m.Origin = id;
        SendAllClientsButOne(m, true, id);
    }

    /// <summary>
    /// 
    /// </summary>
    private void SendData(int id, byte[] content) {
        ServerMessage m = new ServerMessage();

        m.Content = content;
        m.Type = (int)DataEventType_Client.DATA;
        m.Origin = id;
        SendAllClientsButOne(m, false, id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="m"></param>
    /// <param name="reliable"></param>
    /// <param name="id"></param>
    private void SendAllClientsButOne(ServerMessage m, bool reliable, int id) {
        Player[] players = playerManager.AllPlayers;
        for (int i = 0; i < players.Length; i++) {
            int connId = players[i].Id;
            if (id != connId && connId != 0) {
                SendMessage(m, reliable, connId);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="m"></param>
    /// <param name="reliable"></param>
    private void SendAllClients(ServerMessage m, bool reliable) {
        Player[] players = playerManager.AllPlayers;
        for (int i = 0; i < players.Length; i++) {
            int connectionId = players[i].Id;
            if (connectionId != 0) {
                SendMessage(m, reliable, connectionId);
            }
        }
    }

    // --------------------------------------- Public methods ---------------------------------------

    /// <summary>
    /// 
    /// </summary>
    public string LocalIp {
        get {
            return localIp;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pm"></param>
    public ConnectionManager(PlayerManager pm) {
        playerManager = pm;
        serializationUnit = new SerializationUnit();
        port = 5701;
        isStarted = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void StartServer() {

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach(IPAddress ip in host.AddressList) {
            if(ip.AddressFamily == AddressFamily.InterNetwork) {
                localIp = ip.ToString();
            }
        }

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology ht = new HostTopology(cc, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(ht, port);

        isStarted = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckForIncomingData() {

        bool noEventsLeft = false;

        if(isStarted) {
            while(!noEventsLeft) {
                int recHostId;
                int connectionId;
                int channelId;
                byte[] recBuffer = new byte[1024];
                int dataSize;
                byte error;
                NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, recBuffer.Length, out dataSize, out error);

                switch (recData) {
                    case NetworkEventType.Nothing:
                        noEventsLeft = true;
                        break;
                    case NetworkEventType.ConnectEvent:
                        Debug.Log("Player " + connectionId + " has connected");
                        OnConnection(connectionId);
                        break;
                    case NetworkEventType.DataEvent:
                        Debug.Log("Player " + connectionId + " has sent data");
                        Message m = (Message)serializationUnit.DeserializeHelper(recBuffer, dataSize);
                        OnDataEvent(connectionId, m);
                        break;
                    case NetworkEventType.DisconnectEvent:
                        Debug.Log("Player " + connectionId + " has disconnected");
                        OnDisconnect(connectionId);
                        break;
                }
            }
        }
    }

    public void ExecuteGameTurn() {
        SendGameFrame();
    }
        #endregion
    }
