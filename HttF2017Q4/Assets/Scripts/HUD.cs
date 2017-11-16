using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

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
		Debug.Log("Emoticon button clicked: " + buttonName);
		//todo
	}
}
