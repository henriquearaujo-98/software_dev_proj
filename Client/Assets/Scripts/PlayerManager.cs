using System.Collections;
using System.Collections.Generic;
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
    PlayerController pc;
    public bool[] serverInputs; //inputs of this player on the server



    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        pc = GetComponent<PlayerController>();
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
