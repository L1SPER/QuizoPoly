using System;

public enum CardType
{
    Bonus,
    Chance
}

public enum CardEffect
{
    AddMoney,              // Para ver: +X
    SubtractMoney,         // Para al: -X
    CollectFromAllPlayers, // Tüm oyunculardan X al
    GoToJail,              // Hapise git
    GoToStart,             // Başlangıca git + bonus
    MoveForward,           // X kare ileri (etkileşimli)
    MoveBackward,          // X kare geri (etkileşimli)
    GoToNearestVacation    // En yakın tatil bölgesine git
}

[Serializable]
public class ChanceBonusCard
{
    public string title;       // "Bayram harçlığı aldın!"
    public CardEffect effect;  // Hangi etki
    public int amount;         // Para miktarı veya kare sayısı

    public ChanceBonusCard(string title, CardEffect effect, int amount = 0)
    {
        this.title = title;
        this.effect = effect;
        this.amount = amount;
    }
}