using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prey : Animal
{
    [Header("Prey Variables")]
    [SerializeField] private float detectionRange = 10;
    [SerializeField] private float escapeMaxDistance = 80f;
    private Predator currentPredator = null;

    public void AlertPrey(Predator predator){
        SetState(AnimalState.Chase);
        currentPredator = predator;
        StartCoroutine(RunFromPredator());
    }

    private IEnumerator RunFromPredator(){
        while(currentPredator == null || Vector3.Distance(transform.position, currentPredator.transform.position) > detectionRange)
            yield return null;
        

        while(currentPredator != null && Vector3.Distance(transform.position, currentPredator.transform.position) <= detectionRange){
            RunAwayPredator();
            yield return null;
        }
        if(!navMeshAgent.pathPending && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            yield return null;
        
        SetState(AnimalState.Idle);
    }

    private void RunAwayPredator(){
        if(navMeshAgent != null && navMeshAgent.isActiveAndEnabled){
            if(!navMeshAgent.pathPending && navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance){
                Vector3 runDirection = transform.position - currentPredator.transform.position;
                Vector3 escapeDestination = transform.position + runDirection.normalized * (escapeMaxDistance * 2);
                navMeshAgent.SetDestination(GetRandomNavMeshPosition(escapeDestination, escapeMaxDistance));
            }
        }
    }
    protected override void Die(){
        StopAllCoroutines();
        base.Die();
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
            
    }
}
