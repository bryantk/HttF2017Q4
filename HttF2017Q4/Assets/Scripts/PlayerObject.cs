using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class PlayerObject : MonoBehaviour {

	public float Speed = 3;

    [SerializeField] private NavMeshAgent _navAgent;

	private Vector3 inputs;

	public GameObject Player;
	public GameObject Fog;
	public GameObject PlayerCamera;

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
		if (Input.GetMouseButtonDown(0))
		{
			HandleClick();
		}

		inputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		transform.position = transform.position + inputs * Time.deltaTime * Speed;
	}

	void FixedUpdate()
	{
		if (!_playerControlled) return;

		//_count++;
		if (_count > TICKS)
		{
			_count = 0;
			var data = JsonUtility.ToJson(transform.position);
			GNM.Instance.SendDataUnreliable(ILMsgType.SetPos, data);
		}
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
					MoveToLocation(hit.point);
					GNM.Instance.SendData(ILMsgType.MoveTo, JsonUtility.ToJson(hit.point));
					break;
				case "Interactable":
					Debug.Log("Clicked on interactable");
					break;
			}
		}
	}


}
