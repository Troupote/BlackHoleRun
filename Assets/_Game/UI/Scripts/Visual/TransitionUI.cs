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

    public void LaunchTransitionAnimation(bool state, float duration = float.MaxValue)
    {
        gameObject.SetActive(true); _counter.gameObject.SetActive(false);
        StartCoroutine(TransitionAnimation(state, duration == float.MaxValue ? ModuleManager.Instance.DefaultTransitionDuration : duration));
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

    IEnumerator TransitionAnimation(bool starting, float duration)
    {
        float targetScale = starting ? _maxScale : 0f;
        transform.localScale = starting ? Vector3.zero : _maxScale * Vector3.one;

        transform.DOScale(targetScale, duration).SetEase(_ease);
        yield return new WaitForSeconds(duration);
        if (!starting) gameObject.SetActive(false);
    }

    IEnumerator StartAnimation()
    {
        _counter.gameObject.SetActive(true);
        gameObject.SetActive(true);

        IEnumerator AnimateNumber(string text, Color color, float scale, float textScale, float duration)
        {
            _counter.text = text;
            _counter.color = new Color(color.r, color.g, color.b, 0);
            _counter.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(_counter.DOColor(color, 0.2f));
            seq.Join(_counter.transform.DOScale(textScale, 0.4f).SetEase(Ease.OutBack));
            seq.Join(transform.DOScale(scale, 0.4f).SetEase(Ease.OutQuad));
            yield return seq.WaitForCompletion();

            yield return new WaitForSeconds(duration);
        }

        // Animate 3
        yield return AnimateNumber("3", color3, scale3, textScale3, 0.6f);

        // Animate 2
        yield return AnimateNumber("2", color2, scale2, textScale2, 0.6f);

        // Animate 1
        yield return AnimateNumber("1", color1, scale1, textScale1, 0.6f);

        // GO Animation
        _counter.text = "GO";
        _counter.color = new Color(color0.r, color0.g, color0.b, 0);
        _counter.transform.localScale = Vector3.zero;

        Sequence goSeq = DOTween.Sequence();
        goSeq.Append(_counter.DOColor(Color.white, 0.1f));
        goSeq.Join(_counter.transform.DOScale(textScale0, 0.3f).SetEase(Ease.OutBack));
        goSeq.Join(transform.DOScale(scale0, 0.3f).SetEase(Ease.InOutQuad));
        goSeq.Append(_counter.transform.DOShakeScale(0.3f, 0.2f));
        goSeq.Join(_counter.DOFade(0, 0.3f));
        goSeq.Join(transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));
        yield return goSeq.WaitForCompletion();

        _counter.gameObject.SetActive(false);
        gameObject.SetActive(false);
        GameManager.Instance.StartLevel();
    }

}
