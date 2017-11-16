using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GNMHelpers : MonoBehaviour {

	public GameObject CreateMirrorPlayer(GameObject prefab)
	{
		var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
		var PlayerScript = go.GetComponent<PlayerObject>();
		PlayerScript.SetAsClient();
		Destroy(PlayerScript);
		return go;
	}
}
