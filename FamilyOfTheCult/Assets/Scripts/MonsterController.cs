using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// The script for anything that can perform Actions within the game, and the parent of both PlayerController and MonsterController.
public class MonsterController : MonoBehaviour
{
    //---Action Variables---
    public string DefaultAnim = "Idle";

    //---Character Stat Variables---
    public float Speed;
    public float SlowDownAmount = 0.5f;

    public float WanderRadius = 15f;
    public float AttackRange = 1.5f;
    public float VisionRange = 4;
    public float LoseSightRange = 6;
    bool CurrentlyIdle = false;
    public float minIdleTime = 1.5f;
    public float maxIdleTime = 3.5f;

    public float MaxBurnTime = 0;
    float BurnAmount;

    public bool isInLight = false;

    /// Where you spawned into the level. Used for some AI stuff
    public Vector3 StartSpot;

    //---Component Variables--- (hidden because they're set in code)
    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public Animator Anim;
    [HideInInspector] public NavMeshAgent MonAI;
    [HideInInspector] public ControllerForPlayer Target;
    [HideInInspector] public Transform player;

    public BehaviorState targetState;
    // Defines what monster is currently doing
    public enum BehaviorState
    {
        Roaming,
        Idle,
        Stunned,
        Pursuing,
    }

    void Awake() { OnAwake(); }
    public virtual void OnAwake()
    {
        RB = GetComponent<Rigidbody>();
        Anim = GetComponent<Animator>();
        MonAI = GetComponent<NavMeshAgent>();
        Target = FindAnyObjectByType<ControllerForPlayer>();
        player = FindAnyObjectByType<ControllerForPlayer>().transform;

        StartSpot = transform.position;

        targetState = BehaviorState.Roaming;
        MonAI.speed = Speed;
        BurnAmount = MaxBurnTime;
    }

    public void Start() { OnStart(); }
    public virtual void OnStart()
    {
        //Make sure I start with full health
        BurnAmount = MaxBurnTime;
    }

    void FixedUpdate() { OnFixedUpdate(); }
    public virtual void OnFixedUpdate()
    {
        switch (targetState)
        {
            case BehaviorState.Roaming:
                HandleRoaming();
                break;
            case BehaviorState.Stunned:
                HandleStun();
                break;
            case BehaviorState.Pursuing:
                HandlePursuit();
                break;
            case BehaviorState.Idle:
                if (CurrentlyIdle) StartCoroutine(HandleIdle());
                break;
        }
        Debug.Log(CurrentlyIdle);
    }

    public virtual void ChangeState(BehaviorState newState)
    {
        targetState = newState;
    }

    void Update()
    {
        OnUpdate();
        CanSeePlayer();
        //MonAI.speed = currSpeed;
        Debug.Log(MonAI.speed);
    }
    public virtual void OnUpdate() { }

    void CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, Target.transform.position);

        if (distance < VisionRange)
        {
            Ray ray = new Ray(transform.position + Vector3.up, (Target.transform.position - transform.position).normalized);

            if (Physics.Raycast(ray, out RaycastHit hit, VisionRange))
            {
                ControllerForPlayer playerScript = hit.collider.GetComponent<ControllerForPlayer>();
                if (playerScript != null)
                {
                    SeesPlayer();
                }
            }
        }
    }
    public virtual void SeesPlayer()
    {
        Debug.Log("TheSee");
        ChangeState(BehaviorState.Pursuing);
    }
    public void SetRandomDestination(float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += StartSpot; // Offset by the starting position

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
            MonAI.SetDestination(hit.position);
    }

    ///Called when you get hit by an attack or otherwise get hurt. Manages health and checks to see if you died
    public virtual void TakeDamage(float amt)
    {
        //If you don't have HP, you're immune to damage
        if (MaxBurnTime <= 0) return;
        //Lower your HP by the damage you took
        BurnAmount -= amt;
        //If you're at 0HP or less, you die
        if (BurnAmount <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(float amt)
    {
        if (MaxBurnTime <= 0)
            return;


        BurnAmount += amt;
        BurnAmount = Mathf.Clamp(BurnAmount, 0, MaxBurnTime);
    }

    public void ApplyLightExposure(float amount)
    {

        Debug.Log("Burning");
        BurnAmount -= amount;
        float currSpeed = Speed * SlowDownAmount;
        MonAI.speed = currSpeed;
        
        //currentLightExposure = Mathf.Clamp(currentLightExposure, 0, maxLightExposure);

        if (BurnAmount <= 0 ) Die();
    }

    public virtual void StopLightExposure()
    {
        Debug.Log("StoppedLightExposure");
        isInLight = false;
        MonAI.speed = Speed;
    }
    

    ///Called when you hit 0HP. Just deletes your GameObject for now.
    public virtual void Die()
    {
        Destroy(gameObject);
    }

    public virtual void HandlePursuit()
    {
        MonAI.destination = player.transform.position;
    }

    public virtual void HandleRoaming()
    {
        MonAI.speed = Speed;

        // Reached destination, idle there then choose next destination randomly
        if (MonAI.remainingDistance <= MonAI.stoppingDistance + 0.1f)
        {
            SetRandomDestination(WanderRadius);
            CurrentlyIdle = true;
            ChangeState(BehaviorState.Idle);
        }
    }

    public virtual IEnumerator HandleIdle()
    {
        CurrentlyIdle = false;
        float idleTime = UnityEngine.Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);
        ChangeState(BehaviorState.Roaming);
    }

    public virtual void HandleStun()
    {

    }

    public virtual void LoseSight()
    {
        float distance = Vector3.Distance(transform.position, Target.transform.position);
        if (distance > LoseSightRange)
        {
            ChangeState(BehaviorState.Roaming);
        }
    }
}
