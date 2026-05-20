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
    public int ownerId = -1;
    public int buildingLevel = 0;

    [Header("Görsel")]
    public GameObject currentBuilding;  // Şu an dikilmiş bina (referans)

    public void OnPlayerLanded(Player player)
    {
        Debug.Log($"{player.playerName} → {tileName} karesine geldi");
    }
}