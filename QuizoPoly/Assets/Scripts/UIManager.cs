using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("IN GAME PANEL")]
    public GameObject inGamePanel;

    [Header("INFO PANEL")]
    public GameObject infoPanel;
    public TMP_Text infoNameText;
    public Image infoColorImage;
    public TMP_Text infoPriceText;

    [Header("PURCHASE PANEL")]
    public GameObject purchasePanel;
    public Image purchaseColorImage;
    public TMP_Text purchaseNameText;
    public TMP_Text purchasePriceText;
    public Button purchaseBuyButton;
    public Button purchasePassButton;

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
    public Color correctColor = new Color(0.2f, 0.8f, 0.2f);
    public Color wrongColor = new Color(0.9f, 0.2f, 0.2f);
    public float feedbackDuration = 1.5f;

    [Header("RENT PANEL")]
    public GameObject rentPanel;
    public TMP_Text rentNameText;
    public Image rentColorImage;
    public TMP_Text rentBuyPriceText;
    public TMP_Text rentRentPriceText;
    public Button rentBuyButton;
    public Button payRentButton;

    [Header("EXIT JAIL PANEL")]
    public GameObject exitJailPanel;
    public Image exitJailColorImage;
    public TMP_Text exitJailNameText;
    public TMP_Text exitJailPriceText;
    public Button rollDicesButton;
    public Button exitJailButton;
    public Button waitButton;

    [Header("GENERIC INFO PANEL")]
    public GameObject chanceBonusPanel;
    public TMP_Text chanceBonusTitleText;
    public TMP_Text chanceBonusDescriptionText;
    public Image chanceBonusBackgroundImage;
    public Button chanceBonusOkButton;

    [Header("Panel Renkleri")]
    public Color bonusColor = new Color(0.2f, 0.7f, 0.3f);
    public Color chanceColor = new Color(0.9f, 0.7f, 0.1f);
    public Color taxColor = new Color(0.8f, 0.2f, 0.2f);
    public Color goToStartColor = new Color(0.3f, 0.5f, 0.9f);
    public Color jailColor = new Color(0.4f, 0.4f, 0.4f);

    [Header("BANKRUPTCY PANEL")]
    public GameObject bankruptcyPanel;
    public TMP_Text bankruptcyTitleText;
    public TMP_Text bankruptcyDebtText;
    public TMP_Text bankruptcySelectedTotalText;
    public Button bankruptcySellButton;
    public Transform bankruptcyContentParent;  // ScrollView'in Content'i
    public GameObject propertyRowPrefab;       // PropertyRow prefab

    [Header("WIN/LOSE PANELS")]
    public GameObject winPanel;
    public TMP_Text winnerNameText;
    public Button winPanelOkButton;

    public GameObject losePanel;
    public TMP_Text loserNameText;
    public Button losePanelOkButton;

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
    private Action onChanceBonusOk;
    private Action<List<Tile>> onBankruptcySell;
    private Action onBankruptcyCancel;

    private Coroutine questionTimerCoroutine;
    private bool questionAnswered = false;

    private Color originalButtonColorA;
    private Color originalButtonColorB;
    private Color originalButtonColorC;
    private Color originalButtonColorD;

    // Bankruptcy state
    private List<Tile> bankruptcySelectedTiles = new List<Tile>();
    private List<PropertyRowUI> bankruptcyRows = new List<PropertyRowUI>();
    private int bankruptcyDebt = 0;
    private int bankruptcyDebtorMoney = 0;   

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
        if (chanceBonusPanel != null) chanceBonusPanel.SetActive(false);
        if (bankruptcyPanel != null) bankruptcyPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    void Start()
    {
        if (buttonA != null) originalButtonColorA = GetButtonColor(buttonA);
        if (buttonB != null) originalButtonColorB = GetButtonColor(buttonB);
        if (buttonC != null) originalButtonColorC = GetButtonColor(buttonC);
        if (buttonD != null) originalButtonColorD = GetButtonColor(buttonD);

        SetupPurchaseButtons();
        SetupBuildingButtons();
        SetupQuestionButtons();
        SetupRentButtons();
        SetupExitJailButtons();
        SetupChanceBonusButton();
        SetupBankruptcyButton();
        SetupWinLoseButtons();
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

    public void ShowPurchasePanel(Tile tile, int playerMoney, Action buyCallback, Action passCallback)
    {
        purchasePanel.SetActive(true);

        purchaseColorImage.color = tile.groupColor;
        purchaseNameText.text = tile.tileName;
        purchasePriceText.text = FormatMoney(tile.basePrice);

        // Buy butonu - parası yetiyorsa aktif
        purchaseBuyButton.interactable = playerMoney >= tile.basePrice;

        Debug.Log($"[PURCHASE PANEL] Para: {playerMoney}, Fiyat: {tile.basePrice}, BuyAktif: {purchaseBuyButton.interactable}");

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

        if (isSequential) btn.interactable = isNextLevel && hasMoney;
        else btn.interactable = isAboveCurrent && hasMoney;

        if (priceText != null) priceText.text = FormatMoney(cost);
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
        StartCoroutine(ShowAnswerFeedback(answerIndex, correct));
    }

    IEnumerator ShowAnswerFeedback(int clickedIndex, bool correct)
    {
        buttonA.interactable = false;
        buttonB.interactable = false;
        buttonC.interactable = false;
        buttonD.interactable = false;

        Button clickedButton = GetButtonByIndex(clickedIndex);
        if (clickedButton != null)
            SetButtonColor(clickedButton, correct ? correctColor : wrongColor);

        if (!correct)
        {
            Button correctButton = GetButtonByIndex(correctAnswerIndex);
            if (correctButton != null) SetButtonColor(correctButton, correctColor);
        }

        yield return new WaitForSeconds(feedbackDuration);

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

        onQuestionAnswered?.Invoke(correct);
    }

    public void ShowQuestionPanel(Category category, int difficulty, Action<bool> answerCallback)
    {
        if (inGamePanel != null) inGamePanel.SetActive(false);

        questionPanel.SetActive(true);
        questionAnswered = false;

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
            q = QuestionManager.Instance.GetRandomQuestion(category, difficulty);

        if (q == null)
        {
            questionText.text = $"[{category} - {GetDifficultyName(difficulty)}] Test sorusu";
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
        if (questionTimerCoroutine != null) StopCoroutine(questionTimerCoroutine);
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
            questionAnswered = true;
            StartCoroutine(ShowTimeoutFeedback());
        }
    }

    IEnumerator ShowTimeoutFeedback()
    {
        buttonA.interactable = false;
        buttonB.interactable = false;
        buttonC.interactable = false;
        buttonD.interactable = false;

        Button correctButton = GetButtonByIndex(correctAnswerIndex);
        if (correctButton != null) SetButtonColor(correctButton, correctColor);

        yield return new WaitForSeconds(feedbackDuration);

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

    public void ShowRentPanel(Tile tile, int buyPrice, int rentPrice, int playerMoney,
        Action buyCallback, Action payRentCallback)
    {
        if (tile == null) return;

        rentPanel.SetActive(true);

        rentNameText.text = tile.tileName;
        rentColorImage.color = tile.groupColor;
        rentBuyPriceText.text = FormatMoney(buyPrice);
        rentRentPriceText.text = FormatMoney(rentPrice);

        rentBuyButton.interactable = playerMoney >= buyPrice;
        payRentButton.interactable = playerMoney >= rentPrice;

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

    public void ShowExitJailPanel(Tile jailTile, int exitFee, int turnsLeft, int playerMoney,
        Action rollDicesCallback, Action payExitCallback, Action waitCallback)
    {
        exitJailPanel.SetActive(true);

        exitJailColorImage.color = jailTile.groupColor;
        exitJailNameText.text = jailTile.tileName;
        exitJailPriceText.text = FormatMoney(exitFee);

        waitButton.interactable = turnsLeft > 0;
        exitJailButton.interactable = playerMoney >= exitFee;

        onRollDicesClicked = rollDicesCallback;
        onPayExitClicked = payExitCallback;
        onWaitClicked = waitCallback;
    }

    // ============= GENERIC INFO PANEL =============

    void SetupChanceBonusButton()
    {
        if (chanceBonusOkButton != null)
        {
            chanceBonusOkButton.onClick.RemoveAllListeners();
            chanceBonusOkButton.onClick.AddListener(() => {
                chanceBonusPanel.SetActive(false);
                onChanceBonusOk?.Invoke();
            });
        }
    }

    public void ShowGenericInfoPanel(string title, string description, Color bgColor, Action okCallback)
    {
        chanceBonusPanel.SetActive(true);
        chanceBonusTitleText.text = title;
        chanceBonusDescriptionText.text = description;
        if (chanceBonusBackgroundImage != null)
            chanceBonusBackgroundImage.color = bgColor;
        onChanceBonusOk = okCallback;
    }

    public void ShowChanceBonusPanel(CardType type, ChanceBonusCard card, Action okCallback)
    {
        string title = (type == CardType.Bonus) ? "BONUS" : "SANS";
        Color color = (type == CardType.Bonus) ? bonusColor : chanceColor;
        string description = GetCardDescription(card);
        ShowGenericInfoPanel(title, description, color, okCallback);
    }

    public void ShowTaxPanel(int taxRate, int taxAmount, Action okCallback)
    {
        string description =
            $"Vergi karesine dustun!\n\n" +
            $"Paranin %{taxRate}'i alindi\n\n" +
            $"-{FormatMoney(taxAmount)}";
        ShowGenericInfoPanel("VERGI", description, taxColor, okCallback);
    }

    public void ShowGoToStartPanel(Action okCallback)
    {
        string description = "Baslangica Don karesine dustun!\n\nDirekt baslangic karesine\nisinlaniyorsun\n\n(Para alinmaz)";
        ShowGenericInfoPanel("BASLANGICA DON", description, goToStartColor, okCallback);
    }

    public void ShowGoToJailInfoPanel(Action okCallback)
    {
        string description = "Hapis karesine dustun!\n\nDogrudan Silivri Hapishanesi'ne\ngidiyorsun";
        ShowGenericInfoPanel("HAPSE GIDIYORSUN", description, jailColor, okCallback);
    }

    string GetCardDescription(ChanceBonusCard card)
    {
        switch (card.effect)
        {
            case CardEffect.AddMoney:
                return $"{card.title}\n\n+{FormatMoney(card.amount)}";
            case CardEffect.SubtractMoney:
                return $"{card.title}\n\n-{FormatMoney(card.amount)}";
            case CardEffect.CollectFromAllPlayers:
                return $"{card.title}\n\nHer oyuncudan {FormatMoney(card.amount)} alirsin";
            case CardEffect.GoToJail:
                return $"{card.title}\n\nDogrudan hapse gidersin!";
            case CardEffect.GoToStart:
                return $"{card.title}\n\nBaslangic karesine git";
            case CardEffect.MoveForward:
                return $"{card.title}\n\n{card.amount} kare ileri git";
            case CardEffect.MoveBackward:
                return $"{card.title}\n\n{card.amount} kare geri git";
            case CardEffect.GoToNearestVacation:
                return $"{card.title}\n\nEn yakin tatil bolgesine isinlanirsin";
            default:
                return card.title;
        }
    }

    // ============= BANKRUPTCY PANEL =============

    void SetupBankruptcyButton()
    {
        if (bankruptcySellButton != null)
        {
            bankruptcySellButton.onClick.RemoveAllListeners();
            bankruptcySellButton.onClick.AddListener(OnBankruptcySellClicked);
        }
    }

    public void ShowBankruptcyPanel(PlayerToken token, int debt, List<Tile> ownedProperties,
        Action<List<Tile>> sellCallback)
    {
        bankruptcyPanel.SetActive(true);
        bankruptcyDebt = debt;
        bankruptcyDebtorMoney = token.money;   
        onBankruptcySell = sellCallback;

        // Title
        if (bankruptcyTitleText != null)
            bankruptcyTitleText.text = $"BORCUNUZ VAR - {token.playerName}";

        // Debt
        if (bankruptcyDebtText != null)
            bankruptcyDebtText.text = $"Borc: {FormatMoney(debt)}";

        // Önceki rowları temizle
        ClearBankruptcyRows();
        bankruptcySelectedTiles.Clear();

        // Yeni rowları oluştur
        foreach (var tile in ownedProperties)
        {
            GameObject rowObj = Instantiate(propertyRowPrefab, bankruptcyContentParent);
            PropertyRowUI rowUI = rowObj.GetComponent<PropertyRowUI>();

            if (rowUI != null)
            {
                int sellValue = CalculateTileSellValue(tile);
                rowUI.Setup(tile, sellValue, false, OnBankruptcyToggleChanged);
                bankruptcyRows.Add(rowUI);
            }
        }

        UpdateBankruptcySelectedTotal();
    }

    public void HideBankruptcyPanel()
    {
        bankruptcyPanel.SetActive(false);
        ClearBankruptcyRows();
    }

    void ClearBankruptcyRows()
    {
        foreach (var row in bankruptcyRows)
        {
            if (row != null && row.gameObject != null)
                Destroy(row.gameObject);
        }
        bankruptcyRows.Clear();
    }

    int CalculateTileSellValue(Tile tile)
    {
        int landValue = tile.basePrice / 2;  // Arazi yarı fiyat

        // Bina maliyeti
        int costPerLevel = tile.basePrice / 2;
        int currentBuildingCost = GetTotalBuildingCost(tile.buildingLevel, costPerLevel);
        int buildingRefund = currentBuildingCost / 2;  // Binalar yarı fiyat

        return landValue + buildingRefund;
    }

    int GetTotalBuildingCost(int level, int costPerLevel)
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

    void OnBankruptcyToggleChanged(Tile tile, bool isSelected)
    {
        if (isSelected)
        {
            if (!bankruptcySelectedTiles.Contains(tile))
                bankruptcySelectedTiles.Add(tile);
        }
        else
        {
            bankruptcySelectedTiles.Remove(tile);
        }

        UpdateBankruptcySelectedTotal();
    }

    void UpdateBankruptcySelectedTotal()
    {
        int total = 0;
        foreach (var tile in bankruptcySelectedTiles)
        {
            total += CalculateTileSellValue(tile);
        }

        int afterSale = bankruptcyDebtorMoney + total - bankruptcyDebt;

        if (bankruptcySelectedTotalText != null)
        {
            if (afterSale >= 0)
                bankruptcySelectedTotalText.text =
                    $"Secili Toplam: {FormatMoney(total)}  |  Satis sonrasi: {FormatMoney(afterSale)}";
            else
                bankruptcySelectedTotalText.text =
                    $"Secili Toplam: {FormatMoney(total)}  |  Eksik: {FormatMoney(-afterSale)}";
        }

        // Cash + satış toplamı borcu karşılıyorsa aktif
        if (bankruptcySellButton != null)
            bankruptcySellButton.interactable = (bankruptcyDebtorMoney + total) >= bankruptcyDebt;
    }

    void OnBankruptcySellClicked()
    {
        // Seçili tile'ları geri çağrıya gönder
        List<Tile> toSell = new List<Tile>(bankruptcySelectedTiles);
        bankruptcyPanel.SetActive(false);
        ClearBankruptcyRows();
        onBankruptcySell?.Invoke(toSell);
    }

    // ============= WIN/LOSE =============

    void SetupWinLoseButtons()
    {
        if (winPanelOkButton != null)
        {
            winPanelOkButton.onClick.RemoveAllListeners();
            winPanelOkButton.onClick.AddListener(() => {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
            });
        }

        if (losePanelOkButton != null)
        {
            losePanelOkButton.onClick.RemoveAllListeners();
            losePanelOkButton.onClick.AddListener(() => {
                losePanel.SetActive(false);
            });
        }
    }

    public void ShowWinPanel(PlayerToken winner)
    {
        if (winPanel == null) return;

        winPanel.SetActive(true);

        if (winnerNameText != null)
            winnerNameText.text = $"{winner.playerName} KAZANDI!";
    }

    public void ShowLosePanel(PlayerToken loser)
{
    if (losePanel == null) return;

    losePanel.SetActive(true);

    if (loserNameText != null)
        loserNameText.text = $"{loser.playerName} IFLAS ETTI!";
}

    // ============= HELPER =============

    string FormatMoney(int amount)
    {
        return $"{amount.ToString("N0", new System.Globalization.CultureInfo("tr-TR"))} TL";
    }
}