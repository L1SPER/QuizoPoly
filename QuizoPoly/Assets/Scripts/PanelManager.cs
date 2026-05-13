using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PanelManager : MonoBehaviour
{
    [Header("Tüm Paneller (sıra önemli)")]
    public RectTransform[] panels;

    [Header("Animasyon Ayarları")]
    public float transitionDuration = 0.5f;
    public Ease easeType = Ease.InOutCubic;

    private int currentPanelIndex = 0;
    private float screenWidth;

    void Start()
    {
        screenWidth = ((RectTransform)transform).rect.width;

        // Başlangıçta sadece ilk panel ekranda, diğerleri sağda bekliyor
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

        RectTransform currentPanel = panels[currentPanelIndex];
        RectTransform targetPanel = panels[targetIndex];

        // İleri mi gidiyoruz, geri mi?
        bool goingForward = targetIndex > currentPanelIndex;

        if (goingForward)
        {
            // Mevcut panel sola gider, hedef panel sağdan gelir
            currentPanel.DOAnchorPos(new Vector2(-screenWidth, 0), transitionDuration).SetEase(easeType);

            targetPanel.anchoredPosition = new Vector2(screenWidth, 0);
            targetPanel.DOAnchorPos(Vector2.zero, transitionDuration).SetEase(easeType);
        }
        else
        {
            // Mevcut panel sağa gider, hedef panel soldan gelir
            currentPanel.DOAnchorPos(new Vector2(screenWidth, 0), transitionDuration).SetEase(easeType);

            targetPanel.anchoredPosition = new Vector2(-screenWidth, 0);
            targetPanel.DOAnchorPos(Vector2.zero, transitionDuration).SetEase(easeType);
        }

        currentPanelIndex = targetIndex;
    }
}