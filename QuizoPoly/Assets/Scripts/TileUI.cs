using UnityEngine;

public class TileUI : MonoBehaviour
{
    [Header("Temel Bilgiler")]
    public string tileName;
    public int tileIndex;          // 0-39 arası tahtadaki sırası
    public TileType tileType;

    [Header("Arazi Bilgileri (Property ve Vacation için)")]
    public Category category;
    public int basePrice;          // Arazinin temel fiyatı
    public Color groupColor;       // Renk grubu rengi

}
