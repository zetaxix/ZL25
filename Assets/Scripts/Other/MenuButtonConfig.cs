using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MenuButtonConfig : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText; // Butonun text ��esi
    [SerializeField] private Color selectedColor = new Color(0.98f, 0.85f, 0.56f); // #FAD88F rengini Color format�nda tan�ml�yoruz
    [SerializeField] private Vector3 pressedScale = new Vector3(0.4f, 0.4f, 1f); // T�klan�nca k���lme oran�
    [SerializeField] private float pressDuration = 0.5f; // K���lme s�resi
    [SerializeField] private float releaseDuration = 0.2f; // Geri d�nme s�resi

    private Vector3 originalScale; // Butonun orijinal boyutu
    private Color originalColor; // Text'in orijinal rengi
    private Button button; // Buton bile�eni

    private void Awake()
    {
        // Butonun ba�lang�� boyutunu ve text rengini kaydet
        originalScale = transform.localScale;
        if (buttonText != null)
        {
            originalColor = buttonText.color;
        }

        // Buton bile�enini al
        button = GetComponent<Button>();
        if (button != null)
        {
            // Butona t�klama olay�n� ekle
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        // T�klama animasyonunu ba�lat
        StartCoroutine(AnimateButton());
    }

    private IEnumerator AnimateButton()
    {
        // TextMeshPro rengini de�i�tir
        if (buttonText != null)
        {
            buttonText.color = selectedColor;
        }

        // Animasyonla k���lme
        float elapsedTime = 0f;
        while (elapsedTime < pressDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, pressedScale, elapsedTime / pressDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = pressedScale;

        // K���k boyutta k�sa bir duraklama
        yield return new WaitForSeconds(0.1f);

        // Animasyonla eski boyuta geri d�nme
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
