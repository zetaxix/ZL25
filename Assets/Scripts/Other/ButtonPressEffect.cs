using UnityEngine;
using UnityEngine.UI;

public class ButtonPressEffect : MonoBehaviour
{
    private Vector3 originalScale; // Butonun orijinal boyutu
    public float pressedScale = 0.9f; // Týklama sýrasýnda ölçek (0.9 = %90 küçülme)
    public float animationDuration = 0.1f; // Animasyon süresi

    private void Awake()
    {
        // Orijinal boyutu sakla
        originalScale = transform.localScale;
    }

    public void OnButtonPress()
    {
        // Ýçine çökme animasyonu
        StopAllCoroutines(); // Önceki animasyonlarý iptal et
        StartCoroutine(AnimateButtonPress());
    }

    private System.Collections.IEnumerator AnimateButtonPress()
    {
        // Küçült
        yield return AnimateScale(originalScale, originalScale * pressedScale, animationDuration / 2);

        // Eski boyuta geri döndür
        yield return AnimateScale(transform.localScale, originalScale, animationDuration / 2);
    }

    private System.Collections.IEnumerator AnimateScale(Vector3 fromScale, Vector3 toScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Animasyonu Lerp ile gerçekleþtir
            transform.localScale = Vector3.Lerp(fromScale, toScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = toScale; // Hedef boyut
    }
}
