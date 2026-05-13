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

    void Start()
    {
        // Sahne açılınca tüm ayarlar default'a dönsün
        ResetAllToDefaults();
    }

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
        // Oyun sahnesine geçiş yap
        SceneManager.LoadScene("Game"); 
    }
}