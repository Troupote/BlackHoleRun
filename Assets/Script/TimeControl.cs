using UnityEngine;
using System.Collections;

public class TimeControl : MonoBehaviour
{
    private Coroutine timeCoroutine;
    public float aimTime = 2f;

    private bool isInSlowmo = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (timeCoroutine != null)
            {
                StopCoroutine(timeCoroutine);
            }

            if (!isInSlowmo)
            {
                timeCoroutine = StartCoroutine(SlowmotionSequence());
            }
            else
            {
                timeCoroutine = StartCoroutine(ChangeTimeScale(Time.timeScale, 1f, 1f));
                isInSlowmo = false;
            }
        }
    }

    IEnumerator SlowmotionSequence()
    {
        isInSlowmo = true;

        yield return StartCoroutine(ChangeTimeScale(1f, 0.1f, .6f));

        yield return new WaitForSecondsRealtime(aimTime);

        yield return StartCoroutine(ChangeTimeScale(Time.timeScale, 1f, .6f));

        isInSlowmo = false;
    }

    IEnumerator ChangeTimeScale(float start, float end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Time.timeScale = Mathf.Lerp(start, end, elapsed / duration);
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = end;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}
