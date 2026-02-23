using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxController : MonoBehaviour
{
    //Manager for Hurtboxes.
    //Notices when triggers collide and calls HitBegins on the CurrentAction of the Hurtbox's owner,
    //  telling them who they hit.

    //Who owns this hurtbox
    public MonsterController Who;

    void Awake()
    {
        //If you didn't set Who, set it automatically
        if (Who == null) Who = gameObject.GetComponentInParent<MonsterController>();
    }

    //When I hit a hitbox. . .
    private void OnTriggerEnter2D(Collider2D other)
    {
        //If I hit myself, don't do anything
        if (other.gameObject == Who.gameObject) return;
        //Find out if what I hit has a Hitbox
        HitboxController hit = other.GetComponent<HitboxController>();
        //If not, don't do anything
        if (hit == null) return;

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //If I hit myself, don't do anything
        if (other.gameObject == Who.gameObject) return;
        //Find out if what I stopped hitting has a Hitbox
        HitboxController hit = other.GetComponent<HitboxController>();
        //If not, don't do anything
        if (hit == null) return;

    }

}

