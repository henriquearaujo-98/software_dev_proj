using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100f;
    public MeshRenderer model;

    public int kills;
    public int deaths;

    public PlayerController pc;
    
    

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        DontDestroyOnLoad(gameObject);
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }
    }

    

    public void Die()
    {
        if (GetComponent<Enemy>())
        {
            GetComponent<Enemy>().SpawnOnDeath();
        }
    }

    public void Respawn()
    {
        SetHealth(maxHealth);
    }


}
