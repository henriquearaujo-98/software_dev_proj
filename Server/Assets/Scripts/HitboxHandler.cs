using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Hitbox { Head, Torso, Legs, Arms };

public class HitboxHandler : MonoBehaviour
{
    public Player playerScript;
    [HideInInspector] public float damageMultiplier;

    
    public Hitbox type;

    private void Start()
    {
        switch (type)
        {
            case Hitbox.Head: damageMultiplier = 4; 
                break;
            case Hitbox.Torso: damageMultiplier = 1.2f; 
                break;
            case Hitbox.Legs: damageMultiplier = 0.9f;
                break;
            case Hitbox.Arms: damageMultiplier = 0.9f;
                break;
            default: damageMultiplier = 1; 
                break;
        }
    }
}
