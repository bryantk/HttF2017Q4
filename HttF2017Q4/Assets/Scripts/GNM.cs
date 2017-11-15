using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GNM : NetworkManager
{

	private bool isHost;
	private ServerData _serverData;
	private ClientData _clientData;
	private int _myConnectionId;


	public bool IsServer { get { return _serverData != null; } }
	public bool IsClient { get { return _clientData != null; } }


	void OnGUI()
	{
		string mesage = null;
		if (GUI.Button(new Rect(10, 300, 50, 30), "Send 1"))
			mesage = "item 1";
		if (GUI.Button(new Rect(10, 370, 50, 30), "Send 2"))
			mesage = "item 2";

		if (string.IsNullOrEmpty(mesage)) return;


		SendMessage(MyMsgType.Hello, mesage);
	}

	public void SendMessage(short type, string data, int sourceConnection = 0)
	{
		if (client == null) return;

		var message = new DataMessage {
			message = data,
			SourceClient = sourceConnection
		};
		if (IsServer)
		{
			Debug.Log("Sending server message to all");
			NetworkServer.SendToAll(MyMsgType.Hello, message);
		}
		else
		{
			Debug.Log("CLient sending server message");
			message.SourceClient = _myConnectionId;
			client.Send(MyMsgType.Hello, message);
		}
			
	}

	/// <summary>
	/// Client got a message from Server. Do it unless it came from yourself.
	/// </summary>
	/// <param name="netMsg"></param>
	public void OnClientMessageRecieved(NetworkMessage netMsg)
	{
		DataMessage msg = netMsg.ReadMessage<DataMessage>();

		if (msg.SourceClient == _myConnectionId) return;
		Debug.LogWarning("Client GOT message: " + msg.message + "  from client " + msg.SourceClient);

	}


	public void OnServerMessageRecieved(NetworkMessage netMsg)
	{
		DataMessage msg = netMsg.ReadMessage<DataMessage>();
		Debug.LogWarning("SENT Server message: " + msg.message + "  from client " + netMsg.conn.connectionId);
		// Server rebroadcast message to all clients
		Debug.Log(msg.SourceClient);
		SendMessage(netMsg.msgType, msg.message, netMsg.conn.connectionId);
	}





	public override void OnStartServer()
	{
		base.OnStartServer();
		_serverData = gameObject.AddComponent<ServerData>();
		NetworkServer.RegisterHandler(MyMsgType.Hello, OnServerMessageRecieved);
		Debug.LogWarning("OnStartServer");
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		Destroy(_serverData);
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		Debug.LogWarning("OnServerConnect " + conn.connectionId);
	}

	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient(client);
		_myConnectionId = client.connection.connectionId;
		Debug.LogWarning("OnStartClient: " + client.connection.connectionId);
		client.RegisterHandler(MyMsgType.Hello, OnClientMessageRecieved);
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
		Debug.LogWarning("OnClientConnect: " + conn.connectionId);
		_clientData = gameObject.AddComponent<ClientData>();
		_clientData.CreatePlayer(playerPrefab);
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		Debug.LogWarning("OnServerDisconnect " + conn.connectionId);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
		Debug.LogWarning("OnClientDisconnect: " + conn.connectionId);
	}

	public override void OnStopClient()
	{
		client.UnregisterHandler(MyMsgType.Hello);
		Debug.LogWarning("OnStopClient");
		base.OnStopClient();
		Destroy(_clientData);
		
	}

}



public class MyMsgType
{
	public static short Hello = MsgType.Highest + 1;
	public static short SetPos = MsgType.Highest + 1;
};




public class ILMessage : MessageBase
{
	public int SourceClient;
}

public class DataMessage : ILMessage
{
	public string message;
}