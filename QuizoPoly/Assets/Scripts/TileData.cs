using UnityEngine;

public enum TileType
{
    Property,      // Normal arazi
    Vacation,      // Tatil bölgesi
    Start,         // Başlangıç
    Jail,          // Cezaevi
    Chance,        // Şans
    Tax,           // Vergi
    Bonus,         // Bonus
    GoToStart      // Başlangıca dön
}

public enum Category
{
    None,
    Tarih,
    Cografya,
    Sanat,
    Spor,
    Bilim,
    Muzik,
    Edebiyat,
    GenelKultur
}

[CreateAssetMenu(fileName = "NewTile", menuName = "Quizopoly/Tile Data")]
public class TileData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string tileName;
    public int tileIndex;          // 0-39 arası tahtadaki sırası
    public TileType tileType;

    [Header("Arazi Bilgileri (Property ve Vacation için)")]
    public Category category;
    public int basePrice;          // Arazinin temel fiyatı
    public Color groupColor;       // Renk grubu rengi

    [Header("Görsel")]
    public Sprite icon;            // Opsiyonel: kare üstüne yerleştirilecek görsel
}