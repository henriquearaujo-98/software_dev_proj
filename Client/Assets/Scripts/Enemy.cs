using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField] Canvas enemyCanvas;
    [SerializeField] GameObject EnemyModel;
    [SerializeField] private Image foregroundImage;
    [SerializeField] Text UsernameDisplay;
    [SerializeField] GameObject spawnOnDeath;

    [SerializeField] Animator anim;

    public bool[] serverInputs;

    PlayerManager pm;
    GameObject mainPlayer;

    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<PlayerManager>();
        mainPlayer = GameObject.FindGameObjectWithTag("Player");
        serverInputs = new bool[9];
    }

    // Update is called once per frame
    void Update()
    {

        if(pm.health <= 0)
        {
            enemyCanvas.gameObject.SetActive(false);
            EnemyModel.SetActive(false);
            return;
        }
        else
        {
            enemyCanvas.gameObject.SetActive(true);
            EnemyModel.SetActive(true);
        }

        foregroundImage.fillAmount = pm.health / pm.maxHealth;
        UsernameDisplay.text = pm.username;

        AnimationHandler();
    }


    private void LateUpdate()
    {
        enemyCanvas.transform.LookAt(mainPlayer.transform);
        //enemyCanvas.transform.Rotate(0, 180, 0);
    }

    void AnimationHandler()
    {
        anim.SetBool("Forward", serverInputs[0]);

        anim.SetBool("Backwards", serverInputs[1]);


        anim.SetBool("Left", serverInputs[2]);

        anim.SetBool("Right", serverInputs[3]);

        if (serverInputs[0] || serverInputs[1] || serverInputs[2] || serverInputs[3])
        {
            if (serverInputs[5])
                anim.SetBool("Run", true);
            else
                anim.SetBool("Run", false);
        }

        if (serverInputs[7])
            anim.Play("Shoot", 0, 0f);


       // anim.SetBool("Grounded", GetComponent<CharacterController>().isGrounded);

    }

    public void SpawnOnDeath()
    {
        Instantiate(spawnOnDeath, transform.position, Quaternion.identity);
    }
}
