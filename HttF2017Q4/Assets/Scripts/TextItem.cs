using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextItem : MonoBehaviour
{
    [TextArea]
    public string Text;

	public TextMeshPro TMP;

    public int ID = 50;

    public DropZone AssociatedZone;



    void Awake()
    {

	    TMP.text = Text;

		transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(-15f, 15f));
    }

	void Start()
	{
		GNM.Instance.AddToTracked(ID, gameObject);
	}

    public void PickUp(bool sendMessage = false)
    {
		gameObject.SetActive(false);
	    TMP.gameObject.SetActive(true);

		if (AssociatedZone)
        {
            AssociatedZone.ClearZone();
            AssociatedZone = null;
        }

	    if (sendMessage)
	    {
		    GNM.Instance.SendData(ILMsgType.PickedUp, ID.ToString());
	    }
    }

    public void Drop(Vector3 location, bool sendMessage=false)
    {
		transform.position = location + Vector3.up * 2;

		gameObject.SetActive(true);
		if (sendMessage)
		{
			var data = new SpawnData() {PlayerId = ID, Position = location};
			GNM.Instance.SendData(ILMsgType.SetItemPos, JsonUtility.ToJson(data));
		}
	}

}
