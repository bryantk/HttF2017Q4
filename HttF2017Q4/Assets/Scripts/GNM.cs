using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Networking;

public class GNM : NetworkManager
{

	private bool isHost;
	private ServerData _serverData;
	private ClientData _clientData;
	private GNMHelpers _helperMethods;
	private int _myConnectionId;

	public static GNM Instance;

	public bool IsServer { get { return _serverData != null; } }
	public bool IsClient { get { return _clientData != null; } }

	private Dictionary<int, GameObject> _playerObjects = new Dictionary<int, GameObject>();


	void Awake()
	{
		if (Instance != null)
			Destroy(this);
		Instance = this;

		_helperMethods = gameObject.AddComponent<GNMHelpers>();
	}

	void OnGUI()
	{
		string mesage = null;
		if (GUI.Button(new Rect(10, 300, 50, 30), "Send 1"))
			mesage = "item 1";
		if (GUI.Button(new Rect(10, 370, 50, 30), "Send 2"))
			mesage = "item 2";

		if (string.IsNullOrEmpty(mesage)) return;


		SendData(ILMsgType.Hello, mesage);
	}

	/// <summary>
	/// Send a message to all clients (or server first if you are a client)
	/// </summary>
	/// <param name="type"></param>
	/// <param name="data"></param>
	/// <param name="sourceConnection"></param>
	public void SendData(short type, string data, int sourceConnection = 0, int targetConn = -1)
	{
		if (!IsServer && client == null) return;

		var message = new ILMessage
		{
			message = data,
			SourceClient = sourceConnection
		};
		if (IsServer)
		{
			Debug.Log("Sending server message to all on behalf of " + sourceConnection + ": " + type + " - " + data);
			if (targetConn == -1)
				NetworkServer.SendToAll(type, message);
			else
				NetworkServer.SendToClient(targetConn, type, message);
		}
		else
		{
			Debug.Log("CLient sending server message");
			message.SourceClient = _myConnectionId;
			client.Send(type, message);
		}
			
	}

	public void SendDataUnreliable(short type, string data, int sourceConnection = 0, int targetConn = -1)
	{
		if (!IsServer && client == null) return;

		var message = new ILMessage
		{
			message = data,
			SourceClient = sourceConnection
		};
		if (IsServer)
		{
			Debug.Log("Sending server message to all on behalf of " + sourceConnection + ": " + type + " - " + data);
			if (targetConn == -1)
				NetworkServer.SendUnreliableToAll(type, message);
			else
				NetworkServer.SendToClient(targetConn, type, message);
		}
		else
		{
			Debug.Log("CLient sending server message");
			message.SourceClient = _myConnectionId;
			client.SendUnreliable(type, message);
		}
	}


	/// <summary>
	/// Client got a message from Server. Do it unless it came from yourself.
	/// </summary>
	/// <param name="netMsg"></param>
	public void OnClientMessageRecieved(NetworkMessage netMsg)
	{
		ILMessage msg = netMsg.ReadMessage<ILMessage>();

		if (msg.SourceClient == _myConnectionId) return;

		Debug.Log("Got message from " + msg.SourceClient + " - " + msg.message);
		// DO STUFF WITH THE MESSAGE
		if (netMsg.msgType == ILMsgType.SpawnPlayer)
		{
			Debug.Log("Spawn");
			var data = JsonUtility.FromJson<SpawnData>(msg.message);
			var player = _helperMethods.CreateMirrorPlayer(playerPrefab);
			player.transform.position = data.Position;
			player.name = "Player_" + data.PlayerId;
			_playerObjects.Add(data.PlayerId, player);
		}
		else if (netMsg.msgType == ILMsgType.RemoveId)
		{
			Debug.LogWarning("KILL MESSAGE: " + msg.message);
			var id = int.Parse(msg.message);
			Debug.Log("Kill " + id);
			Destroy(_playerObjects[id]);
			_playerObjects.Remove(id);
		}
		else if (netMsg.msgType == ILMsgType.SetPos)
		{
			var id = msg.SourceClient;
			if (!_playerObjects.ContainsKey(id)) return;
			
			var pos = JsonUtility.FromJson<Vector3>(msg.message);
			_playerObjects[id].transform.DOMove(pos, 0.1f);
		}

	}


