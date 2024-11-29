using UnityEngine;
using UnityEngine.UI;

public class ButtonPressEffect : MonoBehaviour
{
    private Vector3 originalScale; // Butonun orijinal boyutu
    public float pressedScale = 0.9f; // T�klama s�ras�nda �l�ek (0.9 = %90 k���lme)
    public float animationDuration = 0.1f; // Animasyon s�resi

    private void Awake()
    {
        // Orijinal boyutu sakla
        originalScale = transform.localScale;
    }

    public void OnButtonPress()
    {
        // ��ine ��kme animasyonu
        StopAllCoroutines(); // �nceki animasyonlar� iptal et
        StartCoroutine(AnimateButtonPress());
    }

    private System.Collections.IEnumerator AnimateButtonPress()
    {
        // K���lt
        yield return AnimateScale(originalScale, originalScale * pressedScale, animationDuration / 2);

        // Eski boyuta geri d�nd�r
        yield return AnimateScale(transform.localScale, originalScale, animationDuration / 2);
    }

    private System.Collections.IEnumerator AnimateScale(Vector3 fromScale, Vector3 toScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Animasyonu Lerp ile ger�ekle�tir
            transform.localScale = Vector3.Lerp(fromScale, toScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = toScale; // Hedef boyut
    }
}
