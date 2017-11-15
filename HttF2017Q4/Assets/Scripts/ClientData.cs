using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{

	public GameObject PlayerGameObject;
	public PlayerObject PlayerScript;

	public GameObject CreatePlayer(GameObject prefab, bool client = false)
	{
		var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
		PlayerScript = go.GetComponent<PlayerObject>();
		if (client)
		{
			PlayerScript.SetAsClient();
			Destroy(PlayerScript);
		}
		else
		{
			PlayerGameObject = go;
		}
		return go;
	}

	void OnDestroy()
	{
		Destroy(PlayerGameObject);
		// tODO send message to destroy
	}
}
