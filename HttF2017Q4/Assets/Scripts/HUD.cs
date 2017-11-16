using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	private PlayerObject _playerObject;

	void Awake()
	{
		_playerObject = GetComponentInParent<PlayerObject>();
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void InventoryButtonOnClick(string buttonName)
	{
		Debug.Log("Inventory button clicked: " + buttonName);
		//todo
	}

	public void EmoticonButtonOnClick(string buttonName)
	{
		_playerObject.Emote(buttonName);
		GNM.Instance.SendDataUnreliable(ILMsgType.Emote, buttonName);
	}
}
