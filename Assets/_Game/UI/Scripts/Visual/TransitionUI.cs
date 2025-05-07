using BHR;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TransitionUI : MonoBehaviour
{
    [SerializeField, Required, FoldoutGroup("Refs")] private TextMeshProUGUI _counter;
    public float startScale, scale3, scale2, scale1, scale0;
    public float startTextScale, textScale3, textScale2, textScale1, textScale0;
    public Color color3, color2, color1, color0;
    [SerializeField, Required, FoldoutGroup("Settings")] private float _maxScale;
    [SerializeField, Required, FoldoutGroup("Settings")] private Ease _ease;

    public void LaunchTransitionAnimation(bool state)
    {
        gameObject.SetActive(true); _counter.gameObject.SetActive(false);
        StartCoroutine(TransitionAnimation(state));
    }

    public void LaunchStartAnimation()
    {
        gameObject.SetActive(true); _counter.gameObject.SetActive(false);
#if UNITY_EDITOR
        if (DebugManager.Instance.DisableStartAnimation)
        {
            ModuleManager.Instance.LaunchTransitionAnimation(false);
            GameManager.Instance.StartLevel();
        }
        else
#endif
        StartCoroutine(StartAnimation());
    }

    IEnumerator TransitionAnimation(bool starting)
    {
        float targetScale = starting ? _maxScale : 0f;
        transform.localScale = starting ? Vector3.zero : _maxScale * Vector3.one;

        transform.DOScale(targetScale, ModuleManager.Instance.TransitionDuration).SetEase(_ease);
        yield return new WaitForSeconds(ModuleManager.Instance.TransitionDuration);
        if (!starting) gameObject.SetActive(false);
    }

    IEnumerator StartAnimation()
    {
        // Init
        _counter.gameObject.SetActive(false);
        transform.localScale = Vector3.one * startScale;
        _counter.color = color3;
        _counter.text = "3";
        _counter.transform.localScale = Vector3.one * startTextScale;

        transform.DOScale(scale3, 0.4f);
        yield return new WaitForSeconds(0.4f);

        gameObject.SetActive(true);
        _counter.gameObject.SetActive(true);
        _counter.DOColor(color2, 1f);
        _counter.transform.DOScale(textScale3, 1f);
        transform.DOScale(scale2, 1f);
        yield return new WaitForSeconds(1f);

        // 2->1
        _counter.text = "2";
        _counter.DOColor(color1, 1f);
        _counter.transform.DOScale(textScale2, 1f);
        transform.DOScale(scale1, 1f);
        yield return new WaitForSeconds(1f);

        // 1-> Go
        _counter.text = "1";
        _counter.DOColor(color0, 1f);
        transform.DOScale(scale0, 1f);
        _counter.transform.DOScale(textScale1, 1f);
        yield return new WaitForSeconds(1f);

        // GO
        _counter.text = "GO";

        _counter.DOColor(Color.black, 0.3f);
        transform.DOScale(0, 0.3f);
        _counter.transform.DOScale(textScale0, 0.3f);

        yield return new WaitForSeconds(0.3f);
        gameObject.SetActive(false);
        _counter.gameObject.SetActive(false);
        GameManager.Instance.StartLevel();
    }

    [Button] void TestAnim()
    {
        gameObject.SetActive(true);

        StartCoroutine(TestCoroutine());
    }

    IEnumerator TestCoroutine()
    {
        LaunchTransitionAnimation(true);
        yield return new WaitForSeconds(ModuleManager.Instance.TransitionDuration);
        LaunchTransitionAnimation(false);
    }
}
