using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] Button resultButton;

    private void Awake()
    {
        resultButton.onClick.AddListener( () =>
        {
            string whoWin = PlayerPrefs.GetString("WhoWin");

            if (whoWin == "UserWin")
            {
                GameScreenOpponentCardManager.Instance.HideOpponentGridParent();

                GameScreenOpponentCardManager.Instance.SaveUserSelectedCardsToJson();
                GameScreenOpponentCardManager.Instance.SaveAIProtectedCardsToJson();

                GameResultScreenManager.Instance.ShowAICard();
                GameResultScreenManager.Instance.ShowUserStolenCards();

            }else if (whoWin == "OpponentWin")
            {
                Debug.Log("BÝLGÝSAYAR KAZANDI LAAAAN!!!");

                KaliteKazanirManager.Instance.HideUserGridParent();

                KaliteKazanirManager.Instance.SaveProtectedCards();
                KaliteKazanirManager.Instance.SaveAIStoleCardsToJson();

                GameResultScreenManager.Instance.ShowUserProtectedCards();
                GameResultScreenManager.Instance.ShowAIStolenCards();

            }


        } );
    }
}
