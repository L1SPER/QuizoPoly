using UnityEngine;
using DG.Tweening;

public class PanelManager : MonoBehaviour
{
    [Header("Tüm Paneller (sıra önemli)")]
    public RectTransform[] panels;

    [Header("Animasyon Ayarları")]
    public float transitionDuration = 0.5f;
    public Ease easeType = Ease.InOutCubic;

    private int currentPanelIndex = 0;
    private int previousPanelIndex = 0;  // ← YENİ EKLENEN
    private float screenWidth;

    void Start()
    {
        screenWidth = ((RectTransform)transform).rect.width;

        for (int i = 0; i < panels.Length; i++)
        {
            if (i == 0)
                panels[i].anchoredPosition = Vector2.zero;
            else
                panels[i].anchoredPosition = new Vector2(screenWidth, 0);
        }
    }

    public void GoToPanel(int targetIndex)
    {
        if (targetIndex == currentPanelIndex) return;
        if (targetIndex < 0 || targetIndex >= panels.Length) return;

        // Önceki paneli kaydet (geri dönüş için)
        previousPanelIndex = currentPanelIndex;

        RectTransform currentPanel = panels[currentPanelIndex];
        RectTransform targetPanel = panels[targetIndex];

        bool goingForward = targetIndex > currentPanelIndex;

        if (goingForward)
        {
            currentPanel.DOAnchorPos(new Vector2(-screenWidth, 0), transitionDuration).SetEase(easeType);
            targetPanel.anchoredPosition = new Vector2(screenWidth, 0);
            targetPanel.DOAnchorPos(Vector2.zero, transitionDuration).SetEase(easeType);
        }
        else
        {
            currentPanel.DOAnchorPos(new Vector2(screenWidth, 0), transitionDuration).SetEase(easeType);
            targetPanel.anchoredPosition = new Vector2(-screenWidth, 0);
            targetPanel.DOAnchorPos(Vector2.zero, transitionDuration).SetEase(easeType);
        }

        currentPanelIndex = targetIndex;
    }

    // YENİ METOD: Önceki panele dön
    public void GoToPreviousPanel()
    {
        GoToPanel(previousPanelIndex);
    }
}