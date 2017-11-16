using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMover : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _navAgent;

    [SerializeField] private Transform destination;

    public void MoveToLocation(Vector3 location)
    {
        _navAgent.SetDestination(location);
    }

    void Start()
    {
        MoveToLocation(destination.position);
    }
}
