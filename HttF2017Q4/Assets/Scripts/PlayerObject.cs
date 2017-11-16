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
	public GameObject Camera;

	private bool _playerControlled = true;

	private const int TICKS = 20;

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

		inputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		transform.position = transform.position + inputs * Time.deltaTime * Speed;
	}

	void FixedUpdate()
	{
		if (!_playerControlled) return;

		_count++;
		if (_count > TICKS)
		{
			_count = 0;
			var data = JsonUtility.ToJson(transform.position);
			GNM.Instance.SendData(ILMsgType.SetPos, data);
		}
	}


	public void SetAsClient()
	{
		_playerControlled = false;
		Destroy(Fog);
		Destroy(Camera);
	}

    public void MoveToLocation(Vector3 location)
    {
        _navAgent.SetDestination(location);
    }

}
