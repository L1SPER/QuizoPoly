using UnityEngine;

public class GameTester : MonoBehaviour
{
    public GameSettings settings;

    void Start()
    {
        Debug.Log("=== OYUN BAŞLADI ===");
        Debug.Log($"Mod: {GameSetupData.CurrentMode}");
        Debug.Log($"Oyuncu sayısı: {GameSetupData.GetPlayerCount()}");

        if (GameSetupData.Players != null)
        {
            for (int i = 0; i < GameSetupData.Players.Length; i++)
            {
                var p = GameSetupData.Players[i];
                Debug.Log($"Oyuncu {i + 1}: {p.playerName}, Renk: {p.playerColor}, Takım: {p.teamId}");
            }
        }

        Debug.Log("=== AYARLAR ===");
        Debug.Log($"Starting Money: {settings.startingMoney}");
        Debug.Log($"Tax Rate: {settings.taxRate}%");
        Debug.Log($"Vacation Victory: {settings.vacationVictoryCount}");
    }
}