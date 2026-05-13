using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerSetupPanel : MonoBehaviour
{
    [Header("Bu panelin kartları")]
    public PlayerCard[] playerCards;

    [Header("Next Butonu")]
    public Button nextButton;

    void OnEnable()
    {
        // Panel her açıldığında event'leri bağla
        foreach (PlayerCard card in playerCards)
        {
            if (card != null)
            {
                card.OnReadyStateChanged -= CheckAllReady; // Önce çıkar (çift bağlanmasın)
                card.OnReadyStateChanged += CheckAllReady;
            }
        }

        // Başlangıçta Next butonu pasif
        nextButton.interactable = false;
    }

    void OnDisable()
    {
        // Panel kapanınca event'leri çöz (memory leak ve hata önler)
        foreach (PlayerCard card in playerCards)
        {
            if (card != null)
            {
                card.OnReadyStateChanged -= CheckAllReady;
            }
        }
    }

    void CheckAllReady()
    {
        foreach (PlayerCard card in playerCards)
        {
            if (card == null || !card.IsReady)
            {
                nextButton.interactable = false;
                return;
            }
        }

        nextButton.interactable = true;
    }
}