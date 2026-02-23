using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevilController : MonsterController
{
    int AggressionLevel = 0;
    float NormalSpeed;

    [Header("Hunting Settings")]
    public float HuntingSpeed = 10;
    public float HuntSpeedIncrease = 1.5f;
    public float MaxTimeBeforeHunt = 0;
    public float MaxHuntTime = 15f;

    public AggresionState aggroState;
    // Defines what monster is currently doing
    public enum AggresionState
    {
        Inactive,
        Hunting
    }

    float TimeBeforeHunt;
    float HuntTime;

    public override void OnAwake()
    {
        base.OnAwake();
        NormalSpeed = base.Speed;
        TimeBeforeHunt = MaxTimeBeforeHunt;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (aggroState != AggresionState.Hunting)
        {
            TimeBeforeHunt -= Time.deltaTime;
            if (TimeBeforeHunt <= 0)
            {
                aggroState = AggresionState.Hunting;
                TimeBeforeHunt = MaxTimeBeforeHunt;
            }
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            IncreaseAggression();
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        switch (aggroState)
        {
            case AggresionState.Hunting:
                base.Speed = HuntingSpeed;
                break;
            case AggresionState.Inactive:
                base.Speed = NormalSpeed;
                break;
        }
    }

    public void IncreaseAggression()
    {
        Debug.Log("Aggression " + AggressionLevel);
        AggressionLevel++;
        HuntingSpeed *= HuntSpeedIncrease;
        VisionRange += 4;
        LoseSightRange += 6;
    }
    public override void SeesPlayer()
    {
        if (aggroState == AggresionState.Hunting) base.SeesPlayer();
        Debug.Log("ThisTooWorks");
    }

    public override void HandleRoaming()
    {
        if (aggroState == AggresionState.Hunting)
        {
            MonAI.speed = base.Speed;

            // Reached destination, idle there then choose next destination randomly
            if (MonAI.remainingDistance <= MonAI.stoppingDistance + 0.1f)
            {
                SetRandomDestination(WanderRadius);
            }
        }
        else base.HandleRoaming();

    }
}

