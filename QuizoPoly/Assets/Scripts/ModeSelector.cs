using UnityEngine;

public class ModeSelector : MonoBehaviour
{
    public void SelectOneVsOne()
    {
        GameSetupData.CurrentMode = GameMode.OneVsOne;
        Debug.Log("Mode: 1v1");
    }

    public void SelectOneVsThree()
    {
        GameSetupData.CurrentMode = GameMode.OneVsThree;
        Debug.Log("Mode: 1v3");
    }

    public void SelectTwoVsTwo()
    {
        GameSetupData.CurrentMode = GameMode.TwoVsTwo;
        Debug.Log("Mode: 2v2");
    }
}