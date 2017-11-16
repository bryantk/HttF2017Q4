using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerObject : MonoBehaviour {

	public float Speed = 3;
	private Vector3 inputs;

	public GameObject Player;
	public GameObject Fog;
	public GameObject Camera;

	private bool _playerControlled = true;

	private const int TICKS = 5;

	private int _count;

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
			GNM.Instance.SendDataUnreliable(ILMsgType.SetPos, data);
		}
	}


	public void SetAsClient()
	{
		_playerControlled = false;
		Destroy(Fog);
		Destroy(Camera);
	}


}
