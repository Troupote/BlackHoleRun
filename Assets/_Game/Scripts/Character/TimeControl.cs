using UnityEngine;
using System.Collections;

public class TimeControl : MonoBehaviour
{
    public bool isSlowed = true;
    public bool isFinished = false;
    public bool isStarted = false;

    public IEnumerator SlowmotionSequence()
    {
        if (!isStarted)
        {
            isStarted = true;
            isSlowed = true;
            isFinished = false;

            StartCoroutine(ChangeTimeScale(1f, 0.1f, 0.6f));

            //Wait until isSlowed becomes false
            yield return new WaitUntil(() => isSlowed == false);

            StartCoroutine(ChangeTimeScale(Time.timeScale, 1f, 0.6f));

            isFinished = true;
        }
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
