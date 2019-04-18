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
        //GetComponent<RectTransform>().DOAnchorPosY(100, 1);
        yield return new WaitForSeconds(0.5f);
        damageText.DOFade(0, 1);
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
