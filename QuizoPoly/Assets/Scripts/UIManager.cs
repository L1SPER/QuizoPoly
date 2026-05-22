using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ============= IN GAME PANEL =============
    [Header("IN GAME PANEL (Arkaplan - soru sırasında gizlenecek)")]
    public GameObject inGamePanel;

    // ============= INFO PANEL =============
    [Header("INFO PANEL")]
    public GameObject infoPanel;
    public TMP_Text infoNameText;
    public Image infoColorImage;
    public TMP_Text infoPriceText;

    // ============= PURCHASE PANEL =============
    [Header("PURCHASE PANEL")]
    public GameObject purchasePanel;
    public Image purchaseColorImage;
    public TMP_Text purchaseNameText;
    public TMP_Text purchasePriceText;
    public Button purchaseBuyButton;
    public Button purchasePassButton;

    // ============= BUILDING PANEL =============
    [Header("BUILDING PANEL")]
    public GameObject buildingPanel;
    public Image buildingColorImage;
    public TMP_Text buildingCategoryText;
    public Button floor1Button;
    public Button floor2Button;
    public Button floor3Button;
    public Button floor4Button;
    public Button hotelButton;
    public Button buildingPassButton;
    public TMP_Text floor1PriceText;
    public TMP_Text floor2PriceText;
    public TMP_Text floor3PriceText;
    public TMP_Text floor4PriceText;
    public TMP_Text hotelPriceText;

    // ============= QUESTION PANEL =============
    [Header("QUESTION PANEL")]
    public GameObject questionPanel;
    public TMP_Text questionText;
    public Button buttonA;
    public Button buttonB;
    public Button buttonC;
    public Button buttonD;
    public TMP_Text textA;
    public TMP_Text textB;
    public TMP_Text textC;
    public TMP_Text textD;
    public TMP_Text questionTimerText;
    public TMP_Text questionCategoryText;
    public TMP_Text questionDifficultyText;

    [Header("QUESTION FEEDBACK RENGİ")]
    public Color correctColor = new Color(0.2f, 0.8f, 0.2f);  // Yeşil
    public Color wrongColor = new Color(0.9f, 0.2f, 0.2f);    // Kırmızı
    public float feedbackDuration = 1.5f;  // Renk gösterim süresi

    // ============= RENT PANEL =============
    [Header("RENT PANEL")]
    public GameObject rentPanel;
    public TMP_Text rentNameText;
    public Image rentColorImage;
    public TMP_Text rentBuyPriceText;
    public TMP_Text rentRentPriceText;
    public Button rentBuyButton;
    public Button payRentButton;

    // ============= EXIT JAIL PANEL =============
    [Header("EXIT JAIL PANEL")]
    public GameObject exitJailPanel;
    public Image exitJailColorImage;
    public TMP_Text exitJailNameText;
    public TMP_Text exitJailPriceText;
    public Button rollDicesButton;
    public Button exitJailButton;
    public Button waitButton;

    // ===== CALLBACKS =====
    private Action onBuyClicked;
    private Action onPurchasePassClicked;
    private Action<int> onBuildLevelChosen;
    private Action onBuildingPassClicked;
    private Action<bool> onQuestionAnswered;
    private int correctAnswerIndex;
    private Action onRentBuyClicked;
    private Action onPayRentClicked;
    private Action onRollDicesClicked;
    private Action onPayExitClicked;
    private Action onWaitClicked;

    // Süre sayacı ve feedback için
    private Coroutine questionTimerCoroutine;
    private bool questionAnswered = false;

    // Original buton renkleri (feedback sonrası geri yüklemek için)
    private Color originalButtonColorA;
    private Color originalButtonColorB;
    private Color originalButtonColorC;
    private Color originalButtonColorD;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (infoPanel != null) infoPanel.SetActive(false);
        if (purchasePanel != null) purchasePanel.SetActive(false);
        if (buildingPanel != null) buildingPanel.SetActive(false);
        if (questionPanel != null) questionPanel.SetActive(false);
        if (rentPanel != null) rentPanel.SetActive(false);
        if (exitJailPanel != null) exitJailPanel.SetActive(false);
    }

    void Start()
    {
        // Orijinal buton renklerini kaydet (feedback sonrası geri yüklemek için)
        if (buttonA != null) originalButtonColorA = GetButtonColor(buttonA);
        if (buttonB != null) originalButtonColorB = GetButtonColor(buttonB);
        if (buttonC != null) originalButtonColorC = GetButtonColor(buttonC);
        if (buttonD != null) originalButtonColorD = GetButtonColor(buttonD);

        SetupPurchaseButtons();
        SetupBuildingButtons();
        SetupQuestionButtons();
        SetupRentButtons();
        SetupExitJailButtons();
    }

    Color GetButtonColor(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null) return img.color;
        return Color.white;
    }

    void SetButtonColor(Button btn, Color color)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    // ============= INFO PANEL =============

    public void ShowInfoPanel(Tile tile)
    {
        if (tile == null) return;

        infoPanel.SetActive(true);

        infoNameText.text = tile.tileName;
        infoColorImage.color = tile.groupColor;

        if (tile.basePrice > 0)
        {
            infoPriceText.gameObject.SetActive(true);
            infoPriceText.text = FormatMoney(tile.basePrice);
        }
        else
        {
            infoPriceText.gameObject.SetActive(false);
        }
    }

    public void HideInfoPanel()
    {
        infoPanel.SetActive(false);
    }

    // ============= PURCHASE =============

    void SetupPurchaseButtons()
    {
        purchaseBuyButton.onClick.RemoveAllListeners();
        purchaseBuyButton.onClick.AddListener(() => {
            purchasePanel.SetActive(false);
            onBuyClicked?.Invoke();
        });

        purchasePassButton.onClick.RemoveAllListeners();
        purchasePassButton.onClick.AddListener(() => {
            purchasePanel.SetActive(false);
            onPurchasePassClicked?.Invoke();
        });
    }

    public void ShowPurchasePanel(Tile tile, Action buyCallback, Action passCallback)
    {
        purchasePanel.SetActive(true);

        purchaseColorImage.color = tile.groupColor;
        purchaseNameText.text = tile.tileName;
        purchasePriceText.text = FormatMoney(tile.basePrice);

        onBuyClicked = buyCallback;
        onPurchasePassClicked = passCallback;
    }

    // ============= BUILDING =============

    void SetupBuildingButtons()
    {
        floor1Button.onClick.RemoveAllListeners();
        floor1Button.onClick.AddListener(() => SelectBuildLevel(1));

        floor2Button.onClick.RemoveAllListeners();
        floor2Button.onClick.AddListener(() => SelectBuildLevel(2));

        floor3Button.onClick.RemoveAllListeners();
        floor3Button.onClick.AddListener(() => SelectBuildLevel(3));

        floor4Button.onClick.RemoveAllListeners();
        floor4Button.onClick.AddListener(() => SelectBuildLevel(4));

        hotelButton.onClick.RemoveAllListeners();
        hotelButton.onClick.AddListener(() => SelectBuildLevel(5));

        buildingPassButton.onClick.RemoveAllListeners();
        buildingPassButton.onClick.AddListener(() => {
            buildingPanel.SetActive(false);
            onBuildingPassClicked?.Invoke();
        });
    }

    void SelectBuildLevel(int level)
    {
        buildingPanel.SetActive(false);
        onBuildLevelChosen?.Invoke(level);
    }

    public void ShowBuildingPanel(Tile tile, int playerMoney, Action<int> buildCallback, Action passCallback)
    {
        buildingPanel.SetActive(true);

        buildingColorImage.color = tile.groupColor;
        buildingCategoryText.text = tile.category.ToString().ToUpper();

        onBuildLevelChosen = buildCallback;
        onBuildingPassClicked = passCallback;

        int costPerLevel = tile.basePrice / 2;
        int currentLevel = tile.buildingLevel;

        bool isSequential = false;
        if (GameManager.Instance != null && GameManager.Instance.gameSettings != null)
        {
            isSequential = GameManager.Instance.gameSettings.buildingLevelSelection == 1;
        }

        int level1Total = costPerLevel;
        int level2Total = costPerLevel * 2;
        int level3Total = costPerLevel * 3;
        int level4Total = costPerLevel * 4;
        int hotelTotal = costPerLevel * 6;

        int currentLevelTotal = GetTotalCost(currentLevel, costPerLevel);

        SetupBuildLevelButton(floor1Button, floor1PriceText, 1, level1Total - currentLevelTotal, currentLevel, playerMoney, isSequential);
        SetupBuildLevelButton(floor2Button, floor2PriceText, 2, level2Total - currentLevelTotal, currentLevel, playerMoney, isSequential);
        SetupBuildLevelButton(floor3Button, floor3PriceText, 3, level3Total - currentLevelTotal, currentLevel, playerMoney, isSequential);
        SetupBuildLevelButton(floor4Button, floor4PriceText, 4, level4Total - currentLevelTotal, currentLevel, playerMoney, isSequential);
        SetupBuildLevelButton(hotelButton, hotelPriceText, 5, hotelTotal - currentLevelTotal, currentLevel, playerMoney, isSequential);
    }

    void SetupBuildLevelButton(Button btn, TMP_Text priceText, int level, int cost, int currentLevel, int playerMoney, bool isSequential)
    {
        bool isAboveCurrent = level > currentLevel;
        bool isNextLevel = level == currentLevel + 1;
        bool hasMoney = playerMoney >= cost;

        if (isSequential)
        {
            btn.interactable = isNextLevel && hasMoney;
        }
        else
        {
            btn.interactable = isAboveCurrent && hasMoney;
        }

        if (priceText != null)
            priceText.text = FormatMoney(cost);
    }

    int GetTotalCost(int level, int costPerLevel)
    {
        switch (level)
        {
            case 0: return 0;
            case 1: return costPerLevel;
            case 2: return costPerLevel * 2;
            case 3: return costPerLevel * 3;
            case 4: return costPerLevel * 4;
            case 5: return costPerLevel * 6;
            default: return 0;
        }
    }

    // ============= QUESTION =============

    void SetupQuestionButtons()
    {
        buttonA.onClick.RemoveAllListeners();
        buttonA.onClick.AddListener(() => OnAnswerClicked(0));

        buttonB.onClick.RemoveAllListeners();
        buttonB.onClick.AddListener(() => OnAnswerClicked(1));

        buttonC.onClick.RemoveAllListeners();
        buttonC.onClick.AddListener(() => OnAnswerClicked(2));

        buttonD.onClick.RemoveAllListeners();
        buttonD.onClick.AddListener(() => OnAnswerClicked(3));
    }

    Button GetButtonByIndex(int index)
    {
        switch (index)
        {
            case 0: return buttonA;
            case 1: return buttonB;
            case 2: return buttonC;
            case 3: return buttonD;
            default: return null;
        }
    }

    void OnAnswerClicked(int answerIndex)
    {
        if (questionAnswered) return;
        questionAnswered = true;

        if (questionTimerCoroutine != null)
        {
            StopCoroutine(questionTimerCoroutine);
            questionTimerCoroutine = null;
        }

        bool correct = (answerIndex == correctAnswerIndex);

        // Feedback başlat: butonları renklendir, kısa bekle, sonra kapat
        StartCoroutine(ShowAnswerFeedback(answerIndex, correct));
    }

    IEnumerator ShowAnswerFeedback(int clickedIndex, bool correct)
    {
        // Tüm butonları pasif yap (oyuncu tekrar basamasın)
        buttonA.interactable = false;
        buttonB.interactable = false;
        buttonC.interactable = false;
        buttonD.interactable = false;

        // Tıklanan butonun rengini ayarla
        Button clickedButton = GetButtonByIndex(clickedIndex);
        if (clickedButton != null)
        {
            SetButtonColor(clickedButton, correct ? correctColor : wrongColor);
        }

        // Eğer yanlış cevap verildiyse, doğru cevabı da yeşil göster
        if (!correct)
        {
            Button correctButton = GetButtonByIndex(correctAnswerIndex);
            if (correctButton != null)
            {
                SetButtonColor(correctButton, correctColor);
            }
        }

        // Feedback süresince bekle
        yield return new WaitForSeconds(feedbackDuration);

        // Butonları orijinal renge döndür ve aktif yap
        SetButtonColor(buttonA, originalButtonColorA);
        SetButtonColor(buttonB, originalButtonColorB);
        SetButtonColor(buttonC, originalButtonColorC);
        SetButtonColor(buttonD, originalButtonColorD);

        buttonA.interactable = true;
        buttonB.interactable = true;
        buttonC.interactable = true;
        buttonD.interactable = true;

        // Soru paneli kapanır, InGamePanel geri açılır
        questionPanel.SetActive(false);
        if (inGamePanel != null) inGamePanel.SetActive(true);

        onQuestionAnswered?.Invoke(correct);
    }

    public void ShowQuestionPanel(Category category, int difficulty, Action<bool> answerCallback)
    {
        if (inGamePanel != null) inGamePanel.SetActive(false);

        questionPanel.SetActive(true);
        questionAnswered = false;

        // Butonları başlangıç durumuna döndür (önceki sorunun renkleri kalmasın)
        SetButtonColor(buttonA, originalButtonColorA);
        SetButtonColor(buttonB, originalButtonColorB);
        SetButtonColor(buttonC, originalButtonColorC);
        SetButtonColor(buttonD, originalButtonColorD);
        buttonA.interactable = true;
        buttonB.interactable = true;
        buttonC.interactable = true;
        buttonD.interactable = true;

        Question q = null;
        if (QuestionManager.Instance != null)
        {
            q = QuestionManager.Instance.GetRandomQuestion(category, difficulty);
        }

        if (q == null)
        {
            questionText.text = $"[{category} - {GetDifficultyName(difficulty)}] Test sorusu. Dogru cevap: A";
            textA.text = "A) Dogru cevap";
            textB.text = "B) Yanlis 1";
            textC.text = "C) Yanlis 2";
            textD.text = "D) Yanlis 3";
            correctAnswerIndex = 0;
        }
        else
        {
            questionText.text = q.text;
            textA.text = $"A) {q.choices[0]}";
            textB.text = $"B) {q.choices[1]}";
            textC.text = $"C) {q.choices[2]}";
            textD.text = $"D) {q.choices[3]}";
            correctAnswerIndex = q.correctAnswer;
        }

        if (questionCategoryText != null)
            questionCategoryText.text = category.ToString().ToUpper();

        if (questionDifficultyText != null)
            questionDifficultyText.text = GetDifficultyName(difficulty).ToUpper();

        onQuestionAnswered = answerCallback;

        int duration = GetDurationForDifficulty(difficulty);
        if (questionTimerCoroutine != null)
            StopCoroutine(questionTimerCoroutine);
        questionTimerCoroutine = StartCoroutine(QuestionTimerCoroutine(duration));
    }

    int GetDurationForDifficulty(int difficulty)
    {
        if (GameManager.Instance == null || GameManager.Instance.gameSettings == null)
            return 30;

        var settings = GameManager.Instance.gameSettings;
        switch (difficulty)
        {
            case 1: return settings.beginnerQuestionTime;
            case 2: return settings.easyQuestionTime;
            case 3: return settings.mediumQuestionTime;
            case 4: return settings.hardQuestionTime;
            case 5: return settings.impossibleQuestionTime;
            default: return 60;
        }
    }

    IEnumerator QuestionTimerCoroutine(int totalSeconds)
    {
        float remaining = totalSeconds;

        while (remaining > 0 && !questionAnswered)
        {
            if (questionTimerText != null)
                questionTimerText.text = Mathf.CeilToInt(remaining).ToString();

            remaining -= Time.deltaTime;
            yield return null;
        }

        if (!questionAnswered)
        {
            Debug.Log("Sure doldu! Yanlis sayiliyor.");
            questionAnswered = true;

            // Süre dolduğunda doğru cevabı yeşil göster, sonra kapat
            StartCoroutine(ShowTimeoutFeedback());
        }
    }

    IEnumerator ShowTimeoutFeedback()
    {
        // Butonları pasif yap
        buttonA.interactable = false;
        buttonB.interactable = false;
        buttonC.interactable = false;
        buttonD.interactable = false;

        // Doğru cevabı yeşil göster
        Button correctButton = GetButtonByIndex(correctAnswerIndex);
        if (correctButton != null)
        {
            SetButtonColor(correctButton, correctColor);
        }

        yield return new WaitForSeconds(feedbackDuration);

        // Butonları orijinal renge döndür
        SetButtonColor(buttonA, originalButtonColorA);
        SetButtonColor(buttonB, originalButtonColorB);
        SetButtonColor(buttonC, originalButtonColorC);
        SetButtonColor(buttonD, originalButtonColorD);
        buttonA.interactable = true;
        buttonB.interactable = true;
        buttonC.interactable = true;
        buttonD.interactable = true;

        questionPanel.SetActive(false);
        if (inGamePanel != null) inGamePanel.SetActive(true);

        onQuestionAnswered?.Invoke(false);
    }

    string GetDifficultyName(int difficulty)
    {
        switch (difficulty)
        {
            case 1: return "Beginner";
            case 2: return "Easy";
            case 3: return "Medium";
            case 4: return "Hard";
            case 5: return "Impossible";
            default: return "Unknown";
        }
    }

    // ============= RENT =============

    void SetupRentButtons()
    {
        rentBuyButton.onClick.RemoveAllListeners();
        rentBuyButton.onClick.AddListener(() => {
            rentPanel.SetActive(false);
            onRentBuyClicked?.Invoke();
        });

        payRentButton.onClick.RemoveAllListeners();
        payRentButton.onClick.AddListener(() => {
            rentPanel.SetActive(false);
            onPayRentClicked?.Invoke();
        });
    }

    public void ShowRentPanel(Tile tile, int buyPrice, int rentPrice,
        Action buyCallback, Action payRentCallback)
    {
        if (tile == null) return;

        rentPanel.SetActive(true);

        rentNameText.text = tile.tileName;
        rentColorImage.color = tile.groupColor;
        rentBuyPriceText.text = FormatMoney(buyPrice);
        rentRentPriceText.text = FormatMoney(rentPrice);

        onRentBuyClicked = buyCallback;
        onPayRentClicked = payRentCallback;
    }

    // ============= EXIT JAIL =============

    void SetupExitJailButtons()
    {
        rollDicesButton.onClick.RemoveAllListeners();
        rollDicesButton.onClick.AddListener(() => {
            exitJailPanel.SetActive(false);
            onRollDicesClicked?.Invoke();
        });

        exitJailButton.onClick.RemoveAllListeners();
        exitJailButton.onClick.AddListener(() => {
            exitJailPanel.SetActive(false);
            onPayExitClicked?.Invoke();
        });

        waitButton.onClick.RemoveAllListeners();
        waitButton.onClick.AddListener(() => {
            exitJailPanel.SetActive(false);
            onWaitClicked?.Invoke();
        });
    }

    public void ShowExitJailPanel(Tile jailTile, int exitFee, int turnsLeft,
        Action rollDicesCallback, Action payExitCallback, Action waitCallback)
    {
        exitJailPanel.SetActive(true);

        exitJailColorImage.color = jailTile.groupColor;
        exitJailNameText.text = jailTile.tileName;
        exitJailPriceText.text = FormatMoney(exitFee);

        waitButton.interactable = turnsLeft > 0;

        onRollDicesClicked = rollDicesCallback;
        onPayExitClicked = payExitCallback;
        onWaitClicked = waitCallback;
    }

    // ============= HELPER =============

    string FormatMoney(int amount)
    {
        return $"{amount.ToString("N0", new System.Globalization.CultureInfo("tr-TR"))} TL";
    }
}