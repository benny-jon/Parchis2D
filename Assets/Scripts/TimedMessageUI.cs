using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimedMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text eventNotification;
    [SerializeField] private float fadeDuration = 0.15f;

    private Coroutine hideCoroutine;

    public void Show(string message, int seconds)
    {
        eventNotification.text = message;

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        eventNotification.gameObject.SetActive(true);
        eventNotification.alpha = 1;

        hideCoroutine = StartCoroutine(HideAfter(seconds));
    }

    public void Hide()
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        eventNotification.alpha = 0;
        eventNotification.gameObject.SetActive(false);
        hideCoroutine = null;
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            eventNotification.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        eventNotification.alpha = 0;
        eventNotification.gameObject.SetActive(false);
        hideCoroutine = null;
    }
}
