using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GNMHelpers : MonoBehaviour {

	public PlayerObject CreateMirrorPlayer(GameObject prefab, int playerID)
	{
		var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
		var PlayerScript = go.GetComponent<PlayerObject>();
		PlayerScript.SetAsClient();
		PlayerScript.SetColor(playerID);
		return PlayerScript;
	}

}
