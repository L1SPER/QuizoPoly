using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Tile Bilgileri")]
    public string tileName;
    public TileType tileType;
    public Category category;
    public int basePrice;
    public Color groupColor;

    [Header("Oyun Durumu")]
    public int ownerId = -1;        // -1 = sahipsiz
    public int buildingLevel = 0;   // 0 = boş, 1-5 binalar

    public void OnPlayerLanded(Player player)
    {
        Debug.Log($"{player.playerName} → {tileName} karesine geldi");
    }
}