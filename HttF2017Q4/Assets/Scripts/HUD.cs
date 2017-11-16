using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	private PlayerObject _playerObject;
	private List<TextItem> _textItems;
	public TextItem SelectedItem;
	[SerializeField] private Transform _inventoryParent;

	void Awake()
	{
		_textItems = new List<TextItem> {null, null, null};
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
		for (var index = 0; index < _textItems.Count; index++)
		{
			var textItem = _textItems[index];
			if (textItem == null) continue;
			var inventorySlot = _inventoryParent.GetChild(index);
			var isSelectedSlot = inventorySlot.name == buttonName;
			var color = isSelectedSlot ? Color.green : Color.cyan;
			inventorySlot.GetComponent<Image>().color = color;

			if (isSelectedSlot)
			{
				SelectedItem = textItem;
			}
		}
	}

	public void EmoticonButtonOnClick(string buttonName)
	{
		_playerObject.Emote(buttonName);
		GNM.Instance.SendDataUnreliable(ILMsgType.Emote, buttonName);
	}

	public void AddInventoryItem(TextItem textItem)
	{
		if (textItem == null || _textItems.Contains(textItem))
		{
			return;
		}

		var inventoryIndex = _textItems.IndexOf(null);
		if (inventoryIndex < 0)
		{
			Debug.LogWarning("Too much inventory");
			//no available slots, send message
			return;
		}

		textItem.PickUp(true);
		_textItems[inventoryIndex] = textItem;

		var inventorySlot = _inventoryParent.GetChild(inventoryIndex);
		if(inventorySlot == null) return;
		inventorySlot.GetComponent<Image>().color = Color.cyan;
		inventorySlot.GetComponentInChildren<TextMeshProUGUI>().text = textItem.Text;
	}

	public void DeselectItem()
	{
		for (var index = 0; index < _textItems.Count; index++)
		{
			var textItem = _textItems[index];
			if (textItem == null) continue;
			var inventorySlot = _inventoryParent.GetChild(index);
			inventorySlot.GetComponent<Image>().color = Color.cyan;
			SelectedItem = null;
		}
	}

	public void RemoveSelectedInventoryItem()
	{
		if(SelectedItem == null) return;

		var index = _textItems.IndexOf(SelectedItem);
		var inventorySlot = _inventoryParent.GetChild(index);
		inventorySlot.GetComponent<Image>().color = Color.white;
		inventorySlot.GetComponentInChildren<TextMeshProUGUI>().text = "";
		SelectedItem = null;
		_textItems[index] = null;
	}

}
