using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxHandler : MonoBehaviour
{
    public Player playerScript;
    [HideInInspector] public float damageMultiplier;

    enum Type { Head, Torso, Legs, Arms};
    [SerializeField] Type type;

    private void Start()
    {
        switch (type)
        {
            case Type.Head: damageMultiplier = 4; 
                break;
            case Type.Torso: damageMultiplier = 1.2f; 
                break;
            case Type.Legs: damageMultiplier = 0.9f;
                break;
            case Type.Arms: damageMultiplier = 0.9f;
                break;
            default: damageMultiplier = 1; 
                break;
        }
    }
}
