using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100f;
    public MeshRenderer model;
    [SerializeField] DamageIndicator DamageIndicator;
    [SerializeField] GameObject DamageFrom;
    Transform DamageFromTransform;
    [SerializeField] Animator canvasAnim;
    public KillFeedHandler killFeedHandler;


    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }
    }

    public void SetDamageIndicator(Vector3 _damageFrom)
    {
        GameObject temp = Instantiate(DamageFrom, _damageFrom, Quaternion.identity);
        DamageFromTransform = temp.transform;
        Invoke("RegisterDamageIndicator", 0f);
    }

    void RegisterDamageIndicator()
    {
        if (!DI_System.CheckIfObjectInSight(DamageFromTransform))
        {
            DI_System.CreateIndicator(DamageFromTransform);
        }
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }

    public void showHitmarker()
    {
        canvasAnim.Play("Hitmarker", 0, 0f);
    }
}
