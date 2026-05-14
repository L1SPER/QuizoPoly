using UnityEngine;

[System.Serializable]
public class TileInfo
{
    public string tileName;
    public TileType tileType;
    public Category category;
    public int basePrice;
    public Color groupColor;
}

[CreateAssetMenu(fileName = "BoardLayout", menuName = "Quizopoly/Board Layout")]
public class BoardLayout : ScriptableObject
{
    public TileInfo[] tiles;
}