using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI damageText;

    public void SetText(string text)
    {
        damageText.text = text;
        StartCoroutine(DestroyAfterTweens());
    }

    private IEnumerator DestroyAfterTweens()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 50, 1.3f).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(0.3f);
        damageText.DOFade(0, 1);
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
