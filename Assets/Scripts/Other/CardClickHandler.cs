using UnityEngine;

public class CardClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        string whoWin = PlayerPrefs.GetString("WhoWin");

        if (whoWin == "UserWin")
        {
            if (GameScreenOpponentCardManager.Instance != null)
            {
                GameScreenOpponentCardManager.Instance.ToggleCardSelection(gameObject);
            }
        }
        else if (whoWin == "OpponentWin")
        {
            if (KaliteKazanirManager.Instance != null)
            {
                KaliteKazanirManager.Instance.ToggleCardSelection(gameObject);
            }
        }

       
    }
}