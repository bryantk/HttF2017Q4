using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    [SerializeField] private Color _inactiveColor;
    [SerializeField] private Color _activeColor;
    [TextArea]
    [SerializeField] private string _defaultText = "\n...\n";

    [SerializeField]
    private LayerMask _itemLayer;

    private bool _isActive;
    private string _currentText;

    public bool HasItem
    {
        get { return _isActive; }
    }

	// Use this for initialization
	void Start ()
	{
	    _currentText = _defaultText;
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
            _currentText = col.GetComponent<TextItem>().Text;
            SetActive(true);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (!_isActive)
            return;
        if (col.GetComponent<TextItem>())
        {
            _currentText = _defaultText;
            SetActive(false);
        }
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
        return _currentText;
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
