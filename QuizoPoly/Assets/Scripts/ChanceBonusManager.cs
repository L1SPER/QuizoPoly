using UnityEngine;
using System.Collections.Generic;

public class ChanceBonusManager : MonoBehaviour
{
    public static ChanceBonusManager Instance { get; private set; }

    private List<ChanceBonusCard> bonusCards;
    private List<ChanceBonusCard> chanceCards;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        InitializeCards();
    }

    void InitializeCards()
    {
        // ============= BONUS KARTLARI (Hep Pozitif) =============
        bonusCards = new List<ChanceBonusCard>
        {
            new ChanceBonusCard("Bayram harçlığı aldın!", CardEffect.AddMoney, 100000),
            new ChanceBonusCard("Vergi iadesi geldi", CardEffect.AddMoney, 50000),
            new ChanceBonusCard("Banka faizi ödüyor", CardEffect.AddMoney, 75000),
            new ChanceBonusCard("Loto tutturdu!", CardEffect.AddMoney, 250000),
            new ChanceBonusCard("Bayram parası", CardEffect.CollectFromAllPlayers, 25000),
            new ChanceBonusCard("Maaş günü", CardEffect.AddMoney, 150000),
            new ChanceBonusCard("Hediye geldi", CardEffect.AddMoney, 50000),
            new ChanceBonusCard("Doğum günün!", CardEffect.CollectFromAllPlayers, 40000),
        };

        // ============= ŞANS KARTLARI (Karışık) =============
        chanceCards = new List<ChanceBonusCard>
        {
            // Pozitif
            new ChanceBonusCard("Promosyon kazandın", CardEffect.AddMoney, 100000),
            new ChanceBonusCard("Mirastan pay aldın", CardEffect.AddMoney, 200000),
            new ChanceBonusCard("Eski borç ödendi", CardEffect.AddMoney, 75000),
            new ChanceBonusCard("İkramiye kazandın", CardEffect.AddMoney, 125000),

            // Negatif
            new ChanceBonusCard("Trafik cezası kestin", CardEffect.SubtractMoney, 50000),
            new ChanceBonusCard("Sağlık masrafı", CardEffect.SubtractMoney, 100000),
            new ChanceBonusCard("Doğrudan hapse!", CardEffect.GoToJail, 0),
            new ChanceBonusCard("Araç tamiri", CardEffect.SubtractMoney, 75000),

            // Hareket
            new ChanceBonusCard("Başlangıca dön (200K al)", CardEffect.GoToStart, 0),
            new ChanceBonusCard("3 kare ileri git", CardEffect.MoveForward, 3),
            new ChanceBonusCard("3 kare geri git", CardEffect.MoveBackward, 3),
            new ChanceBonusCard("En yakın tatil bölgesine git", CardEffect.GoToNearestVacation, 0),
        };

        Debug.Log($"[CHANCE/BONUS] {bonusCards.Count} bonus karti, {chanceCards.Count} sans karti yuklendi");
    }

    public ChanceBonusCard DrawBonusCard()
    {
        if (bonusCards == null || bonusCards.Count == 0)
            return null;
        return bonusCards[Random.Range(0, bonusCards.Count)];
    }

    public ChanceBonusCard DrawChanceCard()
    {
        if (chanceCards == null || chanceCards.Count == 0)
            return null;
        return chanceCards[Random.Range(0, chanceCards.Count)];
    }
}