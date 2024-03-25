using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum AnimalState{
    Idle,
    Moving, 
    Chase,
}
[RequireComponent(typeof(NavMeshAgent))]
public class Animal : MonoBehaviour
{
    [Header("Wander")] 
    [SerializeField] private float wanderDistance = 50f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float maxWalkTime = 6f;

    [Header("Idle")]
    [SerializeField] private float idleTime = 5f;

    [Header("Chase")]
    [SerializeField] private float runSpeed = 8f; 

    [Header("Attributes")]
    [SerializeField] private int health = 10;

    [Tooltip("In Animator you have 3 state 'Idle','Moving' and 'Chase' they controlled by script if you want to change you can drag and drop your animation and rename them. Dont forget to place animator to 3. child object")]
    [SerializeField] protected Animator anim;
    protected NavMeshAgent navMeshAgent;
    protected AnimalState currentState = AnimalState.Idle;

    private void Start() {
        InitialiseAnimal();
    }
    protected virtual void InitialiseAnimal(){
        anim = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = walkSpeed;

        currentState = AnimalState.Idle;
        UpdateState();
    }

    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case AnimalState.Idle:
                HandleIdleState();
                break;
            case AnimalState.Moving:
                HandleMovingState();
                break;
            case AnimalState.Chase:
                HandleChaseState();
                break;
        }
    }
    protected Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance){
        for(int i = 0; i < 5; i++){
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        
            randomDirection += origin;
            NavMeshHit navMeshHit;

            if(NavMesh.SamplePosition(randomDirection, out navMeshHit, distance, NavMesh.AllAreas)){
                return navMeshHit.position;
            }
        }
        return origin;
        
    }
    protected virtual void CheckChaseCondition(){}
    protected virtual void HandleChaseState(){
        StopAllCoroutines();
    }
    protected virtual void HandleIdleState(){
        StartCoroutine(WaitToMove());
    }

    private IEnumerator WaitToMove()
    {
        float WaitTime = UnityEngine.Random.Range(idleTime / 2f, idleTime * 2);
        yield return new WaitForSeconds(WaitTime);

        Vector3 randomDestination = GetRandomNavMeshPosition(transform.position, wanderDistance);
        navMeshAgent.SetDestination(randomDestination);
        SetState(AnimalState.Moving);

    }

    protected virtual void HandleMovingState(){
        StartCoroutine(WaitToReachDestination());
    }

    private IEnumerator WaitToReachDestination()
    {
        float startTime = Time.time;
        while(navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance && navMeshAgent.isActiveAndEnabled){
            if(Time.time - startTime >= maxWalkTime){
                navMeshAgent.ResetPath();
                SetState(AnimalState.Idle);
                yield break;
            }
            CheckChaseCondition();
            yield return null;
        }
        SetState(AnimalState.Idle);
    }
    protected void SetState(AnimalState newState){
        if(currentState == newState)
            return;
        
        currentState  = newState;
        OnStateChange(newState);
        
    }

    protected virtual void OnStateChange(AnimalState newState)
    {
        anim?.CrossFadeInFixedTime(newState.ToString(), 0.5f);
        if(newState == AnimalState.Moving)
            navMeshAgent.speed = walkSpeed;
        if(newState == AnimalState.Chase)
            navMeshAgent.speed = runSpeed;
        UpdateState();
    }
    public virtual void RecieveDamage(int damage, Prey prey, Predator predator){
        Debug.Log(predator.gameObject.name + "Is Damaged the" + prey.gameObject.name);
        health -= damage;
        if(health <= 0)
            Die();
    }
    protected virtual void Die(){
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
