using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextItem : MonoBehaviour
{
    [TextArea]
    public string Text;

    public int ID = 50;

    private Rigidbody _myRigidbody;
    private Collider _myCollider;
    private Renderer[] rends;
    public DropZone AssociatedZone;

    public bool CanPickup;

    void Awake()
    {
        _myRigidbody = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();
        rends = GetComponentsInChildren<Renderer>();
    }

	void Start()
	{
		GNM.Instance.AddToTracked(ID, gameObject);
	}

    public void PickUp(bool sendMessage = false)
    {
		if (!CanPickup)
			return;
		_myRigidbody.isKinematic = true;
		_myRigidbody.useGravity = false;
		_myCollider.enabled = false;

		foreach (Renderer rend in rends)
		{
			rend.enabled = false;
		}

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

		_myRigidbody.isKinematic = false;
        _myRigidbody.useGravity = true;
        _myCollider.enabled = true;
        foreach (Renderer rend in rends)
        {
            rend.enabled = true;
        }

		if (sendMessage)
		{
			GNM.Instance.SendData(ILMsgType.SetPos, ID.ToString());
		}
	}

}
