using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referanslar")]
    public BoardGenerator boardGenerator;
    public DiceManager diceManager;
    public GameSettings gameSettings;
    public GameObject playerTokenPrefab;

    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text instructionText;
    public TMP_Text resultsText;
    public Button rollButton;

    [Header("Pul Yerleşimi")]
    public float tokenHeight = 0.5f;

    [Header("Bina Prefab'ları")]
    public GameObject ownershipMarkerPrefab;
    public GameObject building1Prefab;
    public GameObject building2Prefab;
    public GameObject building3Prefab;
    public GameObject building4Prefab;
    public GameObject hotelPrefab;

    [Header("Bina Yerleştirme")]
    public float buildingHeightOffset = 0.2f;

    [HideInInspector] public List<PlayerToken> playerTokens = new List<PlayerToken>();
    [HideInInspector] public int currentPlayerIndex = 0;

    private bool gameStarted = false;
    private bool gameEnded = false;

    private static readonly Category[] AllPlayableCategories = new Category[]
    {
        Category.Tarih, Category.Cografya, Category.Sanat, Category.Spor,
        Category.Bilim, Category.Muzik, Category.Edebiyat, Category.GenelKultur
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        SpawnPlayerTokens();
        gameStarted = true;
        gameEnded = false;
        currentPlayerIndex = 0;

        titleText.text = "QUIZOPOLY";

        rollButton.gameObject.SetActive(true);
        rollButton.onClick.RemoveAllListeners();
        rollButton.onClick.AddListener(OnRollButtonClicked);

        StartNextTurn();
    }

    void SpawnPlayerTokens()
    {
        playerTokens.Clear();
        Vector3 startPos = boardGenerator.GetTileWorldPosition(0);

        for (int i = 0; i < GameSetupData.Players.Length; i++)
        {
            GameObject tokenObj = Instantiate(playerTokenPrefab);
            PlayerToken token = tokenObj.GetComponent<PlayerToken>();
            if (token == null) token = tokenObj.AddComponent<PlayerToken>();

            token.Initialize(i, GameSetupData.Players[i], gameSettings.startingMoney);

            float offsetX = (i - (GameSetupData.Players.Length - 1) / 2f) * 0.3f;
            tokenObj.transform.position = startPos + new Vector3(offsetX, tokenHeight, 0);
            tokenObj.name = $"Token_{i}_{token.playerName}";

            playerTokens.Add(token);
        }
    }

    void StartNextTurn()
    {
        if (gameEnded) return;

        // İflas etmemiş oyuncuyu bul
        int activeCount = 0;
        foreach (var p in playerTokens)
            if (!p.isBankrupt) activeCount++;

        if (activeCount <= 1)
        {
            CheckWinCondition();
            return;
        }

        // Eğer mevcut oyuncu iflas etmişse sıradakine geç
        int attempts = 0;
        while (playerTokens[currentPlayerIndex].isBankrupt && attempts < playerTokens.Count)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % playerTokens.Count;
            attempts++;
        }

        var current = playerTokens[currentPlayerIndex];

        Debug.Log($"=== TURN === {current.playerName}, isInJail: {current.isInJail}, isBankrupt: {current.isBankrupt}");

        string colorHex = ColorUtility.ToHtmlStringRGB(current.playerColor);
        instructionText.text = $"Turn: <color=#{colorHex}>{current.playerName}</color>";

        UpdateMoneyDisplay();

        if (current.isInJail)
        {
            rollButton.interactable = false;
            StartCoroutine(StartJailTurn(current));
        }
        else
        {
            rollButton.interactable = true;
        }
    }

    IEnumerator StartJailTurn(PlayerToken token)
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(HandleJailTurn(token));
    }

    void UpdateMoneyDisplay()
    {
        string text = "<b>PLAYERS</b>\n\n";

        for (int i = 0; i < playerTokens.Count; i++)
        {
            var p = playerTokens[i];
            string colorHex = ColorUtility.ToHtmlStringRGB(p.playerColor);
            string moneyStr = p.money.ToString("N0", new System.Globalization.CultureInfo("tr-TR"));
            string marker = (i == currentPlayerIndex && !p.isBankrupt) ? "> " : "  ";
            string status = "";
            if (p.isBankrupt) status = " [IFLAS]";
            else if (p.isInJail) status = " [JAIL]";

            text += $"{marker}<color=#{colorHex}>{p.playerName}</color>: {moneyStr} TL{status}\n";
        }

        resultsText.text = text;
    }

    void OnRollButtonClicked()
    {
        if (!gameStarted || gameEnded) return;
        if (diceManager.IsRolling()) return;

        rollButton.interactable = false;
        StartCoroutine(HandleTurn());
    }

    IEnumerator HandleTurn()
    {
        var token = playerTokens[currentPlayerIndex];

        diceManager.RollBothDice();
        while (diceManager.IsRolling()) yield return null;

        int diceTotal = diceManager.GetTotal();
        bool isDouble = diceManager.IsDouble();

        Debug.Log($"{token.playerName} atti: {diceTotal}, cift: {isDouble}");

        if (isDouble)
        {
            token.consecutiveDoubles++;
            if (token.consecutiveDoubles >= 3)
            {
                yield return StartCoroutine(SendToJail(token));
                token.consecutiveDoubles = 0;
                EndTurn();
                yield break;
            }
        }
        else
        {
            token.consecutiveDoubles = 0;
        }

        MovePlayer(token, diceTotal);
        yield return new WaitForSeconds(1f);

        Tile landedTile = boardGenerator.GetTile(token.currentTileIndex);
        Debug.Log($"{token.playerName} -> {landedTile.tileName}");

        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideInfoPanel();

        yield return StartCoroutine(HandleTileLanding(token, landedTile));

        if (token.isBankrupt || gameEnded)
        {
            EndTurn();
            yield break;
        }

        if (token.isInJail)
        {
            EndTurn();
            yield break;
        }

        if (isDouble)
        {
            rollButton.interactable = true;
        }
        else
        {
            EndTurn();
        }
    }

    void MovePlayer(PlayerToken token, int steps)
    {
        int totalTiles = boardGenerator.GetTileCount();
        int oldIndex = token.currentTileIndex;
        int newIndex = (oldIndex + steps) % totalTiles;

        if (newIndex < oldIndex || (newIndex == 0 && steps > 0))
        {
            token.money += gameSettings.passStartBonus;
        }

        token.currentTileIndex = newIndex;

        Vector3 tilePos = boardGenerator.GetTileWorldPosition(newIndex);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = tilePos + new Vector3(offsetX, tokenHeight, 0);

        UpdateMoneyDisplay();
    }

    IEnumerator HandleTileLanding(PlayerToken token, Tile tile)
    {
        if (tile.tileType == TileType.Property || tile.tileType == TileType.Vacation)
        {
            if (tile.ownerId == -1)
                yield return StartCoroutine(HandleEmptyProperty(token, tile));
            else if (tile.ownerId == token.playerId)
                yield return StartCoroutine(HandleOwnProperty(token, tile));
            else
                yield return StartCoroutine(HandleOpponentProperty(token, tile));
        }
        else
        {
            switch (tile.tileType)
            {
                case TileType.GoToStart:
                    yield return StartCoroutine(HandleGoToStart(token));
                    break;
                case TileType.Jail:
                    bool jailAck = false;
                    UIManager.Instance.ShowGoToJailInfoPanel(() => jailAck = true);
                    while (!jailAck) yield return null;
                    yield return StartCoroutine(SendToJail(token));
                    break;
                case TileType.Tax:
                    yield return StartCoroutine(HandleTax(token));
                    break;
                case TileType.Bonus:
                    yield return StartCoroutine(HandleChanceBonusCard(token, CardType.Bonus));
                    break;
                case TileType.Chance:
                    yield return StartCoroutine(HandleChanceBonusCard(token, CardType.Chance));
                    break;
                default:
                    yield return new WaitForSeconds(1f);
                    break;
            }
        }
    }

    IEnumerator HandleEmptyProperty(PlayerToken token, Tile tile)
    {
        bool decisionMade = false;
        bool wantsToBuy = false;

        UIManager.Instance.ShowPurchasePanel(
            tile,
            token.money,    // ← Para parametresi eklendi
            buyCallback: () => { wantsToBuy = true; decisionMade = true; },
            passCallback: () => { wantsToBuy = false; decisionMade = true; }
        );

        while (!decisionMade) yield return null;

        if (!wantsToBuy) yield break;
        if (token.money < tile.basePrice) yield break;

        int difficulty;
        Category questionCategory;

        if (tile.tileType == TileType.Vacation)
        {
            difficulty = 5;
            questionCategory = AllPlayableCategories[Random.Range(0, AllPlayableCategories.Length)];
        }
        else
        {
            difficulty = 2;
            questionCategory = tile.category;
        }

        bool answeredCorrectly = false;
        bool answered = false;

        UIManager.Instance.ShowQuestionPanel(questionCategory, difficulty,
            answerCallback: (correct) => { answeredCorrectly = correct; answered = true; });

        while (!answered) yield return null;

        if (answeredCorrectly)
        {
            token.money -= tile.basePrice;
            tile.ownerId = token.playerId;
            UpdateTileVisual(tile, token.playerColor);

            if (tile.tileType == TileType.Vacation)
            {
                token.vacationCount++;
                CheckVacationVictory(token);
            }

            UpdateMoneyDisplay();
        }
    }

    IEnumerator HandleOwnProperty(PlayerToken token, Tile tile)
    {
        if (tile.buildingLevel >= 5)
        {
            yield return new WaitForSeconds(1f);
            yield break;
        }

        bool decisionMade = false;
        int chosenLevel = 0;

        UIManager.Instance.ShowBuildingPanel(
            tile, token.money,
            buildCallback: (level) => { chosenLevel = level; decisionMade = true; },
            passCallback: () => { chosenLevel = 0; decisionMade = true; }
        );

        while (!decisionMade) yield return null;

        if (chosenLevel == 0) yield break;

        int costPerLevel = tile.basePrice / 2;
        int newLevelTotal = chosenLevel == 5 ? costPerLevel * 6 : costPerLevel * chosenLevel;
        int currentLevelTotal = GetTotalCost(tile.buildingLevel, costPerLevel);
        int totalCost = newLevelTotal - currentLevelTotal;

        if (token.money < totalCost) yield break;

        int difficulty;
        Category questionCategory;

        if (tile.tileType == TileType.Vacation)
        {
            difficulty = 5;
            questionCategory = AllPlayableCategories[Random.Range(0, AllPlayableCategories.Length)];
        }
        else
        {
            difficulty = chosenLevel;
            questionCategory = tile.category;
        }

        bool answeredCorrectly = false;
        bool answered = false;

        UIManager.Instance.ShowQuestionPanel(questionCategory, difficulty,
            answerCallback: (correct) => { answeredCorrectly = correct; answered = true; });

        while (!answered) yield return null;

        if (answeredCorrectly)
        {
            token.money -= totalCost;
            tile.buildingLevel = chosenLevel;
            UpdateTileVisual(tile, token.playerColor);
            UpdateMoneyDisplay();
        }
    }

    IEnumerator HandleOpponentProperty(PlayerToken token, Tile tile)
    {
        int rentPrice = CalculateRent(tile);
        int buyPrice = tile.basePrice * 2;

        PlayerToken owner = GetPlayerById(tile.ownerId);
        if (owner == null) yield break;

        Debug.Log($"{token.playerName} -> {tile.tileName} (kira: {rentPrice}, calma: {buyPrice}, para: {token.money})");

        // Hem rent hem buy için para yetmiyorsa → BANKRUPTCY
        if (token.money < rentPrice && token.money < buyPrice)
        {
            Debug.Log($"{token.playerName} icin ikisi de yetmez, bankruptcy");
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(HandleBankruptcy(token, rentPrice, owner));
            yield break;
        }

        bool decisionMade = false;
        int decision = 0;

        UIManager.Instance.ShowRentPanel(
            tile, buyPrice, rentPrice, token.money,
            buyCallback: () => { decision = 2; decisionMade = true; },
            payRentCallback: () => { decision = 1; decisionMade = true; }
        );

        while (!decisionMade) yield return null;

        if (decision == 1)
        {
            yield return StartCoroutine(PayRent(token, owner, rentPrice));
        }
        else if (decision == 2)
        {
            yield return StartCoroutine(TryToStealProperty(token, owner, tile, buyPrice));
        }
    }

    IEnumerator PayRent(PlayerToken payer, PlayerToken owner, int rentAmount)
    {
        if (payer.money < rentAmount)
        {
            // Burası teorik olarak gelmemeli ama güvenlik için
            yield return StartCoroutine(HandleBankruptcy(payer, rentAmount, owner));
            yield break;
        }

        payer.money -= rentAmount;
        owner.money += rentAmount;
        UpdateMoneyDisplay();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator TryToStealProperty(PlayerToken stealer, PlayerToken owner, Tile tile, int buyPrice)
    {
        int rentPrice = CalculateRent(tile);

        if (stealer.money < buyPrice)
        {
            yield return StartCoroutine(PayRent(stealer, owner, rentPrice));
            yield break;
        }

        int difficulty = 5;
        Category questionCategory;

        if (tile.tileType == TileType.Vacation)
            questionCategory = AllPlayableCategories[Random.Range(0, AllPlayableCategories.Length)];
        else
            questionCategory = tile.category;

        bool answeredCorrectly = false;
        bool answered = false;

        UIManager.Instance.ShowQuestionPanel(questionCategory, difficulty,
            answerCallback: (correct) => { answeredCorrectly = correct; answered = true; });

        while (!answered) yield return null;

        if (answeredCorrectly)
        {
            stealer.money -= buyPrice;
            owner.money += buyPrice;
            tile.ownerId = stealer.playerId;

            if (tile.tileType == TileType.Vacation)
            {
                owner.vacationCount--;
                stealer.vacationCount++;
                CheckVacationVictory(stealer);
            }

            UpdateTileVisual(tile, stealer.playerColor);
            UpdateMoneyDisplay();
        }
        else
        {
            yield return StartCoroutine(PayRent(stealer, owner, rentPrice));
        }
    }

    IEnumerator HandleGoToStart(PlayerToken token)
    {
        bool ack = false;
        UIManager.Instance.ShowGoToStartPanel(() => ack = true);
        while (!ack) yield return null;

        token.currentTileIndex = 0;
        Vector3 startPos = boardGenerator.GetTileWorldPosition(0);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = startPos + new Vector3(offsetX, tokenHeight, 0);
        yield return new WaitForSeconds(0.3f);
    }

    // ============= TAX =============

    IEnumerator HandleTax(PlayerToken token)
    {
        int taxRate = gameSettings.taxRate;
        int taxAmount = (token.money * taxRate) / 100;

        if (taxAmount > token.money) taxAmount = token.money;

        bool ack = false;
        UIManager.Instance.ShowTaxPanel(taxRate, taxAmount, () => ack = true);
        while (!ack) yield return null;

        token.money -= taxAmount;
        UpdateMoneyDisplay();
        yield return new WaitForSeconds(0.3f);
    }

    // ============= CHANCE / BONUS =============

    IEnumerator HandleChanceBonusCard(PlayerToken token, CardType type)
    {
        if (ChanceBonusManager.Instance == null) yield break;

        ChanceBonusCard card = (type == CardType.Bonus)
            ? ChanceBonusManager.Instance.DrawBonusCard()
            : ChanceBonusManager.Instance.DrawChanceCard();

        if (card == null) yield break;

        bool ack = false;
        UIManager.Instance.ShowChanceBonusPanel(type, card, () => ack = true);
        while (!ack) yield return null;

        yield return StartCoroutine(ApplyCardEffect(token, card));
    }

    IEnumerator ApplyCardEffect(PlayerToken token, ChanceBonusCard card)
    {
        switch (card.effect)
        {
            case CardEffect.AddMoney:
                token.money += card.amount;
                UpdateMoneyDisplay();
                yield return new WaitForSeconds(0.5f);
                break;

            case CardEffect.SubtractMoney:
                if (token.money < card.amount)
                {
                    yield return StartCoroutine(HandleBankruptcy(token, card.amount, null));
                }
                else
                {
                    token.money -= card.amount;
                    UpdateMoneyDisplay();
                    yield return new WaitForSeconds(0.5f);
                }
                break;

            case CardEffect.CollectFromAllPlayers:
                int totalCollected = 0;
                foreach (var other in playerTokens)
                {
                    if (other.playerId == token.playerId || other.isBankrupt) continue;
                    int payAmount = Mathf.Min(card.amount, other.money);
                    other.money -= payAmount;
                    totalCollected += payAmount;
                }
                token.money += totalCollected;
                UpdateMoneyDisplay();
                yield return new WaitForSeconds(0.5f);
                break;

            case CardEffect.GoToJail:
                yield return StartCoroutine(SendToJail(token));
                break;

            case CardEffect.GoToStart:
                token.currentTileIndex = 0;
                Vector3 startPos = boardGenerator.GetTileWorldPosition(0);
                float offsetX0 = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
                token.transform.position = startPos + new Vector3(offsetX0, tokenHeight, 0);
                token.money += gameSettings.passStartBonus;
                UpdateMoneyDisplay();
                yield return new WaitForSeconds(1f);
                break;

            case CardEffect.MoveForward:
                yield return StartCoroutine(MoveAndInteract(token, card.amount));
                break;

            case CardEffect.MoveBackward:
                yield return StartCoroutine(MoveAndInteract(token, -card.amount));
                break;

            case CardEffect.GoToNearestVacation:
                yield return StartCoroutine(MoveToNearestVacation(token));
                break;
        }
    }

    IEnumerator MoveAndInteract(PlayerToken token, int steps)
    {
        int totalTiles = boardGenerator.GetTileCount();
        int oldIndex = token.currentTileIndex;
        int newIndex = ((oldIndex + steps) % totalTiles + totalTiles) % totalTiles;

        if (steps > 0 && newIndex < oldIndex)
            token.money += gameSettings.passStartBonus;

        token.currentTileIndex = newIndex;
        Vector3 tilePos = boardGenerator.GetTileWorldPosition(newIndex);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = tilePos + new Vector3(offsetX, tokenHeight, 0);

        UpdateMoneyDisplay();
        yield return new WaitForSeconds(0.8f);

        Tile landedTile = boardGenerator.GetTile(newIndex);
        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideInfoPanel();

        yield return StartCoroutine(HandleTileLanding(token, landedTile));
    }

    IEnumerator MoveToNearestVacation(PlayerToken token)
    {
        int totalTiles = boardGenerator.GetTileCount();
        int currentIdx = token.currentTileIndex;
        int nearestVacationIdx = -1;

        for (int offset = 1; offset < totalTiles; offset++)
        {
            int checkIdx = (currentIdx + offset) % totalTiles;
            Tile checkTile = boardGenerator.GetTile(checkIdx);
            if (checkTile != null && checkTile.tileType == TileType.Vacation)
            {
                nearestVacationIdx = checkIdx;
                break;
            }
        }

        if (nearestVacationIdx == -1) yield break;

        if (nearestVacationIdx < currentIdx)
            token.money += gameSettings.passStartBonus;

        token.currentTileIndex = nearestVacationIdx;
        Vector3 tilePos = boardGenerator.GetTileWorldPosition(nearestVacationIdx);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = tilePos + new Vector3(offsetX, tokenHeight, 0);

        UpdateMoneyDisplay();
        yield return new WaitForSeconds(0.8f);

        Tile landedTile = boardGenerator.GetTile(nearestVacationIdx);
        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideInfoPanel();

        yield return StartCoroutine(HandleTileLanding(token, landedTile));
    }

    void EndTurn()
    {
        if (gameEnded) return;
        currentPlayerIndex = (currentPlayerIndex + 1) % playerTokens.Count;
        StartNextTurn();
    }

    // ============= JAIL =============

    IEnumerator SendToJail(PlayerToken token)
    {
        int jailIndex = 10;
        token.currentTileIndex = jailIndex;
        token.isInJail = true;

        int duration = gameSettings.jailDuration;
        if (duration <= 0) duration = 3;
        token.jailTurnsLeft = duration;

        Vector3 jailPos = boardGenerator.GetTileWorldPosition(jailIndex);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = jailPos + new Vector3(offsetX, tokenHeight, 0);

        UpdateMoneyDisplay();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator HandleJailTurn(PlayerToken token)
    {
        Tile jailTile = boardGenerator.GetTile(10);
        int exitFee = gameSettings.jailExitFee;

        bool decisionMade = false;
        int decision = 0;

        UIManager.Instance.ShowExitJailPanel(
            jailTile, exitFee, token.jailTurnsLeft, token.money,
            rollDicesCallback: () => { decision = 1; decisionMade = true; },
            payExitCallback: () => { decision = 2; decisionMade = true; },
            waitCallback: () => { decision = 3; decisionMade = true; }
        );

        while (!decisionMade) yield return null;

        if (decision == 1) yield return StartCoroutine(TryRollOutOfJail(token));
        else if (decision == 2) yield return StartCoroutine(PayToExitJail(token, exitFee));
        else if (decision == 3) yield return StartCoroutine(WaitInJail(token));
    }

    IEnumerator TryRollOutOfJail(PlayerToken token)
    {
        diceManager.RollBothDice();
        while (diceManager.IsRolling()) yield return null;

        int diceTotal = diceManager.GetTotal();
        bool isDouble = diceManager.IsDouble();

        if (isDouble)
        {
            token.isInJail = false;
            token.jailTurnsLeft = 0;
            token.consecutiveDoubles = 0;

            MovePlayer(token, diceTotal);
            yield return new WaitForSeconds(1f);

            Tile landedTile = boardGenerator.GetTile(token.currentTileIndex);
            UIManager.Instance.ShowInfoPanel(landedTile);
            yield return new WaitForSeconds(1.5f);
            UIManager.Instance.HideInfoPanel();

            yield return StartCoroutine(HandleTileLanding(token, landedTile));
        }
        else
        {
            token.jailTurnsLeft--;
            if (token.jailTurnsLeft <= 0)
                yield return StartCoroutine(ForceExitJail(token));
        }

        EndTurn();
    }

    IEnumerator PayToExitJail(PlayerToken token, int exitFee)
    {
        token.money -= exitFee;
        token.isInJail = false;
        token.jailTurnsLeft = 0;
        token.consecutiveDoubles = 0;

        UpdateMoneyDisplay();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(NormalDiceRollAfterJail(token));
    }

    IEnumerator WaitInJail(PlayerToken token)
    {
        token.jailTurnsLeft--;
        if (token.jailTurnsLeft <= 0)
            yield return StartCoroutine(ForceExitJail(token));

        EndTurn();
    }

    IEnumerator ForceExitJail(PlayerToken token)
    {
        int exitFee = gameSettings.jailExitFee;

        if (token.money < exitFee)
        {
            // Zorunlu çıkışta para yetmezse iflas
            yield return StartCoroutine(HandleBankruptcy(token, exitFee, null));
        }
        else
        {
            token.money -= exitFee;
        }

        token.isInJail = false;
        token.jailTurnsLeft = 0;
        token.consecutiveDoubles = 0;

        UpdateMoneyDisplay();
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator NormalDiceRollAfterJail(PlayerToken token)
    {
        diceManager.RollBothDice();
        while (diceManager.IsRolling()) yield return null;

        int diceTotal = diceManager.GetTotal();
        bool isDouble = diceManager.IsDouble();

        if (isDouble) token.consecutiveDoubles = 1;

        MovePlayer(token, diceTotal);
        yield return new WaitForSeconds(1f);

        Tile landedTile = boardGenerator.GetTile(token.currentTileIndex);
        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideInfoPanel();

        yield return StartCoroutine(HandleTileLanding(token, landedTile));

        if (isDouble) rollButton.interactable = true;
        else EndTurn();
    }

    // ============= BANKRUPTCY =============

    IEnumerator HandleBankruptcy(PlayerToken debtor, int debt, PlayerToken creditor)
    {
        Debug.Log($"[BANKRUPTCY] {debtor.playerName}, borc: {debt}");

        // Sahip olduğu araziler
        List<Tile> ownedProperties = GetOwnedProperties(debtor);

        // Hiç arazi yoksa direkt iflas
        if (ownedProperties.Count == 0)
        {
            Debug.Log($"{debtor.playerName} arazisi yok, direkt iflas");
            yield return StartCoroutine(DeclareBankruptcy(debtor, creditor));
            yield break;
        }

        // Tüm arazilerin toplam değeri borcu karşılıyor mu?
        int totalAvailable = debtor.money;
        foreach (var t in ownedProperties)
            totalAvailable += CalculateTileSellValue(t);

        if (totalAvailable < debt)
        {
            Debug.Log($"{debtor.playerName} hepsini satsa bile yetmiyor, iflas");
            // Tüm parası ve arazileri creditor'a (varsa) geçer
            yield return StartCoroutine(DeclareBankruptcy(debtor, creditor));
            yield break;
        }

        // Panel aç
        bool sold = false;
        List<Tile> soldTiles = null;

        UIManager.Instance.ShowBankruptcyPanel(debtor, debt, ownedProperties,
            sellCallback: (tiles) => { soldTiles = tiles; sold = true; });

        while (!sold) yield return null;

        // Seçili araziler satılır
        int totalEarned = 0;
        foreach (var tile in soldTiles)
        {
            int sellValue = CalculateTileSellValue(tile);
            totalEarned += sellValue;

            // Arazi temizlenir
            tile.ownerId = -1;
            tile.buildingLevel = 0;
            UpdateTileVisual(tile, Color.white);

            if (tile.tileType == TileType.Vacation)
                debtor.vacationCount--;
        }

        debtor.money += totalEarned;
        Debug.Log($"{debtor.playerName} {soldTiles.Count} arazi sattı, +{totalEarned} TL");

        // Borç öde
        if (creditor != null)
        {
            debtor.money -= debt;
            creditor.money += debt;
        }
        else
        {
            debtor.money -= debt;
            if (debtor.money < 0) debtor.money = 0;
        }

        UpdateMoneyDisplay();
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator DeclareBankruptcy(PlayerToken debtor, PlayerToken creditor)
    {
        Debug.Log($"[IFLAS] {debtor.playerName} iflas etti!");

        // Tüm arazilerini serbest bırak
        List<Tile> ownedProperties = GetOwnedProperties(debtor);
        foreach (var tile in ownedProperties)
        {
            // Eğer alacaklı varsa arazileri ona ver, yoksa boşalt
            if (creditor != null)
            {
                tile.ownerId = creditor.playerId;
                UpdateTileVisual(tile, creditor.playerColor);

                if (tile.tileType == TileType.Vacation)
                {
                    debtor.vacationCount--;
                    creditor.vacationCount++;
                }
            }
            else
            {
                tile.ownerId = -1;
                tile.buildingLevel = 0;
                UpdateTileVisual(tile, Color.white);

                if (tile.tileType == TileType.Vacation)
                    debtor.vacationCount--;
            }
        }

        // Kalan parayı alacaklıya ver
        if (creditor != null && debtor.money > 0)
        {
            creditor.money += debtor.money;
        }

        debtor.money = 0;
        debtor.isBankrupt = true;
        debtor.isInJail = false;
        debtor.jailTurnsLeft = 0;

        UpdateMoneyDisplay();

        // Lose panel göster
        UIManager.Instance.ShowLosePanel(debtor);

        yield return new WaitForSeconds(2f);

        // Vacation victory kontrolü (creditor 3 tatil mi aldı?)
        if (creditor != null)
            CheckVacationVictory(creditor);

        // Win condition kontrolü
        CheckWinCondition();
    }

    List<Tile> GetOwnedProperties(PlayerToken token)
    {
        List<Tile> owned = new List<Tile>();
        int totalTiles = boardGenerator.GetTileCount();

        for (int i = 0; i < totalTiles; i++)
        {
            Tile t = boardGenerator.GetTile(i);
            if (t != null && t.ownerId == token.playerId)
                owned.Add(t);
        }
        return owned;
    }

    int CalculateTileSellValue(Tile tile)
    {
        int landValue = tile.basePrice / 2;
        int costPerLevel = tile.basePrice / 2;
        int currentBuildingCost = GetTotalCost(tile.buildingLevel, costPerLevel);
        int buildingRefund = currentBuildingCost / 2;
        return landValue + buildingRefund;
    }

    // ============= WIN CONDITIONS =============

    void CheckVacationVictory(PlayerToken token)
    {
        if (gameSettings.vacationVictoryEnabled && token.vacationCount >= gameSettings.vacationVictoryCount)
        {
            Debug.Log($"[WIN] {token.playerName} {token.vacationCount} tatil topladı, kazandı!");
            gameEnded = true;
            UIManager.Instance.ShowWinPanel(token);
        }
    }

    void CheckWinCondition()
    {
        if (gameEnded) return;

        int activeCount = 0;
        PlayerToken lastActive = null;
        foreach (var p in playerTokens)
        {
            if (!p.isBankrupt)
            {
                activeCount++;
                lastActive = p;
            }
        }

        if (activeCount <= 1 && lastActive != null)
        {
            Debug.Log($"[WIN] Son oyuncu: {lastActive.playerName}");
            gameEnded = true;
            UIManager.Instance.ShowWinPanel(lastActive);
        }
    }

    // ============= VISUAL =============

    void UpdateTileVisual(Tile tile, Color ownerColor)
    {
        if (tile.currentBuilding != null)
        {
            Destroy(tile.currentBuilding);
            tile.currentBuilding = null;
        }

        if (tile.ownerId == -1) return;

        GameObject prefabToUse = GetBuildingPrefab(tile.buildingLevel);
        if (prefabToUse == null) return;

        Vector3 buildingPos = tile.transform.position + new Vector3(0, buildingHeightOffset, 0);
        GameObject building = Instantiate(prefabToUse, buildingPos, Quaternion.identity);
        building.transform.SetParent(tile.transform);
        building.name = $"Building_{tile.tileName}_Level{tile.buildingLevel}";

        Renderer renderer = building.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = ownerColor;
        }

        tile.currentBuilding = building;
    }

    GameObject GetBuildingPrefab(int buildingLevel)
    {
        switch (buildingLevel)
        {
            case 0: return ownershipMarkerPrefab;
            case 1: return building1Prefab;
            case 2: return building2Prefab;
            case 3: return building3Prefab;
            case 4: return building4Prefab;
            case 5: return hotelPrefab;
            default: return null;
        }
    }

    // ============= HELPERS =============

    int CalculateRent(Tile tile)
    {
        int baseRent;
        if (tile.buildingLevel == 0) baseRent = tile.basePrice / 4;
        else baseRent = (tile.basePrice / 2) * tile.buildingLevel;

        // Aynı renk grubunun tamamı aynı kişide → kira x2
        if (HasColorGroupMonopoly(tile))
            baseRent *= 2;

        return baseRent;
    }

    PlayerToken GetPlayerById(int playerId)
    {
        foreach (var token in playerTokens)
            if (token.playerId == playerId) return token;
        return null;
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
    // Bir tile'ın renk grubundaki TÜM property/vacation tile'lar aynı kişiye aitse true
    bool HasColorGroupMonopoly(Tile tile)
    {
        if (tile.ownerId < 0) return false;
        if (tile.tileType != TileType.Property && tile.tileType != TileType.Vacation)
            return false;

        int totalTiles = boardGenerator.GetTileCount();
        for (int i = 0; i < totalTiles; i++)
        {
            Tile t = boardGenerator.GetTile(i);
            if (t == null) continue;
            if (t.tileType != TileType.Property && t.tileType != TileType.Vacation) continue;
            if (t.groupColor != tile.groupColor) continue;
            if (t.ownerId != tile.ownerId) return false;
        }
        return true;
    }
}