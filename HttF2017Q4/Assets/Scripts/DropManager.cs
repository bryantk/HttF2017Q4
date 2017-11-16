using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropManager : MonoBehaviour
{

    [SerializeField]
    private Transform _maxPoint;
    [SerializeField]
    private Transform _minPoint;

    [SerializeField] private int _initialNumberOfZones = 10;
    [Range(0f, 1f)]
    [SerializeField] private float _padding;

    [SerializeField] private DropZone _dropZonePrefab;
    [SerializeField] private TextMeshPro _textMeshGround;

    private List<DropZone> zones = new List<DropZone>();

    private string _fullText;

    public void Generate(int numberOfZones)
    {
        foreach (DropZone zone in zones)
        {
            zones.Remove(zone);
            DestroyImmediate(zone.gameObject);
        }

        float length = Vector3.Distance(_maxPoint.position, _minPoint.position);
        float unitSize = length / numberOfZones;
        unitSize *= 1f -_padding;

        DropZone DZTemplate = Instantiate(_dropZonePrefab).GetComponent<DropZone>();

        DZTemplate.SetSize(unitSize);

        for (int i = 0; i < numberOfZones; i++)
        {
            GameObject newZone = Instantiate(DZTemplate.gameObject);
            zones.Add(newZone.GetComponent<DropZone>());

            newZone.transform.position = Vector3.Lerp(_minPoint.position, _maxPoint.position, (float)i / ((float)numberOfZones-1));
        }

    }

    // Use this for initialization
	void Start ()
	{
	    Generate(_initialNumberOfZones);
	}
	
	// Update is called once per frame
	void Update () {
		CreateText();
	}

    private void CreateText()
    {
        _fullText = "";
        foreach (DropZone zone in zones)
        {
            _fullText += zone.GetCurrentText();
        }
        _textMeshGround.text = _fullText;
    }
}
