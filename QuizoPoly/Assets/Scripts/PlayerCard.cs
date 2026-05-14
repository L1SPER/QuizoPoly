using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerCard : MonoBehaviour
{
    [Header("UI Referansları")]
    public Image nameImage;              // Üst renk şeridi (Player 1 yazısı arkası)
    public TMP_Text nameText;            // "Player 1" yazısı
    public TMP_InputField nameInput;     // İsim giriş kutusu

    [Header("Renk Butonları")]
    public Button[] colorButtons;
    public Color[] colors;

    [Header("Ready Butonu")]
    public Button readyButton;
    public Image readyButtonImage;

    [Header("Varsayılan")]
    public string defaultName = "Player 1";

    public event Action OnReadyStateChanged;
    public bool IsReady { get; private set; } = false;

    // Renkler
    private Color defaultNameImageColor;
    private Color defaultReadyButtonColor;
    private Color readyGreenColor = new Color(0.3f, 0.8f, 0.3f, 1f);

    private int selectedColorIndex = -1;
    private Color selectedColor;

    void Start()
    {
        // Başlangıç renklerini kaydet (geri dönüş için)
        defaultNameImageColor = nameImage.color;
        defaultReadyButtonColor = readyButtonImage.color;

        // Renk butonlarını otomatik bağla
        for (int i = 0; i < colorButtons.Length; i++)
        {
            int index = i;
            colorButtons[i].onClick.AddListener(() => OnColorSelected(index));

            Outline outline = colorButtons[i].GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
        }

        // Ready butonunu bağla
        readyButton.onClick.AddListener(OnReadyClicked);
    }

    public void OnColorSelected(int index)
    {
        // Hazırsa renk değiştirmeyi engelle
        if (IsReady) return;

        selectedColorIndex = index;
        selectedColor = colors[index];

        // Tüm outline'ları kapat, seçileni aç
        for (int i = 0; i < colorButtons.Length; i++)
        {
            Outline outline = colorButtons[i].GetComponent<Outline>();
            if (outline != null) outline.enabled = (i == index);
        }
    }

    public void OnReadyClicked()
    {
        if (!IsReady)
        {
            // READY MODUNA GEÇ
            string finalName = string.IsNullOrWhiteSpace(nameInput.text) ? defaultName : nameInput.text;
            nameText.text = finalName;

            if (selectedColorIndex >= 0)
            {
                nameImage.color = selectedColor;
            }

            readyButtonImage.color = readyGreenColor;
            IsReady = true;

            Debug.Log($"{finalName} hazır!");
        }
        else
        {
            // İPTAL ET - varsayılana dön
            nameText.text = defaultName;
            nameImage.color = defaultNameImageColor;
            readyButtonImage.color = defaultReadyButtonColor;
            IsReady = false;

            Debug.Log($"{defaultName} iptal etti");
        }

        OnReadyStateChanged?.Invoke();
    }

    public string GetPlayerName()
    {
        return string.IsNullOrWhiteSpace(nameInput.text) ? defaultName : nameInput.text;
    }

    public Color GetSelectedColor()
    {
        return selectedColorIndex >= 0 ? selectedColor : Color.white;
    }
}