using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingSlider : MonoBehaviour
{
    public enum DisplayFormat
    {
        Number,
        Percentage,
        Time,
        Multiplier
    }

    [Header("UI Referansları")]
    public Slider slider;
    public TMP_Text valueText;

    [Header("Format")]
    public DisplayFormat format = DisplayFormat.Number;

    [Header("Artış Miktarı (Step Size)")]
    public float stepSize = 1f;

    [Header("Varsayılan Değer")]
    public float defaultValue = 0f;  // ← YENİ EKLENEN

    void Start()
    {
        SnapToStep(slider.value);
        slider.onValueChanged.AddListener(SnapToStep);
    }

    void SnapToStep(float value)
    {
        float snappedValue = Mathf.Round(value / stepSize) * stepSize;
        snappedValue = Mathf.Clamp(snappedValue, slider.minValue, slider.maxValue);

        if (!Mathf.Approximately(slider.value, snappedValue))
        {
            slider.SetValueWithoutNotify(snappedValue);
        }

        UpdateValueText(snappedValue);
    }

    void UpdateValueText(float value)
    {
        switch (format)
        {
            case DisplayFormat.Number:
                valueText.text = Mathf.RoundToInt(value).ToString("N0", new System.Globalization.CultureInfo("tr-TR"));
                break;
            case DisplayFormat.Percentage:
                valueText.text = $"{Mathf.RoundToInt(value)}%";
                break;
            case DisplayFormat.Time:
                valueText.text = $"{Mathf.RoundToInt(value)}s";
                break;
            case DisplayFormat.Multiplier:
                valueText.text = $"{value:F1}x";
                break;
        }
    }

    // YENİ EKLENEN METOD
    public void ResetToDefault()
    {
        slider.value = defaultValue;
        SnapToStep(defaultValue);
    }
}