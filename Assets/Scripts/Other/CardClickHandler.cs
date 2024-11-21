using UnityEngine;

public class CardClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (GameScreenOpponentCardManager.Instance != null)
        {
            GameScreenOpponentCardManager.Instance.ToggleCardSelection(gameObject);
        }
    }
}