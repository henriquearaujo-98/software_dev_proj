using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedHandler : MonoBehaviour
{

    [SerializeField] GameObject KillFeedItem;

    public void InstantiateKillFeedItem(string _text)
    {
        GameObject kf = Instantiate(KillFeedItem, transform.position, Quaternion.identity);
        kf.transform.parent = gameObject.transform;
        kf.GetComponent<Text>().text = _text;
    }
}
