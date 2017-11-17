using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    [SerializeField] private Color _inactiveColor;
    [SerializeField] private Color _activeColor;
    private string _defaultText = "\n<color=red>...<color=black>  ";

    [SerializeField]
    private LayerMask _itemLayer;

    private DropManager _myDropManager;

    public DropManager DropManager
    {
        get { return _myDropManager; }
        set
        {
            if (_myDropManager != null) return;
            _myDropManager = value;
        }
    }

    private bool _isActive;
    public TextItem CurrentTextItem;

    public bool HasItem
    {
        get { return _isActive; }
    }

	// Use this for initialization
	void Start ()
	{
	    SetActive((false));
	}

    void SetActive(bool active)
    {
        _isActive = active;
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            rend.material.color = active ? _activeColor : _inactiveColor;
        } 
    }

    void OnTriggerEnter(Collider col)
    {
        if (_isActive)
            return;
        if (col.GetComponent<TextItem>())
        {
            col.transform.position = transform.position;
            CurrentTextItem = col.GetComponent<TextItem>();
	        col.GetComponentInChildren<TextMeshPro>(true).gameObject.SetActive(false);
            col.GetComponent<TextItem>().AssociatedZone = this;
			DropManager.UpdateText();
            SetActive(true);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (!_isActive)
            return;
        if (col.GetComponent<TextItem>())
        {
            ClearZone();
        }
    }

    public void ClearZone()
    {
        CurrentTextItem = null;
        SetActive(false);
		DropManager.UpdateText();
    }

	// Update is called once per frame
	void Update () {
	    if (GNM.Instance.IsClient)
	    {
	        return;
	    }
	}

    public string GetCurrentText()
    {
        if (CurrentTextItem == null)
            return _defaultText;
        return CurrentTextItem.Text;
    }

    public float GetSize()
    {
        return GetComponent<Renderer>().bounds.size.z;
    }

    public void SetSize(float size)
    {
        //adjust for absolute size
        float multiplier = GetComponent<Collider>().bounds.size.z / transform.localScale.z;
        float adjustedSize = size * multiplier;
        Transform myParent = transform.parent;
        transform.parent = null;
        transform.localScale = new Vector3(adjustedSize, adjustedSize, adjustedSize);
        transform.parent = myParent;
    }
}
