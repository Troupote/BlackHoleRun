using BHR;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartAnimationUI : MonoBehaviour
{
    public float startScale, scale3, scale2, scale1,scale0;
    public float textScale3,textScale2,textScale1,textScale0;
    public Color color3, color2, color1,color0;

    [SerializeField] private Image _blackHole;
    [SerializeField] private TextMeshProUGUI _counter;

    IEnumerator StartAnimationCoroutine()
    {
        // Init
        _counter.gameObject.SetActive(false);
        _blackHole.gameObject.SetActive(false);
        _blackHole.transform.localScale = Vector3.one * startScale;
        _counter.color = color3;
        _counter.text = "3";
        _counter.transform.localScale = Vector3.one * textScale3;

        _blackHole.transform.DOScale(scale3, 0.4f);
        yield return new WaitForSeconds(0.4f);

        _blackHole.gameObject.SetActive(true);
        _counter.gameObject.SetActive(true);
        _counter.DOColor(color2, 1f);
        _counter.transform.DOScale(textScale2,1f);
        _blackHole.transform.DOScale(scale2, 1f);
        yield return new WaitForSeconds(1f);
        // 2->1
        _counter.text = "2";
        _counter.DOColor(color1, 1f);
        _counter.transform.DOScale(textScale1, 1f);
        _blackHole.transform.DOScale(scale1, 1f);
        yield return new WaitForSeconds(1f);
        // 1-> Go
        _counter.text = "1";
        _counter.DOColor(color0, 1f);
        _blackHole.transform.DOScale(scale0, 1f);
        _counter.transform.DOScale(textScale0, 1f);
        yield return new WaitForSeconds(1f);

        // GO
        // Enable play
        _counter.text = "GO";
        _counter.DOColor(Color.black, 0.3f);
        _blackHole.transform.DOScale(0, 0.3f);
        _counter.transform.DOScale(textScale3, 0.3f);

        yield return new WaitForSeconds(0.3f);
        _blackHole.gameObject.SetActive(false);
        _counter.gameObject.SetActive(false);
        gameObject.SetActive(false);
        GameManager.Instance.StartLevel();
    }

    public void Awake()
    {
        _blackHole.gameObject.SetActive(false);
        _counter.gameObject.SetActive(false);
    }

    public void StartAnimation()
    {
        gameObject.SetActive(true);
        StartCoroutine(StartAnimationCoroutine());
    }
}