	public void OnServerMessageRecieved(NetworkMessage netMsg)
	{
		ILMessage msg = netMsg.ReadMessage<ILMessage>();
		Debug.LogWarning("SENT Server message: " + msg.message + "  from client " + netMsg.conn.connectionId);
		// Server rebroadcast message to all clients
		Debug.Log(msg.SourceClient);
		SendData(netMsg.msgType, msg.message, netMsg.conn.connectionId);
	}





	public override void OnStartServer()
	{
		base.OnStartServer();
		_serverData = gameObject.AddComponent<ServerData>();
		NetworkServer.RegisterHandler(ILMsgType.Hello, OnServerMessageRecieved);
		_playerObjects = new Dictionary<int, GameObject>();
		Debug.LogWarning("OnStartServer");
		// REGISTER MESSAGES HERE
		NetworkServer.RegisterHandler(ILMsgType.Hello, OnClientMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.SetPos, OnClientMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.SpawnPlayer, OnClientMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.RemoveId, OnClientMessageRecieved);
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

		// Send CreatePlayer commands to all
		var poco = new SpawnData {PlayerId = conn.connectionId, Position = Vector3.zero};
		SendData(ILMsgType.SpawnPlayer, JsonUtility.ToJson(poco), conn.connectionId);
		// Send existing player info to new guy
		foreach (var kv in _playerObjects)
		{
			Debug.LogWarning("Updating spawn " + kv.Key);
			var poco2 = new SpawnData { PlayerId = kv.Key, Position = kv.Value.transform.position };
			SendData(ILMsgType.SpawnPlayer, JsonUtility.ToJson(poco2), -1, conn.connectionId);
		}
		// Creat player (controlled or as mirror) and add to dict
		//var player = _clientData.CreatePlayer(playerPrefab, conn.connectionId != _myConnectionId);
		//_playerObjects.Add(conn.connectionId, player);
	}

	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient(client);
		//_myConnectionId = client.connection.connectionId;
		Debug.LogWarning("OnStartClient:");
		// REGISTER MESSAGES HERE
		client.RegisterHandler(ILMsgType.Hello, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.SetPos, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.SpawnPlayer, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.RemoveId, OnClientMessageRecieved);
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
		_myConnectionId = conn.connectionId;
		Debug.LogWarning("OnClientConnect: " + conn.connectionId);
		_clientData = gameObject.AddComponent<ClientData>();
		var player = _clientData.CreatePlayer(playerPrefab);
		player.name = "Player_" + conn.connectionId + "_me";
		_playerObjects[conn.connectionId] = player;
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		Debug.LogWarning("OnServerDisconnect " + conn.connectionId);

		// TODO - tell everyone he left
		SendData(ILMsgType.RemoveId, conn.connectionId.ToString(), conn.connectionId);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
		Debug.LogWarning("OnClientDisconnect: " + conn.connectionId);
	}

	public override void OnStopClient()
	{
		client.UnregisterHandler(ILMsgType.Hello);
		Debug.LogWarning("OnStopClient");
		base.OnStopClient();
		Destroy(_clientData);
		
	}

}



public class ILMsgType
{
	public static short Hello = MsgType.Highest + 1;
	public static short SetPos = MsgType.Highest + 2;
	public static short SpawnPlayer = MsgType.Highest + 3;
	public static short RemoveId = MsgType.Highest + 4;
};




public class ILMessage : MessageBase
{
	public int SourceClient;
	public string message;
}


[Serializable]
public class SpawnData
{
	public int PlayerId;
	public Vector3 Position;
}