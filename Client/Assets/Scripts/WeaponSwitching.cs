using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour {

    [Header("References")]
    public Transform[] weapons;
    public Transform currentWeapon;
    public int primaryWeaponID = 0;
    public int secondaryWeaponID = 1;


    [Header("Keys")]
    [SerializeField] private KeyCode[] keys;

    [Header("Settings")]
    [SerializeField] private float switchTime;

    public int selectedWeapon; // 0 - primary; 1 - secondary
    private float timeSinceLastSwitch;

    private void Start() {

        SetWeapons();
        Select(selectedWeapon);

        timeSinceLastSwitch = 0f;
    }

    private void SetWeapons() {
        weapons = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
            weapons[i] = transform.GetChild(i);

        if (keys == null) keys = new KeyCode[weapons.Length];
    }

    private void Update() {
        int previousSelectedWeapon = selectedWeapon;

        for (int i = 0; i < keys.Length; i++)
            if (Input.GetKeyDown(keys[i]) && timeSinceLastSwitch >= switchTime)
                selectedWeapon = i;
            
            
        if (previousSelectedWeapon != selectedWeapon) Select(selectedWeapon);

        timeSinceLastSwitch += Time.deltaTime;
    }

    private void Select(int selectedWeapon) {

        foreach (Transform item in weapons) { item.gameObject.SetActive(false); }

        switch (selectedWeapon)
        {
            case 0:
                currentWeapon = weapons[primaryWeaponID];
                
                weapons[primaryWeaponID].gameObject.SetActive(true);
                weapons[secondaryWeaponID].gameObject.SetActive(false);
                OnWeaponSelected(primaryWeaponID);
                break;

            case 1:
                currentWeapon = weapons[secondaryWeaponID];

                weapons[primaryWeaponID].gameObject.SetActive(false);
                weapons[secondaryWeaponID].gameObject.SetActive(true);
                OnWeaponSelected(secondaryWeaponID);
                break;

        }

        timeSinceLastSwitch = 0f;
    }

    private void OnWeaponSelected(int weaponIndex) { 
        ClientSend.WeaponIndex(weaponIndex);
    }
}