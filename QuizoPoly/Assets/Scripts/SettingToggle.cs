using UnityEngine;
using UnityEngine.UI;

public class SettingToggle : MonoBehaviour
{
    [Header("UI Referansı")]
    public Toggle toggle;

    [Header("Varsayılan Değer")]
    public bool defaultValue = true;

    void Start()
    {
        toggle.isOn = defaultValue;
    }

    public void ResetToDefault()
    {
        toggle.isOn = defaultValue;
    }

    public bool GetValue()
    {
        return toggle.isOn;
    }
}