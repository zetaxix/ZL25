using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MenuButtonConfig : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText; // Butonun text öðesi
    [SerializeField] private Color selectedColor = new Color(0.98f, 0.85f, 0.56f); // #FAD88F rengini Color formatýnda tanýmlýyoruz
    [SerializeField] private Vector3 pressedScale = new Vector3(0.4f, 0.4f, 1f); // Týklanýnca küçülme oraný
    [SerializeField] private float pressDuration = 0.5f; // Küçülme süresi
    [SerializeField] private float releaseDuration = 0.2f; // Geri dönme süresi

    private Vector3 originalScale; // Butonun orijinal boyutu
    private Color originalColor; // Text'in orijinal rengi
    private Button button; // Buton bileþeni

    private void Awake()
    {
        // Butonun baþlangýç boyutunu ve text rengini kaydet
        originalScale = transform.localScale;
        if (buttonText != null)
        {
            originalColor = buttonText.color;
        }

        // Buton bileþenini al
        button = GetComponent<Button>();
        if (button != null)
        {
            // Butona týklama olayýný ekle
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        // Týklama animasyonunu baþlat
        StartCoroutine(AnimateButton());
    }

    private IEnumerator AnimateButton()
    {
        // TextMeshPro rengini deðiþtir
        if (buttonText != null)
        {
            buttonText.color = selectedColor;
        }

        // Animasyonla küçülme
        float elapsedTime = 0f;
        while (elapsedTime < pressDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, pressedScale, elapsedTime / pressDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = pressedScale;

        // Küçük boyutta kýsa bir duraklama
        yield return new WaitForSeconds(0.1f);

        // Animasyonla eski boyuta geri dönme
        elapsedTime = 0f;
        while (elapsedTime < releaseDuration)
        {
            transform.localScale = Vector3.Lerp(pressedScale, originalScale, elapsedTime / releaseDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;

        // Rengi eski haline getir
        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }
    }
}
