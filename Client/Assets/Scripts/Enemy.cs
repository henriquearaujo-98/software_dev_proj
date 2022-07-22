using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    [SerializeField] private Image foregroundImage;
    [SerializeField] private float updateSpeedSeconds = 0.2f;
    PlayerManager pm;

    public Text UsernameDisplay;

    GameObject mainPlayer;

    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<PlayerManager>();
        mainPlayer = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        foregroundImage.fillAmount = pm.health / pm.maxHealth;
        UsernameDisplay.text = pm.username;
    }

    public void setUsernameUI(string username)
    {
        UsernameDisplay.text = username;
    }

    private void LateUpdate()
    {
        transform.LookAt(mainPlayer.transform);
        transform.Rotate(0, 180, 0);
    }
}
