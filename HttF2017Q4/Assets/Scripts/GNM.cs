using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GNM : NetworkManager
{

	private bool isServer;
	private bool isHost;
	private bool isClient;

	private GameObject playerGameObject;
	void Start()
	{
		
	}

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

	public void SendMessage(short type, string data)
	{
		if (client == null) return;

		var message = new DataMessage {message = data};
		if (isServer)
		{
			Debug.Log("Sending server message to all");
			NetworkServer.SendToAll(MyMsgType.Hello, message);
		}
		else
		{
			Debug.Log("CLient sending server message");
			client.Send(MyMsgType.Hello, message);
		}
			
	}

	public void OnServerMessage(NetworkMessage netMsg)
	{
		DataMessage msg = netMsg.ReadMessage<DataMessage>();
		Debug.LogWarning("Server SENT message: " + msg.message + "  from client " + netMsg.conn.connectionId);
		SendMessage(netMsg.msgType, msg.message);
	}

	public void OnMessage(NetworkMessage netMsg)
	{
		DataMessage msg = netMsg.ReadMessage<DataMessage>();
		Debug.LogWarning("Client GOT message: " + msg.message + "  from client " + netMsg.conn.connectionId);
	}



	public override void OnStartServer()
	{
		base.OnStartServer();
		isServer = true;
		NetworkServer.RegisterHandler(MyMsgType.Hello, OnServerMessage);
		Debug.LogWarning("Starting server");
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		isServer = false;
	}



	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
		Debug.LogWarning("Client connected: " + conn.connectionId);
		playerGameObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
		Debug.LogWarning("Client DISconnected: " + conn.connectionId);
		
	}

	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient(client);
		isClient = true;
		Debug.LogWarning("Starting cleint");
		client.RegisterHandler(MyMsgType.Hello, OnMessage);
	}

	public override void OnStopClient()
	{
		client.UnregisterHandler(MyMsgType.Hello);
		Destroy(playerGameObject);
		Debug.LogWarning("Client stopped: ");
		base.OnStopClient();
		isClient = false;
		
	}

}



public class MyMsgType
{
	public static short Hello = MsgType.Highest + 1;
};

public class DataMessage : MessageBase
{
	public string message;
}