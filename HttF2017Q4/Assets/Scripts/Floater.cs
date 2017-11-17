using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Floater : MonoBehaviour
{

	public float MinY;
	public float MaxY;
	public float TTM;
	public Ease EaseType = Ease.Linear;

	// Use this for initialization
	void Start ()
	{
		transform.DOMoveY(MinY, 0);

		TTM += Random.Range(-1f, 1f);

		Sequence mySequence = DOTween.Sequence();
		mySequence.Append(transform.DOMoveY(MaxY, TTM));
		mySequence.AppendInterval(0.25f);
		mySequence.Append(transform.DOMoveY(MinY, TTM));
		mySequence.AppendInterval(0.25f);
		mySequence.SetLoops(-1);
		mySequence.SetEase(EaseType);
	}

}
