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

    private static readonly Category[] AllPlayableCategories = new Category[]
    {
        Category.Tarih,
        Category.Cografya,
        Category.Sanat,
        Category.Spor,
        Category.Bilim,
        Category.Muzik,
        Category.Edebiyat,
        Category.GenelKultur
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
        var current = playerTokens[currentPlayerIndex];

        Debug.Log($"=== START NEXT TURN === Player: {current.playerName}, isInJail: {current.isInJail}, jailTurnsLeft: {current.jailTurnsLeft}");

        string colorHex = ColorUtility.ToHtmlStringRGB(current.playerColor);
        instructionText.text = $"Turn: <color=#{colorHex}>{current.playerName}</color>";

        UpdateMoneyDisplay();

        if (current.isInJail)
        {
            Debug.Log($"[JAIL] {current.playerName} HAPISTE, RollButton pasif, panel acilacak");
            rollButton.interactable = false;
            StartCoroutine(StartJailTurn(current));
        }
        else
        {
            Debug.Log($"[OK] {current.playerName} normal tur, RollButton aktif");
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
            string marker = (i == currentPlayerIndex) ? "> " : "  ";
            string jailMarker = p.isInJail ? " [JAIL]" : "";
            text += $"{marker}<color=#{colorHex}>{p.playerName}</color>: {moneyStr} TL{jailMarker}\n";
        }

        resultsText.text = text;
    }

    void OnRollButtonClicked()
    {
        if (!gameStarted) return;
        if (diceManager.IsRolling()) return;

        rollButton.interactable = false;
        StartCoroutine(HandleTurn());
    }

    IEnumerator HandleTurn()
    {
        var token = playerTokens[currentPlayerIndex];

        diceManager.RollBothDice();

        while (diceManager.IsRolling())
            yield return null;

        int diceTotal = diceManager.GetTotal();
        bool isDouble = diceManager.IsDouble();

        Debug.Log($"{token.playerName} atti: {diceManager.GetDice1Value()} + {diceManager.GetDice2Value()} = {diceTotal}, cift mi: {isDouble}");

        if (isDouble)
        {
            token.consecutiveDoubles++;

            if (token.consecutiveDoubles >= 3)
            {
                Debug.Log($"{token.playerName} 3 art arda cift atti, hapse gidiyor!");
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
        Debug.Log($"{token.playerName} -> {landedTile.tileName} ({landedTile.tileType})");

        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideInfoPanel();

        yield return StartCoroutine(HandleTileLanding(token, landedTile));

        if (token.isInJail)
        {
            EndTurn();
            yield break;
        }

        if (isDouble)
        {
            Debug.Log($"{token.playerName} cift atti, tekrar zar atma hakki kazandi!");
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
            Debug.Log($"{token.playerName} baslangici gecti, +{gameSettings.passStartBonus}");
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
            {
                yield return StartCoroutine(HandleEmptyProperty(token, tile));
            }
            else if (tile.ownerId == token.playerId)
            {
                yield return StartCoroutine(HandleOwnProperty(token, tile));
            }
            else
            {
                yield return StartCoroutine(HandleOpponentProperty(token, tile));
            }
        }
        else
        {
            switch (tile.tileType)
            {
                case TileType.GoToStart:
                    yield return StartCoroutine(HandleGoToStart(token));
                    break;

                case TileType.Jail:
                    // Önce bilgi paneli
                    bool jailAcknowledged = false;
                    UIManager.Instance.ShowGoToJailInfoPanel(() => jailAcknowledged = true);
                    while (!jailAcknowledged) yield return null;
                    // Sonra hapise gönder
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
                    Debug.Log($"Ozel kare (henuz islenmedi): {tile.tileType}");
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
            buyCallback: () => { wantsToBuy = true; decisionMade = true; },
            passCallback: () => { wantsToBuy = false; decisionMade = true; }
        );

        while (!decisionMade)
            yield return null;

        if (!wantsToBuy)
        {
            Debug.Log($"{token.playerName} pas gecti: {tile.tileName}");
            yield break;
        }

        if (token.money < tile.basePrice)
        {
            Debug.Log($"{token.playerName} parasi yetmiyor!");
            yield break;
        }

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

        UIManager.Instance.ShowQuestionPanel(
            questionCategory,
            difficulty,
            answerCallback: (correct) => {
                answeredCorrectly = correct;
                answered = true;
            }
        );

        while (!answered)
            yield return null;

        if (answeredCorrectly)
        {
            token.money -= tile.basePrice;
            tile.ownerId = token.playerId;

            UpdateTileVisual(tile, token.playerColor);

            if (tile.tileType == TileType.Vacation)
            {
                token.vacationCount++;
                Debug.Log($"{token.playerName} tatil bolgesi aldi! Toplam: {token.vacationCount}");
            }

            UpdateMoneyDisplay();
            Debug.Log($"{token.playerName} {tile.tileName}'yi satin aldi! (-{tile.basePrice} TL)");
        }
        else
        {
            Debug.Log($"{token.playerName} soruyu bilemedi, arazi alamadi");
        }
    }

    IEnumerator HandleOwnProperty(PlayerToken token, Tile tile)
    {
        if (tile.buildingLevel >= 5)
        {
            Debug.Log($"{tile.tileName} otel seviyesinde, daha fazla bina dikilemez");
            yield return new WaitForSeconds(1f);
            yield break;
        }

        bool decisionMade = false;
        int chosenLevel = 0;

        UIManager.Instance.ShowBuildingPanel(
            tile,
            token.money,
            buildCallback: (level) => { chosenLevel = level; decisionMade = true; },
            passCallback: () => { chosenLevel = 0; decisionMade = true; }
        );

        while (!decisionMade)
            yield return null;

        if (chosenLevel == 0)
        {
            Debug.Log($"{token.playerName} bina dikmeyi pas gecti");
            yield break;
        }

        int costPerLevel = tile.basePrice / 2;
        int newLevelTotal = chosenLevel == 5 ? costPerLevel * 6 : costPerLevel * chosenLevel;
        int currentLevelTotal = GetTotalCost(tile.buildingLevel, costPerLevel);
        int totalCost = newLevelTotal - currentLevelTotal;

        if (token.money < totalCost)
        {
            Debug.Log($"{token.playerName} parasi yetmiyor!");
            yield break;
        }

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

        UIManager.Instance.ShowQuestionPanel(
            questionCategory,
            difficulty,
            answerCallback: (correct) => {
                answeredCorrectly = correct;
                answered = true;
            }
        );

        while (!answered)
            yield return null;

        if (answeredCorrectly)
        {
            token.money -= totalCost;
            tile.buildingLevel = chosenLevel;
            UpdateMoneyDisplay();

            UpdateTileVisual(tile, token.playerColor);

            string levelName = chosenLevel == 5 ? "Otel" : $"{chosenLevel} Kat";
            Debug.Log($"{token.playerName} {tile.tileName}'de {levelName} dikti! (-{totalCost} TL)");
        }
        else
        {
            Debug.Log($"{token.playerName} soruyu bilemedi, bina dikilemedi");
        }
    }

    IEnumerator HandleOpponentProperty(PlayerToken token, Tile tile)
    {
        int rentPrice = CalculateRent(tile);
        int buyPrice = tile.basePrice * 2;

        PlayerToken owner = GetPlayerById(tile.ownerId);
        if (owner == null)
        {
            Debug.LogError($"Sahip bulunamadi: {tile.ownerId}");
            yield break;
        }

        Debug.Log($"{token.playerName} -> {tile.tileName} (sahibi: {owner.playerName}, kira: {rentPrice}, calma: {buyPrice})");

        bool decisionMade = false;
        int decision = 0;

        UIManager.Instance.ShowRentPanel(
            tile,
            buyPrice,
            rentPrice,
            buyCallback: () => { decision = 2; decisionMade = true; },
            payRentCallback: () => { decision = 1; decisionMade = true; }
        );

        while (!decisionMade)
            yield return null;

        if (decision == 1)
        {
            yield return StartCoroutine(PayRent(token, owner, rentPrice));
            yield break;
        }

        if (decision == 2)
        {
            yield return StartCoroutine(TryToStealProperty(token, owner, tile, buyPrice));
            yield break;
        }
    }

    IEnumerator PayRent(PlayerToken payer, PlayerToken owner, int rentAmount)
    {
        if (payer.money < rentAmount)
        {
            Debug.Log($"{payer.playerName} parasi yetmiyor! Tum parasi gidiyor: {payer.money} TL");
            owner.money += payer.money;
            payer.money = 0;
        }
        else
        {
            payer.money -= rentAmount;
            owner.money += rentAmount;
            Debug.Log($"{payer.playerName} kira odedi: {rentAmount} TL -> {owner.playerName}");
        }

        UpdateMoneyDisplay();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator TryToStealProperty(PlayerToken stealer, PlayerToken owner, Tile tile, int buyPrice)
    {
        int rentPrice = CalculateRent(tile);

        if (stealer.money < buyPrice)
        {
            Debug.Log($"{stealer.playerName} calmaya parasi yetmiyor, kira oduyor");
            yield return StartCoroutine(PayRent(stealer, owner, rentPrice));
            yield break;
        }

        int difficulty = 4;
        Category questionCategory;

        if (tile.tileType == TileType.Vacation)
        {
            questionCategory = AllPlayableCategories[Random.Range(0, AllPlayableCategories.Length)];
        }
        else
        {
            questionCategory = tile.category;
        }

        bool answeredCorrectly = false;
        bool answered = false;

        UIManager.Instance.ShowQuestionPanel(
            questionCategory,
            difficulty,
            answerCallback: (correct) => {
                answeredCorrectly = correct;
                answered = true;
            }
        );

        while (!answered)
            yield return null;

        if (answeredCorrectly)
        {
            stealer.money -= buyPrice;
            owner.money += buyPrice;

            tile.ownerId = stealer.playerId;

            if (tile.tileType == TileType.Vacation)
            {
                owner.vacationCount--;
                stealer.vacationCount++;
                Debug.Log($"Tatil bolgesi el degistirdi. {owner.playerName}: {owner.vacationCount}, {stealer.playerName}: {stealer.vacationCount}");
            }

            UpdateTileVisual(tile, stealer.playerColor);

            UpdateMoneyDisplay();
            Debug.Log($"{stealer.playerName} {tile.tileName}'yi caldi! ({owner.playerName}'den, -{buyPrice} TL)");
        }
        else
        {
            Debug.Log($"{stealer.playerName} soruyu bilemedi, calamadi. Kira oduyor...");
            yield return StartCoroutine(PayRent(stealer, owner, rentPrice));
        }
    }

    IEnumerator HandleGoToStart(PlayerToken token)
    {
        Debug.Log($"{token.playerName} -> Baslangica Don karesine dustu");

        // Panel göster
        bool acknowledged = false;
        UIManager.Instance.ShowGoToStartPanel(() => acknowledged = true);

        while (!acknowledged)
            yield return null;

        // Başlangıca ışınla
        token.currentTileIndex = 0;

        Vector3 startPos = boardGenerator.GetTileWorldPosition(0);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = startPos + new Vector3(offsetX, tokenHeight, 0);

        Debug.Log($"{token.playerName} baslangica dondu (para alinmadi)");

        yield return new WaitForSeconds(0.3f);
    }

    // ============= TAX =============

    IEnumerator HandleTax(PlayerToken token)
    {
        int taxRate = gameSettings.taxRate;
        int taxAmount = (token.money * taxRate) / 100;

        Debug.Log($"[TAX] {token.playerName} vergi karesine dustu. Para: {token.money}, Oran: %{taxRate}, Vergi: {taxAmount} TL");

        if (taxAmount > token.money)
        {
            taxAmount = token.money;
        }

        // Paneli göster
        bool acknowledged = false;
        UIManager.Instance.ShowTaxPanel(taxRate, taxAmount, () => acknowledged = true);

        while (!acknowledged)
            yield return null;

        // Para kesimi
        token.money -= taxAmount;
        UpdateMoneyDisplay();

        Debug.Log($"[TAX] {token.playerName} {taxAmount} TL vergi odedi. Kalan: {token.money} TL");

        yield return new WaitForSeconds(0.3f);
    }

    // ============= CHANCE / BONUS =============

    IEnumerator HandleChanceBonusCard(PlayerToken token, CardType type)
    {
        if (ChanceBonusManager.Instance == null)
        {
            Debug.LogError("ChanceBonusManager.Instance NULL! Sahnede ChanceBonusManager objesi olmali.");
            yield break;
        }

        // Kart çek
        ChanceBonusCard card = (type == CardType.Bonus)
            ? ChanceBonusManager.Instance.DrawBonusCard()
            : ChanceBonusManager.Instance.DrawChanceCard();

        if (card == null)
        {
            Debug.LogError("Kart cekilemedi!");
            yield break;
        }

        Debug.Log($"[{type}] {token.playerName} kart cekti: {card.title} ({card.effect}, {card.amount})");

        // Paneli göster
        bool acknowledged = false;
        UIManager.Instance.ShowChanceBonusPanel(type, card, () => acknowledged = true);

        while (!acknowledged)
            yield return null;

        // Etkiyi uygula
        yield return StartCoroutine(ApplyCardEffect(token, card));
    }

    IEnumerator ApplyCardEffect(PlayerToken token, ChanceBonusCard card)
    {
        switch (card.effect)
        {
            case CardEffect.AddMoney:
                token.money += card.amount;
                UpdateMoneyDisplay();
                Debug.Log($"{token.playerName} +{card.amount} TL aldi");
                yield return new WaitForSeconds(0.5f);
                break;

            case CardEffect.SubtractMoney:
                int subAmount = Mathf.Min(card.amount, token.money);
                token.money -= subAmount;
                UpdateMoneyDisplay();
                Debug.Log($"{token.playerName} -{subAmount} TL verdi");
                yield return new WaitForSeconds(0.5f);
                break;

            case CardEffect.CollectFromAllPlayers:
                int totalCollected = 0;
                foreach (var other in playerTokens)
                {
                    if (other.playerId == token.playerId) continue;

                    int payAmount = Mathf.Min(card.amount, other.money);
                    other.money -= payAmount;
                    totalCollected += payAmount;
                }
                token.money += totalCollected;
                UpdateMoneyDisplay();
                Debug.Log($"{token.playerName} diger oyunculardan toplam {totalCollected} TL aldi");
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
                Debug.Log($"{token.playerName} baslangica gitti +{gameSettings.passStartBonus} TL");
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
        int newIndex = ((oldIndex + steps) % totalTiles + totalTiles) % totalTiles;  // negatifte de calisir

        // Başlangıçtan geçti mi? (ileri giderken)
        if (steps > 0 && newIndex < oldIndex)
        {
            token.money += gameSettings.passStartBonus;
            Debug.Log($"{token.playerName} baslangici gecti, +{gameSettings.passStartBonus}");
        }

        token.currentTileIndex = newIndex;

        Vector3 tilePos = boardGenerator.GetTileWorldPosition(newIndex);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = tilePos + new Vector3(offsetX, tokenHeight, 0);

        UpdateMoneyDisplay();

        yield return new WaitForSeconds(0.8f);

        // Yeni kareye düştüğünde etkileşim
        Tile landedTile = boardGenerator.GetTile(newIndex);
        Debug.Log($"{token.playerName} -> {landedTile.tileName} (Sans/Bonus hareketi)");

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

        // İleri yönde en yakın vacation tile'ı bul
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

        if (nearestVacationIdx == -1)
        {
            Debug.LogWarning("Tatil bolgesi bulunamadi!");
            yield break;
        }

        // Başlangıçtan geçtiyse para ver
        if (nearestVacationIdx < currentIdx)
        {
            token.money += gameSettings.passStartBonus;
            Debug.Log($"{token.playerName} baslangici gecti, +{gameSettings.passStartBonus}");
        }

        token.currentTileIndex = nearestVacationIdx;

        Vector3 tilePos = boardGenerator.GetTileWorldPosition(nearestVacationIdx);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = tilePos + new Vector3(offsetX, tokenHeight, 0);

        UpdateMoneyDisplay();

        yield return new WaitForSeconds(0.8f);

        Tile landedTile = boardGenerator.GetTile(nearestVacationIdx);
        Debug.Log($"{token.playerName} -> {landedTile.tileName} (Sans karti ile)");

        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideInfoPanel();

        yield return StartCoroutine(HandleTileLanding(token, landedTile));
    }

    void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % playerTokens.Count;
        StartNextTurn();
    }

    // ============= JAIL =============

    IEnumerator SendToJail(PlayerToken token)
    {
        Debug.Log($"[JAIL] SendToJail BASLADI: {token.playerName}, gameSettings.jailDuration = {gameSettings.jailDuration}");

        int jailIndex = 10;
        token.currentTileIndex = jailIndex;
        token.isInJail = true;

        int duration = gameSettings.jailDuration;
        if (duration <= 0) duration = 3;
        token.jailTurnsLeft = duration;

        Debug.Log($"[JAIL] isInJail = {token.isInJail}, jailTurnsLeft = {token.jailTurnsLeft}");

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

        if (UIManager.Instance == null || UIManager.Instance.exitJailPanel == null)
        {
            Debug.LogError("UIManager veya exitJailPanel NULL!");
            EndTurn();
            yield break;
        }

        bool decisionMade = false;
        int decision = 0;

        UIManager.Instance.ShowExitJailPanel(
            jailTile,
            exitFee,
            token.jailTurnsLeft,
            rollDicesCallback: () => { decision = 1; decisionMade = true; },
            payExitCallback: () => { decision = 2; decisionMade = true; },
            waitCallback: () => { decision = 3; decisionMade = true; }
        );

        while (!decisionMade)
            yield return null;

        if (decision == 1)
        {
            yield return StartCoroutine(TryRollOutOfJail(token));
        }
        else if (decision == 2)
        {
            yield return StartCoroutine(PayToExitJail(token, exitFee));
        }
        else if (decision == 3)
        {
            yield return StartCoroutine(WaitInJail(token));
        }
    }

    IEnumerator TryRollOutOfJail(PlayerToken token)
    {
        diceManager.RollBothDice();

        while (diceManager.IsRolling())
            yield return null;

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
            {
                yield return StartCoroutine(ForceExitJail(token));
            }
        }

        EndTurn();
    }

    IEnumerator PayToExitJail(PlayerToken token, int exitFee)
    {
        if (token.money < exitFee)
        {
            yield return StartCoroutine(TryRollOutOfJail(token));
            yield break;
        }

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
        {
            yield return StartCoroutine(ForceExitJail(token));
        }

        EndTurn();
    }

    IEnumerator ForceExitJail(PlayerToken token)
    {
        int exitFee = gameSettings.jailExitFee;

        if (token.money < exitFee)
        {
            token.money = Mathf.Max(0, token.money - exitFee);
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

        while (diceManager.IsRolling())
            yield return null;

        int diceTotal = diceManager.GetTotal();
        bool isDouble = diceManager.IsDouble();

        if (isDouble)
        {
            token.consecutiveDoubles = 1;
        }

        MovePlayer(token, diceTotal);

        yield return new WaitForSeconds(1f);

        Tile landedTile = boardGenerator.GetTile(token.currentTileIndex);
        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideInfoPanel();

        yield return StartCoroutine(HandleTileLanding(token, landedTile));

        if (isDouble)
        {
            rollButton.interactable = true;
        }
        else
        {
            EndTurn();
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

        if (tile.ownerId == -1)
            return;

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
        if (tile.buildingLevel == 0)
        {
            return tile.basePrice / 4;
        }
        else
        {
            return (tile.basePrice / 2) * tile.buildingLevel;
        }
    }

    PlayerToken GetPlayerById(int playerId)
    {
        foreach (var token in playerTokens)
        {
            if (token.playerId == playerId)
                return token;
        }
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
}