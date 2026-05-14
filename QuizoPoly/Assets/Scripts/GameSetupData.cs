using UnityEngine;

public enum GameMode
{
    OneVsOne,    // 1v1 - 2 oyuncu
    OneVsThree,  // 1v3 - 4 oyuncu
    TwoVsTwo     // 2v2 - takım modu
}

[System.Serializable]
public class PlayerSetupInfo
{
    public string playerName;
    public Color playerColor;
    public int teamId;  // 0 = takımsız, 1 = Takım 1, 2 = Takım 2
}

public static class GameSetupData
{
    public static GameMode CurrentMode;
    public static PlayerSetupInfo[] Players;

    public static int GetPlayerCount()
    {
        return Players != null ? Players.Length : 0;
    }

    public static bool IsTeamMode()
    {
        return CurrentMode == GameMode.TwoVsTwo;
    }
}