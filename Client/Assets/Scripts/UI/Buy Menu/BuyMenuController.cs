using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyMenuController : MonoBehaviour
{

    [SerializeField] GameObject BuyMenuItem;

    [SerializeField] GameObject PrimaryWeaponContainer;
    [SerializeField] Text PrimaryWeaponConfirm;

    [SerializeField] GameObject SecondaWeaponContainer;
    [SerializeField] Text SecondaryWeaponConfirm;

    // Start is called before the first frame update
    void Start()
    {
        PrimaryWeaponConfirm.text = GameManager.instance.weapons[GameManager.instance.primaryWeaponID].name;
        SecondaryWeaponConfirm.text = GameManager.instance.weapons[GameManager.instance.secondaryWeaponID].name;

        InitializePrimaryWeaponContainer();
        InitializeSecondaryWeaponContainer();
    }

    void InitializePrimaryWeaponContainer()
    {
        foreach (Weapon item in GameManager.instance.weapons)
        {
            GameObject temp = Instantiate(BuyMenuItem, transform.position, Quaternion.identity);
            temp.transform.parent = PrimaryWeaponContainer.transform;
            temp.GetComponent<BuyMenuItem>().text.text = item.name;

            temp.GetComponent<Button>().onClick.AddListener(delegate { 

                GameManager.instance.primaryWeaponID = item.id;
                PrimaryWeaponConfirm.text = item.name;

            });
        }
    }

    void InitializeSecondaryWeaponContainer()
    {
        foreach (Weapon item in GameManager.instance.weapons)
        {
            GameObject temp = Instantiate(BuyMenuItem, transform.position, Quaternion.identity);
            temp.transform.parent = SecondaWeaponContainer.transform;
            temp.GetComponent<BuyMenuItem>().text.text = item.name;

            temp.GetComponent<Button>().onClick.AddListener(delegate { 

                GameManager.instance.secondaryWeaponID = item.id; 
                SecondaryWeaponConfirm.text = item.name;
            });
        }
    }
}
