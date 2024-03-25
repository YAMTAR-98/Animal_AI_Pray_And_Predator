using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Predator : Animal
{
    [Header("Predator Veriables")]
    [SerializeField] private float detectionRange = 20;
    [SerializeField] private float maxChaseTime = 10f;
    [SerializeField] private int biteDamage = 3;
    [SerializeField] private float biteCooldown = 1f;
    private Prey currentChaseTarget;
    
    protected override void CheckChaseCondition()
    {
        if(currentChaseTarget)
            return;

        Collider[] colliders = new Collider[10];
        int numColiders = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, colliders);

        for(int i = 0; i < numColiders; i++){
            Prey prey = colliders[i].GetComponent<Prey>();
            if(prey != null){
                StartChase(prey);
                return;
            }
        }
        currentChaseTarget = null;
    }

    private void StartChase(Prey prey){
        currentChaseTarget = prey;
        SetState(AnimalState.Chase);
    }
    protected override void HandleChaseState(){
        if(currentChaseTarget != null){
            currentChaseTarget.AlertPrey(this);
            StartCoroutine(ChasePrey());
        }else{
            SetState(AnimalState.Idle);
        }
    }

    private IEnumerator ChasePrey(){
        float startTime = Time.time;
        while(currentChaseTarget != null && Vector3.Distance(transform.position, currentChaseTarget.transform.position) > navMeshAgent.stoppingDistance){
            if(Time.time - startTime >= maxChaseTime || currentChaseTarget == null){
                StopChase();
                yield break;
            }
            SetState(AnimalState.Chase);
            navMeshAgent.SetDestination(currentChaseTarget.transform.position);
            yield return null;
        }
        if(currentChaseTarget)
            currentChaseTarget.RecieveDamage(biteDamage, currentChaseTarget, this);

        yield return new WaitForSeconds(biteCooldown);

        currentChaseTarget = null;
        HandleChaseState();
        CheckChaseCondition();
    }

    private void StopChase(){
        navMeshAgent.ResetPath();
        currentChaseTarget = null;
        SetState(AnimalState.Idle);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
            
    }
}
