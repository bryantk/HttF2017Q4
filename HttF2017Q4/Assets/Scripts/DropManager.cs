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
        DZTemplate.transform.position = new Vector3(-100f, -100f, -100f);

        DZTemplate.SetSize(unitSize);

        for (int i = 0; i < numberOfZones; i++)
        {
            GameObject newZoneGO = Instantiate(DZTemplate.gameObject);
            DropZone newZone = newZoneGO.GetComponent<DropZone>();

            zones.Add(newZone.GetComponent<DropZone>());

            newZoneGO.transform.position = Vector3.Lerp(_minPoint.position, _maxPoint.position, (float)i / ((float)numberOfZones-1));
            newZoneGO.transform.parent = transform;
            newZone.DropManager = this;
        }

    }

    // Use this for initialization
	void Start ()
	{
	    Generate(_initialNumberOfZones);
	}

    public void UpdateText()
    {
        _fullText = "";
        int currentID = 0;
        bool correctOrderSoFar = true;
        foreach (DropZone zone in zones)
        {
            _fullText += zone.GetCurrentText();
            if (zone.CurrentTextItem != null && zone.CurrentTextItem.ID > currentID)
            {
                currentID = zone.CurrentTextItem.ID;
            }
            else
            {
                correctOrderSoFar = false;
            }
        }
        _textMeshGround.text = _fullText;

        if (correctOrderSoFar)
            OrderCorrect();
    }

    public void OrderCorrect()
    {
        Debug.Log("Order Is Correct!");
    }
}
