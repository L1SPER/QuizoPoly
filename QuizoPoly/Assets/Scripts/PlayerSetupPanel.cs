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

    public PlayerSetupInfo[] GetPlayerInfos()
    {
        PlayerSetupInfo[] infos = new PlayerSetupInfo[playerCards.Length];

        for (int i = 0; i < playerCards.Length; i++)
        {
            infos[i] = new PlayerSetupInfo
            {
                playerName = playerCards[i].GetPlayerName(),
                playerColor = playerCards[i].GetSelectedColor(),
                teamId = 0  // varsayılan, 2v2'de düzeltilecek
            };
        }

        // Eğer 2v2 modu ise takımları ata
        if (GameSetupData.IsTeamMode() && infos.Length == 4)
        {
            infos[0].teamId = 1;
            infos[1].teamId = 1;
            infos[2].teamId = 2;
            infos[3].teamId = 2;
        }

        return infos;
    }
}