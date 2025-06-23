using BHR;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.5f;

    private void Awake()
    {
        if (flashImage != null)
        {
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;
        }
    }

    public void Flash()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float halfDuration = flashDuration / 2f;
        float timer = 0f;

        // Fade in
        yield return new WaitForSeconds(1f);
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, timer / halfDuration));
            yield return null;
        }
        SetAlpha(1f);

        CharactersManager.Instance.DisableCharacterAndSingularityControllerScripts();
        CharactersManager.Instance.DisableCharacterAndSingularityBehaviorScripts();
        CharactersManager.Instance.IsEndingCinematicStarted = true;

        yield return new WaitForSeconds(4f);

        GameManager.Instance.EndLevel(true);
        // Fade out
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, timer / halfDuration));
            yield return null;
        }

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (flashImage != null)
        {
            Color color = flashImage.color;
            color.a = alpha;
            flashImage.color = color;
        }
    }
}
