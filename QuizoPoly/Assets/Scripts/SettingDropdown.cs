using UnityEngine;
using TMPro;

public class SettingDropdown : MonoBehaviour
{
    [Header("UI Referansı")]
    public TMP_Dropdown dropdown;

    [Header("Varsayılan Değer (Index)")]
    public int defaultIndex = 0;

    void Start()
    {
        dropdown.value = defaultIndex;
    }

    public void ResetToDefault()
    {
        dropdown.value = defaultIndex;
    }

    public int GetSelectedIndex()
    {
        return dropdown.value;
    }

    public string GetSelectedText()
    {
        return dropdown.options[dropdown.value].text;
    }
}   