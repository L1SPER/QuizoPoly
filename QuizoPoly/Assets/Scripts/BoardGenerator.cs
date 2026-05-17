using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class BoardGenerator : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject tilePrefab;

    [Header("Tahta Verisi")]
    public BoardLayout boardLayout;

    [Header("Tahta Ayarları")]
    public int tilesPerSide = 11;
    public Transform tilesParent;

    [Header("Otomatik Hesaplanır")]
    [SerializeField] private float tileSize;  // Tile prefab'ından otomatik okunur

    private List<Tile> allTiles = new List<Tile>();

    void Start()
    {
        // Tile prefab'ının boyutunu otomatik al (X eksenindeki scale)
        if (tilePrefab != null)
        {
            tileSize = tilePrefab.transform.localScale.x;
        }

        GenerateBoard();
    }

    void GenerateBoard()
    {
        if (boardLayout == null || boardLayout.tiles.Length < 40)
        {
            Debug.LogError("BoardLayout atanmamış veya 40 kare yok!");
            return;
        }

        float halfBoard = (tilesPerSide - 1) * tileSize / 2f;
        int tileIndex = 0;

        // SOL KENAR (aşağıdan yukarı)
        for (int i = 0; i < tilesPerSide - 1; i++)
        {
            Vector3 pos = new Vector3(-halfBoard, 0, -halfBoard + i * tileSize);
            SpawnTile(pos, tileIndex++);
        }

        // ÜST KENAR (soldan sağa)
        for (int i = 0; i < tilesPerSide - 1; i++)
        {
            Vector3 pos = new Vector3(-halfBoard + i * tileSize, 0, halfBoard);
            SpawnTile(pos, tileIndex++);
        }

        // SAĞ KENAR (yukarıdan aşağı)
        for (int i = 0; i < tilesPerSide - 1; i++)
        {
            Vector3 pos = new Vector3(halfBoard, 0, halfBoard - i * tileSize);
            SpawnTile(pos, tileIndex++);
        }

        // ALT KENAR (sağdan sola)
        for (int i = 0; i < tilesPerSide - 1; i++)
        {
            Vector3 pos = new Vector3(halfBoard - i * tileSize, 0, -halfBoard);
            SpawnTile(pos, tileIndex++);
        }

        Debug.Log($"Tahta oluşturuldu — Toplam kare: {allTiles.Count}, Tile boyutu: {tileSize}");
    }

    void SpawnTile(Vector3 position, int index)
    {
        GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, tilesParent);

        TileInfo info = boardLayout.tiles[index];
        tileObj.name = $"Tile_{index:D2}_{info.tileName}";

        Tile tile = tileObj.AddComponent<Tile>();
        if (tile != null)
        {
            tile.tileName = info.tileName;
            tile.tileType = info.tileType;
            tile.category = info.category;
            tile.basePrice = info.basePrice;
            tile.groupColor = info.groupColor;
        }
      

        Renderer renderer = tileObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = info.groupColor;
        }

        // Tile üzerindeki yazıyı güncelle ve döndür
        TextMeshProUGUI label = tileObj.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = info.tileName;

            // Hangi kenarda olduğuna göre yazıyı döndür
            float yRotation = GetLabelRotation(index);
            label.transform.parent.rotation = Quaternion.Euler(90, yRotation, -90);

        }

        allTiles.Add(tile);
    }

    // Index'e göre yazının Y dönüşünü belirler
    float GetLabelRotation(int index)
    {
        // Kenarlar
        if (index >= 0 && index <= 9) return 0f;     // Sol kenar
        if (index >= 10 && index <= 19) return 90f;  // Üst kenar
        if (index >= 20 && index <= 29) return 180f;  // Sağ kenar
        if (index >= 30 && index <= 39) return 270f;    // Alt kenar

        return 0f;
    }
    public Tile GetTile(int index)
    {
        if (index < 0 || index >= allTiles.Count)
            return null;
        return allTiles[index];
    }

    public int GetTileCount()
    {
        return allTiles.Count;
    }

    public Vector3 GetTileWorldPosition(int index)
    {
        Tile tile = GetTile(index);
        if (tile == null) return Vector3.zero;
        return tile.transform.position;
    }
}