using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{

	public GameObject PlayerGameObject;
	public PlayerObject PlayerScript;

	public GameObject CreatePlayer(GameObject prefab, bool client = false)
	{
		PlayerGameObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
		PlayerScript = PlayerGameObject.GetComponent<PlayerObject>();
		if (client)
		{
			PlayerScript.SetAsClient();
		}
		return PlayerGameObject;
	}

	void OnDestroy()
	{
		Destroy(PlayerGameObject);
		// tODO send message to destroy
	}
}
