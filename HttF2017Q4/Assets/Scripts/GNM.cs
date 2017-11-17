using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GNM : NetworkManager
{

	private bool isHost;
	private ServerData _serverData;
	private ClientData _clientData;
	private GNMHelpers _helperMethods;
	private int _myConnectionId = -2;

	public Camera BaseCamera;
	public Transform Lobby;
	public TextMeshPro StartText;

	public static GNM Instance;

	public bool IsServer { get { return _serverData != null; } }
	public bool IsClient { get { return _clientData != null; } }

	public int Players { get { return _playerObjects.Count; } }

	private Dictionary<int, PlayerObject> _playerObjects = new Dictionary<int, PlayerObject>();
	private Dictionary<int, GameObject> _TrackedObjects = new Dictionary<int, GameObject>();

	private bool _gameStarted;
	private string ServerIP;

	void Awake()
	{
		if (Instance != null)
			Destroy(this);
		Instance = this;

		_helperMethods = gameObject.AddComponent<GNMHelpers>();
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

		Debug.LogWarning("Got message from " + msg.SourceClient + " - " + msg.message);
		// DO STUFF WITH THE MESSAGE
		if (netMsg.msgType == ILMsgType.Hello)
		{
			_myConnectionId = int.Parse(msg.message);
			_clientData = gameObject.AddComponent<ClientData>();
			var player = _clientData.CreatePlayer(playerPrefab);
			player.name = "Player_" + _myConnectionId + "_me";
			_playerObjects[_myConnectionId] = player;
		}
		else if (_myConnectionId == -2)
		{
			return;
		}
		else if (netMsg.msgType == ILMsgType.SpawnPlayer)
		{
			var data = JsonUtility.FromJson<SpawnData>(msg.message);
			if (data.PlayerId == _myConnectionId) return;

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
			Destroy(_playerObjects[id].gameObject);
			_playerObjects.Remove(id);
		}
		else if (netMsg.msgType == ILMsgType.SetPos)
		{
			var id = msg.SourceClient;

			if (_playerObjects.ContainsKey(id))
			{
				_playerObjects[id].transform.position = JsonUtility.FromJson<Vector3>(msg.message);
			}
			else if (_TrackedObjects.ContainsKey(id))
			{
				var interactable = _TrackedObjects[id].GetComponent<TextItem>();
				if (interactable == null) return;

				interactable.Drop(JsonUtility.FromJson<Vector3>(msg.message));
			}
		}
		else if (netMsg.msgType == ILMsgType.SetItemPos)
		{
			var itemData = JsonUtility.FromJson<SpawnData>(msg.message);
			if (itemData == null)
				Debug.LogError("WHY!");
			if (!_TrackedObjects.ContainsKey(itemData.PlayerId)) return;

			var item = _TrackedObjects[itemData.PlayerId];
			var ti = item.GetComponent<TextItem>();
			if (ti == null) return;

			ti.Drop(itemData.Position);
		}
		else if (netMsg.msgType == ILMsgType.MoveTo)
		{
			var id = msg.SourceClient;
			Debug.LogWarning(id);
			if (!_playerObjects.ContainsKey(id)) return;

			var pos = JsonUtility.FromJson<Vector3>(msg.message);
			_playerObjects[id].MoveTo(pos);
		}
		else if (netMsg.msgType == ILMsgType.Emote)
		{
			var id = msg.SourceClient;
			if (!_playerObjects.ContainsKey(id)) return;

			_playerObjects[id].Emote(msg.message);
		}
		else if (netMsg.msgType == ILMsgType.PickedUp)
		{
			var id = int.Parse(msg.message);
			if (!_TrackedObjects.ContainsKey(id)) return;

			var interactable = _TrackedObjects[id].GetComponent<TextItem>();
			if (interactable == null) return;

			interactable.PickUp();
		}
		else if (netMsg.msgType == ILMsgType.Pause)
		{
			SetPause(msg.message == "True");
		}
		else if (netMsg.msgType == ILMsgType.StartGame)
		{
			StartGame();
		}
		else if (netMsg.msgType == ILMsgType.EndGame)
		{
			EndGame();
		}
	}


	public void OnServerMessageRecieved(NetworkMessage netMsg)
	{
		ILMessage msg = netMsg.ReadMessage<ILMessage>();
		Debug.LogWarning("SENT Server message: " + msg.message + "  from client " + netMsg.conn.connectionId);
		// Server rebroadcast message to all clients
		Debug.Log(msg.SourceClient);
		// Propagate it to clients
		SendData(netMsg.msgType, msg.message, netMsg.conn.connectionId);
	}





	public override void OnStartServer()
	{
		base.OnStartServer();

		ServerIP = System.Net.Dns.GetHostName();
		var ipEntry = System.Net.Dns.GetHostEntry(ServerIP);
		var addr = ipEntry.AddressList;
		ServerIP = addr[addr.Length - 1].ToString();

		_serverData = gameObject.AddComponent<ServerData>();
		_playerObjects = new Dictionary<int, PlayerObject>();
		Debug.LogWarning("OnStartServer");
		// REGISTER MESSAGES HERE
		NetworkServer.RegisterHandler(ILMsgType.Hello, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.SetPos, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.SpawnPlayer, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.RemoveId, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.MoveTo, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.Emote, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.PickedUp, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.SetItemPos, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.Pause, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.StartGame, OnServerMessageRecieved);
		NetworkServer.RegisterHandler(ILMsgType.EndGame, OnServerMessageRecieved);
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
		// Teach connection what its ID is
		SendData(ILMsgType.Hello, conn.connectionId.ToString(), -1, conn.connectionId);
		// Send existing player info to new guy
		foreach (var kv in _playerObjects)
		{
			Debug.LogWarning("Updating spawn " + kv.Key);
			var poco2 = new SpawnData { PlayerId = kv.Key, Position = kv.Value.transform.position };
			SendData(ILMsgType.SpawnPlayer, JsonUtility.ToJson(poco2), -1, conn.connectionId);
		}
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
		client.RegisterHandler(ILMsgType.MoveTo, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.Emote, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.PickedUp, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.SetItemPos, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.Pause, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.StartGame, OnClientMessageRecieved);
		client.RegisterHandler(ILMsgType.EndGame, OnClientMessageRecieved);
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		if (_gameStarted)
		{
			conn.Disconnect();
			return;
		}

		base.OnClientConnect(conn);
		
		Debug.LogWarning("OnClientConnect: " + conn.connectionId);
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
		Cleanup();
	}

	public override void OnStopClient()
	{
		Debug.LogWarning("OnStopClient");
		base.OnStopClient();
		Cleanup();
	}

	private void Cleanup()
	{
		if (_clientData != null)
			Destroy(_clientData);
		foreach (var kv in _playerObjects)
		{
			Destroy(kv.Value.gameObject);
		}
		_playerObjects.Clear();
		_myConnectionId = -2;
	}


	public void AddToTracked(int id, GameObject go)
	{
		_TrackedObjects.Add(id, go);
	}


	/// <summary>
	/// Server command only
	/// </summary>
	/// <param name="shouldPause"></param>
	public void Pause(bool shouldPause)
	{
		Debug.LogError("need to pause: " + shouldPause);
		SendData(ILMsgType.Pause, shouldPause.ToString());
		SetPause(shouldPause);
	}

	private void SetPause(bool shouldPause)
	{
		//foreach (var kv in _playerObjects)
		//{
		//	kv.Value.Paused = shouldPause;
		//}
		_clientData.PlayerScript.Paused = shouldPause;
	}


	private void StartGame(bool sendMessage = false)
	{
		_gameStarted = true;
		Lobby.DOMoveY(-2, 4).OnComplete(() => Destroy(Lobby.gameObject));

		if (sendMessage)
		{
			SendData(ILMsgType.StartGame, "");
		}
	}

	public void EndGame(bool sendMessage = false)
	{
		BaseCamera.depth = 2;
		BaseCamera.transform.DOMove(new Vector3(0, 185, 10), 10).SetEase(Ease.InCubic);
		BaseCamera.DOColor(new Color(0.2f, 0.39f, 0.7f), 10).SetEase(Ease.InCubic);
		_clientData.PlayerScript.SetAsClient();
		StartText.text = "The End";


		if (sendMessage)
		{
			SendData(ILMsgType.EndGame, "");
		}
	}


	void OnGUI()
	{
		if (!IsServer) return;

		GUI.Label(new Rect(10, 10, 100, 20), ServerIP);

		if (_gameStarted) return;

		if (GUI.Button(new Rect(200, 330, 100, 50), "Start"))
			StartGame(true);

	}

}



public class ILMsgType
{
	public static short Hello = MsgType.Highest + 1; // First contact message
	public static short SetPos = MsgType.Highest + 2;
	public static short SpawnPlayer = MsgType.Highest + 3;
	public static short RemoveId = MsgType.Highest + 4;
	public static short MoveTo = MsgType.Highest + 5;
	public static short Emote = MsgType.Highest + 6;
	public static short PickedUp = MsgType.Highest + 7;
	public static short SetItemPos = MsgType.Highest + 8;
	public static short Pause = MsgType.Highest + 9;
	public static short StartGame = MsgType.Highest + 10;
	public static short EndGame = MsgType.Highest + 11;
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