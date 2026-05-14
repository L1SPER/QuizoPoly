using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsPanel : MonoBehaviour
{
    [Header("Tüm Setting Slider'ları")]
    public SettingSlider[] allSliders;

    [Header("Tüm Setting Toggle'ları")]
    public SettingToggle[] allToggles;

    [Header("Tüm Setting Dropdown'ları")]
    public SettingDropdown[] allDropdowns;

    [Header("PlayerInfo Panelleri (oyuncu bilgilerini almak için)")]
    public PlayerSetupPanel oneVsOnePanel;
    public PlayerSetupPanel oneVsThreePanel;
    public PlayerSetupPanel twoVsTwoPanel;

    [Header("Oyun Ayarları (referans)")]
    public GameSettings gameSettings;

    public void ResetAllToDefaults()
    {
        foreach (SettingSlider setting in allSliders)
        {
            if (setting != null)
                setting.ResetToDefault();
        }

        foreach (SettingToggle toggle in allToggles)
        {
            if (toggle != null)
                toggle.ResetToDefault();
        }

        foreach (SettingDropdown dropdown in allDropdowns)
        {
            if (dropdown != null)
                dropdown.ResetToDefault();
        }

        Debug.Log("Tüm ayarlar varsayılan değerlere döndürüldü");
    }

    public void StartGame()
    {
        // Aktif modun panelinden oyuncu bilgilerini al
        PlayerSetupInfo[] players = null;

        switch (GameSetupData.CurrentMode)
        {
            case GameMode.OneVsOne:
                players = oneVsOnePanel.GetPlayerInfos();
                break;
            case GameMode.OneVsThree:
                players = oneVsThreePanel.GetPlayerInfos();
                break;
            case GameMode.TwoVsTwo:
                players = twoVsTwoPanel.GetPlayerInfos();
                break;
        }

        GameSetupData.Players = players;

        // Settings değerlerini GameSettings asset'ine yaz
        UpdateGameSettings();

        Debug.Log($"Oyun başlıyor — Mod: {GameSetupData.CurrentMode}, Oyuncu sayısı: {players.Length}");

        SceneManager.LoadScene("Game");
    }

    void UpdateGameSettings()
    {
        gameSettings.startingMoney = (int)allSliders[0].slider.value;
        gameSettings.passStartBonus = (int)allSliders[1].slider.value;
        gameSettings.bonusTileReward = (int)allSliders[2].slider.value;
        gameSettings.taxRate = (int)allSliders[3].slider.value;
        gameSettings.jailExitFee = (int)allSliders[4].slider.value;

        gameSettings.emptyLandRent = (int)allSliders[5].slider.value;
        gameSettings.builtLandRent = (int)allSliders[6].slider.value;
        gameSettings.colorSetMultiplier = allSliders[7].slider.value;
        gameSettings.buyFromOpponentMultiplier = allSliders[8].slider.value;

        gameSettings.beginnerQuestionTime = (int)allSliders[9].slider.value;
        gameSettings.easyQuestionTime = (int)allSliders[10].slider.value;
        gameSettings.mediumQuestionTime = (int)allSliders[11].slider.value;
        gameSettings.hardQuestionTime = (int)allSliders[12].slider.value;
        gameSettings.impossibleQuestionTime = (int)allSliders[13].slider.value;

        gameSettings.jailDuration = (int)allSliders[14].slider.value;
        gameSettings.vacationVictoryCount = (int)allSliders[15].slider.value;

        gameSettings.vacationVictoryEnabled = allToggles[0].GetValue();
        gameSettings.chanceCardsEnabled = allToggles[1].GetValue();

        gameSettings.jailExitDifficulty = allDropdowns[0].GetSelectedIndex();
        gameSettings.maxGameDuration = allDropdowns[1].GetSelectedIndex();
        gameSettings.endGameTiebreaker = allDropdowns[2].GetSelectedIndex();
        gameSettings.buildingLevelSelection = allDropdowns[3].GetSelectedIndex();
    }
}