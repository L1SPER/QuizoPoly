using UnityEngine;

public class PlayerToken : MonoBehaviour
{
    public int playerId;
    public string playerName;
    public Color playerColor;
    public int teamId;
    public int currentTileIndex = 0;
    public int money;

    public void Initialize(int id, PlayerSetupInfo info, int startMoney)
    {
        playerId = id;
        playerName = info.playerName;
        playerColor = info.playerColor;
        teamId = info.teamId;
        money = startMoney;
        currentTileIndex = 0;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = playerColor;
        }
    }
}