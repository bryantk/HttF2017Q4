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

    public void PickUp()
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
    }

    public void Drop(Vector3 Location)
    {
        _myRigidbody.isKinematic = false;
        _myRigidbody.useGravity = true;
        _myCollider.enabled = true;
        foreach (Renderer rend in rends)
        {
            rend.enabled = true;
        }

        transform.position = Location;
    }

}
