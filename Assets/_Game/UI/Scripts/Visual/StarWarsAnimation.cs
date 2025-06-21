using Assets.SimpleLocalization.Scripts;
using BHR;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarWarsAnimation : MonoBehaviour
{
    [SerializeField] private Transform _circle;
    [SerializeField] private Transform _logo;
    [SerializeField] private Transform _startLogoPos;
    [SerializeField] private Transform _endLogoPos;
    [SerializeField] private float _logoMoveDuration;
    [SerializeField] private float _logoFadeDuration;

    [SerializeField] private Transform _container;
    [SerializeField] private TextMeshProUGUI _thanks;
    [SerializeField] private TextMeshProUGUI _credits;
    [SerializeField] private Transform _startPos;
    [SerializeField] private Transform _endPos;
    [SerializeField] private float _scrollDuration;

    private void OnEnable()
    {
        StartCoroutine(Credits());
    }

    [Button]
    IEnumerator Credits()
    {
        // Inits
        //_logo.position = _startPos.position;
        _logo.GetComponent<Image>().enabled = false;
        _logo.GetComponent<Image>().color = Color.white;
        _container.position = _startPos.position;
        _circle.gameObject.SetActive(true);
        _circle.transform.localScale = Vector3.zero;
        _credits.text = LocalizationManager.Localize("M/C/Credits");
        _thanks.text = LocalizationManager.Localize("M/C/Thanks");

        // Sequence
        yield return new WaitForSeconds(0.5f);

        _circle.DOScale(25f, 1.5f);
        yield return new WaitForSeconds(2f);

        _circle.gameObject.SetActive(false);
        _logo.GetComponent<Image>().enabled = true;
        MoveLogo();
        yield return new WaitForSeconds(_logoMoveDuration - 1f);

        FadeLogo();
        yield return new WaitForSeconds(_logoFadeDuration);

        Scroll();
        yield return new WaitForSeconds(_scrollDuration);

        yield return new WaitForSeconds(2f);
        ModuleManager.Instance.OnBack();
    }

    private void MoveLogo()
    {
        _logo.DOMove(_endLogoPos.position, _logoMoveDuration);
    }

    private void FadeLogo()
    {
        _logo.GetComponent<Image>().DOColor(new Color(1f,1f,1f,0f), _logoFadeDuration);
    }

    private void Scroll()
    {
        DOTween.Kill(_container);
        _container.DOMove(_endPos.position, _scrollDuration).SetEase(Ease.Linear);
    }
}
