using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour {

	public float Speed = 3;
	private Vector3 inputs;

	public GameObject Player;
	public GameObject Fog;
	public GameObject Camera;


	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		inputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		transform.position = transform.position + inputs * Time.deltaTime * Speed;
	}


	public void SetAsClient()
	{
		Destroy(Fog);
		Destroy(Camera);
	}


}
