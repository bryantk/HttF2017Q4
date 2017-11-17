using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClientData : MonoBehaviour
{

	public GameObject PlayerGameObject;
	public PlayerObject PlayerScript;

	/// <summary>
	/// When client connects and creates its own controlalble player
	/// </summary>
	/// <param name="prefab"></param>
	/// <returns></returns>
	public PlayerObject CreatePlayer(GameObject prefab, int playerID)
	{
		var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
		PlayerScript = go.GetComponent<PlayerObject>();
		PlayerScript.EventSystem = FindObjectOfType<EventSystem>();
		PlayerScript.SetColor(playerID);
		PlayerGameObject = go;
		return PlayerScript;
	}

	void OnDestroy()
	{
		Destroy(PlayerGameObject);
		// tODO send message to destroy
	}
}
