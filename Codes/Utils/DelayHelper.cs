using UnityEngine;
using System;
using System.Collections;

public static class DelayHelper
{
    /// <summary>
    /// Waits for seconds then sends a call back.
    /// </summary>
    public static void Wait(float delay, Action callback)
    {
        GameObject temp = new GameObject("DelayHelper");
        temp.hideFlags = HideFlags.HideAndDontSave;
        temp.AddComponent<DelayHelperBehaviour>().StartDelay(delay, callback);
    }


    public static void WaitForAFrame(Action callback)
    {
        Wait(0f, callback);
    }
}

public class DelayHelperBehaviour : MonoBehaviour
{
    public void StartDelay(float delay, Action callback)
    {
        StartCoroutine(DelayCoroutine(delay, callback));
    }

    private IEnumerator DelayCoroutine(float delay, Action callback)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return null; // Wait for a frame
        }
        callback?.Invoke();
        Destroy(gameObject);
    }
}