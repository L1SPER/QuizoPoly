using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Oyuncu Bilgileri")]
    public int id;                    // 0, 1, 2, 3
    public string playerName;
    public Color playerColor;

    [Header("Oyun Durumu")]
    public int money = 5_000_000;     // Başlangıç parası
    public int currentTileIndex = 0;  // Tahtadaki konum (0 = başlangıç)
    public bool isInJail = false;
    public int jailTurnsLeft = 0;

    [Header("Sahip Olunan Araziler")]
    public System.Collections.Generic.List<Tile> ownedTiles = new System.Collections.Generic.List<Tile>();

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"{playerName} +{amount} para aldı. Yeni bakiye: {money}");
    }

    public void RemoveMoney(int amount)
    {
        money -= amount;
        Debug.Log($"{playerName} -{amount} para verdi. Yeni bakiye: {money}");
    }

    public bool CanAfford(int amount)
    {
        return money >= amount;
    }

    public void GoToJail()
    {
        isInJail = true;
        jailTurnsLeft = 3;
        currentTileIndex = 10;  // Cezaevinin tahta indexi (sonra düzeltiriz)
        Debug.Log($"{playerName} cezaevine gitti!");
    }
}