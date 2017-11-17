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

    private Rigidbody _myRigidbody;
    private Collider _myCollider;
    private Renderer[] rends;
    public DropZone AssociatedZone;



    void Awake()
    {
        _myRigidbody = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();
        rends = GetComponentsInChildren<Renderer>();

	    TMP.text = Text;

		transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(-10f, 10f));
    }

	void Start()
	{
		GNM.Instance.AddToTracked(ID, gameObject);
	}

    public void PickUp(bool sendMessage = false)
    {
		gameObject.SetActive(false);
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
		location.y += 2;
		transform.position = location;

		gameObject.SetActive(true);

		if (sendMessage)
		{
			var data = new SpawnData() {PlayerId = ID, Position = location};
			GNM.Instance.SendData(ILMsgType.SetItemPos, JsonUtility.ToJson(data));
		}
	}

}
