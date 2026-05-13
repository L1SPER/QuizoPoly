using UnityEngine;
using System.Collections.Generic;

public class BoardGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public int tilesPerSide = 10;      // Her kenarda 10 kare (köşeler dahil)
    public float tileSize = 1f;        // Kare boyutu
    public Transform tilesParent;       // Tile'ların altına gireceği obje

    private List<Tile> allTiles = new List<Tile>();

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        int totalTiles = (tilesPerSide - 1) * 4;  // 36 kenar + 4 köşe = 40
        float halfBoard = (tilesPerSide - 1) * tileSize / 2f;

        int tileIndex = 0;

        // Alt kenar (soldan sağa)
        for (int i = 0; i < tilesPerSide - 1; i++)
        {
            Vector3 pos = new Vector3(-halfBoard + i * tileSize, 0, -halfBoard);
            SpawnTile(pos, tileIndex++);
        }

        // Sağ kenar (aşağıdan yukarı)
        for (int i = 0; i < tilesPerSide - 1; i++)
        {
            Vector3 pos = new Vector3(halfBoard, 0, -halfBoard + i * tileSize);
            SpawnTile(pos, tileIndex++);
        }

        // Üst kenar (sağdan sola)
        for (int i = 0; i < tilesPerSide - 1; i++)
        {
            Vector3 pos = new Vector3(halfBoard - i * tileSize, 0, halfBoard);
            SpawnTile(pos, tileIndex++);
        }

        // Sol kenar (yukarıdan aşağı)
        for (int i = 0; i < tilesPerSide - 1; i++)
        {
            Vector3 pos = new Vector3(-halfBoard, 0, halfBoard - i * tileSize);
            SpawnTile(pos, tileIndex++);
        }
    }

    void SpawnTile(Vector3 position, int index)
    {
        GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, tilesParent);
        tileObj.name = $"Tile_{index:D2}";

        Tile tile = tileObj.GetComponent<Tile>();
        if (tile != null)
        {
            // İlerideki adımda burada TileData atayacağız
        }

        allTiles.Add(tile);
    }
}