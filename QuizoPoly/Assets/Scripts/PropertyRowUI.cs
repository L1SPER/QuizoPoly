using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PropertyRowUI : MonoBehaviour
{
    [Header("UI Referansları")]
    public TMP_Text rowText;       // "Konya - 1 Katlı (45.000 TL)"
    public Toggle sellToggle;       // [✓]
    public Image colorIndicator;    // Sol kenarda renk (opsiyonel)

    [HideInInspector] public Tile tile;
    [HideInInspector] public int sellValue;

    private Action<Tile, bool> onToggleChanged;

    public void Setup(Tile tileRef, int value, bool isSelected, Action<Tile, bool> toggleCallback)
    {
        tile = tileRef;
        sellValue = value;
        onToggleChanged = toggleCallback;

        // Bina durumu yazısı
        string statusName = GetBuildingStatusName(tile.buildingLevel);
        string moneyStr = value.ToString("N0", new System.Globalization.CultureInfo("tr-TR"));
        rowText.text = $"{tile.tileName} - {statusName} ({moneyStr} TL)";

        // Renk göstergesi (opsiyonel)
        if (colorIndicator != null)
            colorIndicator.color = tile.groupColor;

        // Toggle ayarı
        if (sellToggle != null)
        {
            sellToggle.onValueChanged.RemoveAllListeners();
            sellToggle.isOn = isSelected;
            sellToggle.onValueChanged.AddListener((value) => {
                onToggleChanged?.Invoke(tile, value);
            });
        }
    }

    string GetBuildingStatusName(int buildingLevel)
    {
        switch (buildingLevel)
        {
            case 0: return "Boş Arsa";
            case 1: return "1 Katlı";
            case 2: return "2 Katlı";
            case 3: return "3 Katlı";
            case 4: return "4 Katlı";
            case 5: return "Otel";
            default: return "Bilinmiyor";
        }
    }
}