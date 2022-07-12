using System;
using System.Collections;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{

    private const float MaxTimer = 10f;
    [SerializeField]
    private float timer = MaxTimer;

    private CanvasGroup canvasGroup = null;
    public CanvasGroup CanvasGroup
    {
        get
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            return canvasGroup;
        }
    }

    private RectTransform rect = null;
    public RectTransform Rect
    {
        get
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
                if (rect == null)
                {
                    rect = gameObject.AddComponent<RectTransform>();
                }
            }
            return rect;
        }
    }

    public Transform Target { get; private set; } = null;
    private Transform player = null;

    private IEnumerator IE_Countdown = null;
    private Action unRegister = null;

    Vector3 direction = Vector3.zero;
    Quaternion rotation = Quaternion.identity;
    Vector3 targetPos = Vector3.zero;

    public void Register(Transform target, Transform player, Action unRegister)
    {
        this.Target = target;
        this.player = player;
        this.unRegister = unRegister;

        StartCoroutine(RotateToTheTarget());
        StartTimer();
    }
    public void Restart()
    {
        timer = MaxTimer;
        StartTimer();
    }
    private void StartTimer()
    {
        if (IE_Countdown != null) { StopCoroutine(IE_Countdown); }
        IE_Countdown = Countdown();
        StartCoroutine(IE_Countdown);
    }

    IEnumerator RotateToTheTarget()
    {
        while (enabled)
        {
            if (Target)
            {
                targetPos = Target.position;
                rotation = Target.transform.rotation;
            }
            direction = player.transform.position - targetPos;

            rotation = Quaternion.LookRotation(direction);
            rotation.z = -rotation.y;
            rotation.x = 0;
            rotation.y = 0;

            Vector3 northDirection = new Vector3(0, 0, player.transform.eulerAngles.y);
            Rect.localRotation = rotation * Quaternion.Euler(northDirection);

            yield return null;
        }
    }
    IEnumerator Countdown()
    {
        while (CanvasGroup.alpha < 1.0f)
        {
            CanvasGroup.alpha += 4 * Time.deltaTime;
            yield return null;
        }
        while (timer > 0)
        {
            timer--;
            yield return new WaitForSeconds(1);
        }
        while (CanvasGroup.alpha > 0.0f)
        {
            CanvasGroup.alpha -= 2 * Time.deltaTime;
            yield return null;
        }
        unRegister();
        Destroy(gameObject);
    }
}