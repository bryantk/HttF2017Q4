using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextItem : MonoBehaviour
{
    [TextArea]
    public string Text;

    private Rigidbody _myRigidbody;
    private Collider _myCollider;
    private Renderer[] rends;

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
    }

}
