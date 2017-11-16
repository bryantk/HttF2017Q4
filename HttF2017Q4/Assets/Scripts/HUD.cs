using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	private PlayerObject _playerObject;

	[SerializeField] private Transform _inventoryParent;

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

	public void AddInventoryItem(int index, string description)
	{
		var inventorySlot = _inventoryParent.GetChild(index);
		if(inventorySlot == null) return;
		inventorySlot.GetComponent<Image>().color = Color.cyan;
		inventorySlot.GetComponentInChildren<TextMeshProUGUI>().text = description;
	}

	public void RemoveInventoryItem(int index)
	{
		var inventorySlot = _inventoryParent.GetChild(index);
		if (inventorySlot == null) return;
		inventorySlot.GetComponent<Image>().color = Color.white;
		inventorySlot.GetComponentInChildren<TextMeshProUGUI>().text = "";
	}
}
