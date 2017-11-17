using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class PlayerObject : MonoBehaviour {

	public float Speed = 3;

    [SerializeField] private NavMeshAgent _navAgent;

	public HUD HUD;
	public GameObject Player;
	public GameObject Fog;
	public GameObject PlayerCamera;
	public EventSystem EventSystem;
	private bool _playerControlled = true;

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

		var location = Fog.transform.position;
		location.y = 0;
		Fog.transform.position = location;

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
		Player.transform.LookAt(_navAgent.steeringTarget);
	    var rot = Player.transform.rotation.eulerAngles;
	    rot.x = 0;
	    rot.z = 0;
		Player.transform.rotation = Quaternion.Euler(rot);

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
					//Drop any selected item if possible
					var path = new NavMeshPath();
					if (HUD.SelectedItem != null)
					{
						//Can we get to the point clicked?
						if (NavMesh.CalculatePath(Player.transform.position, hit.point, NavMesh.AllAreas, path))
						{
							//Is the point too close to the edge?
							NavMeshHit meshHit;
							NavMesh.FindClosestEdge(hit.point, out meshHit, NavMesh.AllAreas);
							if (meshHit.distance < 1.5) break;
							HUD.SelectedItem.Drop(hit.point, true, true);
							HUD.RemoveSelectedInventoryItem();
							break;
						}
					}
					//Otherwise, move there
					MoveTo(hit.point, true);
					break;
				case "Interactable":
					var distance = Vector3.Distance(hit.point, Player.transform.position);
					var maxDistance = 2;
					if (distance > maxDistance)
					{
						var moveTowards = Vector3.MoveTowards(Player.transform.position, hit.point, distance - maxDistance + .5f);
						MoveTo(moveTowards, true);
						return;
					}
					var textItem = hit.transform.GetComponent<TextItem>();
					HUD.AddInventoryItem(textItem);
					break;
				case "DropZone":
					var selectedText = HUD.SelectedItem;
					var dropZone = hit.transform.GetComponent<DropZone>();

					if (dropZone.HasItem)
					{
						if (selectedText == null)
						{
							//pick up book in zone
							HUD.AddInventoryItem(dropZone.CurrentTextItem);
							break;
						}
						//todo swap books
						break;
					}
					if (selectedText == null)
					{
						break;
					}
					selectedText.Drop(hit.point, true);
					HUD.RemoveSelectedInventoryItem();
					break;
			}
		}
	}

	public void MoveTo(Vector3 location, bool sendMessage = false)
	{
		MoveToLocation(location);

		if (sendMessage)
			GNM.Instance.SendData(ILMsgType.MoveTo, JsonUtility.ToJson(location));
	}

	public void Emote(string msgMessage, bool sendMessage = false)
	{
		Debug.Log("EMOTING: " + msgMessage);

		var name = "emotes_angry";
		if (msgMessage == "Happy")
			name = "emotes_happy";
		var emotePrefab = (GameObject)Resources.Load(name);
		var emote = Instantiate(emotePrefab, transform);
		emote.transform.position = Player.transform.position + Vector3.up;

		emote.transform.DOMoveY(2, 2).OnComplete(()=>Destroy(emote));

		if (sendMessage)
			GNM.Instance.SendDataUnreliable(ILMsgType.Emote, msgMessage);
	}
}
