using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileData data;
    public int ownerId = -1;
    public int buildingLevel = 0;

    public void OnPlayerLanded(Player player)
    {
        Debug.Log($"{player.playerName} → {data.tileName} karesine geldi");

        switch (data.tileType)
        {
            case TileType.Property:
            case TileType.Vacation:
                HandlePropertyTile(player);
                break;

            case TileType.Start:
                Debug.Log("Başlangıç karesi");
                break;

            case TileType.Jail:
                player.GoToJail();
                break;

            case TileType.Chance:
                Debug.Log("Şans kartı çekilecek");
                break;

            case TileType.Tax:
                Debug.Log("Vergi ödenecek");
                break;

            case TileType.Bonus:
                Debug.Log("Bonus soru sorulacak");
                break;

            case TileType.GoToStart:
                Debug.Log("Oyuncu başa dönüyor");
                player.currentTileIndex = 0;
                break;
        }
    }

    private void HandlePropertyTile(Player player)
    {
        if (ownerId == -1)
        {
            Debug.Log($"{data.tileName} satın almak için soru sorulacak. Fiyat: {data.basePrice}");
        }
        else if (ownerId == player.id)
        {
            Debug.Log($"{data.tileName} kendi arazin, bina ekleyebilirsin");
        }
        else
        {
            Debug.Log($"{data.tileName} rakip arazisi, kira ödenecek");
        }
    }
}