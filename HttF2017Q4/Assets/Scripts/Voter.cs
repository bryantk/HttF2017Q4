using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Voter : MonoBehaviour
{

	public TextMeshPro Text;
	public DropManager Manager;
	public ParticleSystem Particles;

	public int Count;



	private string voteString = "{0} Vote{1} Left";

	private Coroutine vote;



	// Use this for initialization
	void Start ()
	{
		SetText();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other)
	{
		Count++;
		SetText();
	}

	void OnTriggerExit(Collider other)
	{
		Count--;
		SetText();
	}

	private void SetText()
	{
		if (Count == 0)
		{
			if (vote != null)
				StopCoroutine(vote);
			Text.text = "We Require Additional Votes!";
			return;
		}

		var total = GNM.Instance.Players;
		var need = Mathf.Max(Mathf.FloorToInt(total*0.75f), 0);

		var left = Mathf.Max(need - Count, 0);
		if (left > 0)
		{
			if (vote != null)
				StopCoroutine(vote);
			Text.text = string.Format(voteString, left, left > 1 ? "s" : "");
			return;
		}

		vote = StartCoroutine(VoteProccess());
	}

	IEnumerator VoteProccess()
	{
		Text.text = "3";
		yield return new WaitForSeconds(0.5f);
		Text.text = "2";
		yield return new WaitForSeconds(0.5f);
		Text.text = "1";
		yield return new WaitForSeconds(0.5f);
		if (Manager.Correct)
		{
			Text.text = "Poor Grasshopper";
			Particles.Stop();
			yield return new WaitForSeconds(1f);
			GNM.Instance.EndGame(true);
		}
		else
		{
			Text.text = "Wrong Story";
		}
	}

}
