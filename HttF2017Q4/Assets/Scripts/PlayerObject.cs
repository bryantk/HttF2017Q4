using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class PlayerObject : MonoBehaviour {

	public float Speed = 3;

    [SerializeField] private NavMeshAgent _navAgent;

	private Vector3 inputs;

	public GameObject Player;
	public GameObject Fog;
	public GameObject PlayerCamera;
	public EventSystem EventSystem;
	private bool _playerControlled = true;

	private const int TICKS = 5;

	private int _count;

    void Awake()
    {
        if (_navAgent == null)
            _navAgent = GetComponent<NavMeshAgent>();
    }

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (!_playerControlled) return;
		if (Input.GetMouseButton(0) && !EventSystem.IsPointerOverGameObject())
		{
			HandleClick();
		}

		inputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		transform.position = transform.position + inputs * Time.deltaTime * Speed;
	}

	public void SetAsClient()
	{
		_playerControlled = false;
		Destroy(Fog);
		Destroy(PlayerCamera);
	}

    public void MoveToLocation(Vector3 location)
    {
        _navAgent.SetDestination(location);
    }

	private void HandleClick()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			var objectTag = hit.transform.tag;
			switch (objectTag)
			{
				case "Map":
					SendMove(hit.point);
					break;
				case "Interactable":
					var distance = Vector3.Distance(hit.point, Player.transform.position);
					var maxDistance = 2;
					if (distance > maxDistance)
					{
						var moveTowards = Vector3.MoveTowards(Player.transform.position, hit.point, distance - maxDistance + .5f);
						SendMove(moveTowards);
						return;
					}
					var textItem = hit.transform.GetComponent<TextItem>();
					if (textItem == null)
					{
						Debug.LogWarning("Interactable was not text item");
						return;
					}
					textItem.PickUp();
					break;
			}
		}
	}

	private void SendMove(Vector3 location)
	{
		MoveToLocation(location);
		GNM.Instance.SendData(ILMsgType.MoveTo, JsonUtility.ToJson(location));
	}

	public void Emote(string msgMessage)
	{
		Debug.Log("EMOTING");
	}
}
