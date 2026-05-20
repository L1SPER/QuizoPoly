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

    [Header("UI (Mevcut UI Tekrar Kullanılıyor)")]
    public TMP_Text titleText;
    public TMP_Text instructionText;
    public TMP_Text resultsText;
    public Button rollButton;

    [Header("Pul Yerleşimi")]
    public float tokenHeight = 0.5f;

    [HideInInspector] public List<PlayerToken> playerTokens = new List<PlayerToken>();
    [HideInInspector] public int currentPlayerIndex = 0;

    private bool gameStarted = false;

    [Header("Bina Prefab'ları")]
    public GameObject ownershipMarkerPrefab;  // Sadece alındı belirtisi
    public GameObject building1Prefab;        // 1 Kat
    public GameObject building2Prefab;        // 2 Kat
    public GameObject building3Prefab;        // 3 Kat
    public GameObject building4Prefab;        // 4 Kat
    public GameObject hotelPrefab;            // Otel

    [Header("Bina Yerleştirme")]
    public float buildingHeightOffset = 0.2f;  // Tile üstünden ne kadar yukarıda

    // Vacation kareler için rastgele kategori seçiminde kullanılır
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

        string colorHex = ColorUtility.ToHtmlStringRGB(current.playerColor);
        instructionText.text = $"Turn: <color=#{colorHex}>{current.playerName}</color>";

        UpdateMoneyDisplay();
        rollButton.interactable = true;

        Debug.Log($"Sıra: {current.playerName}, Para: {current.money}");
    }

    void UpdateMoneyDisplay()
    {
        string text = "<b>PLAYERS</b>\n\n";

        for (int i = 0; i < playerTokens.Count; i++)
        {
            var p = playerTokens[i];
            string colorHex = ColorUtility.ToHtmlStringRGB(p.playerColor);
            string moneyStr = p.money.ToString("N0", new System.Globalization.CultureInfo("tr-TR"));
            string marker = (i == currentPlayerIndex) ? "▶ " : "  ";
            text += $"{marker}<color=#{colorHex}>{p.playerName}</color>: {moneyStr} ₺\n";
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

        Debug.Log($"{token.playerName} attı: {diceManager.GetDice1Value()} + {diceManager.GetDice2Value()} = {diceTotal}, çift mi: {isDouble}");

        // Çift sayacını güncelle
        if (isDouble)
        {
            token.consecutiveDoubles++;

            // 3 art arda çift → hapise git
            if (token.consecutiveDoubles >= 3)
            {
                Debug.Log($"{token.playerName} 3 art arda çift attı, hapse gidiyor!");
                yield return StartCoroutine(SendToJail(token));
                token.consecutiveDoubles = 0;  // sıfırla
                EndTurn();
                yield break;
            }
        }
        else
        {
            token.consecutiveDoubles = 0;  // çift değilse sıfırla
        }

        // Normal hareket
        MovePlayer(token, diceTotal);

        yield return new WaitForSeconds(1f);

        Tile landedTile = boardGenerator.GetTile(token.currentTileIndex);
        Debug.Log($"{token.playerName} → {landedTile.tileName} ({landedTile.tileType})");

        // InfoPanel göster
        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideInfoPanel();

        // Kare etkileşimi
        yield return StartCoroutine(HandleTileLanding(token, landedTile));

        // Eğer çift attıysa tekrar zar atma hakkı
        if (isDouble)
        {
            Debug.Log($"{token.playerName} çift attı, tekrar zar atma hakkı kazandı! (consecutive: {token.consecutiveDoubles})");

            // Sıra geçmiyor, aynı oyuncu tekrar atacak
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
            Debug.Log($"{token.playerName} başlangıcı geçti, +{gameSettings.passStartBonus}");
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
                // Başkasının arazisi
                yield return StartCoroutine(HandleOpponentProperty(token, tile));
            }
        }
        else
        {
            // Özel kareler
            switch (tile.tileType)
            {
                case TileType.GoToStart:
                    yield return StartCoroutine(HandleGoToStart(token));
                    break;

                // İleride: Tax, Chance, Bonus, Jail buraya gelecek

                default:
                    Debug.Log($"Özel kare (henüz işlenmedi): {tile.tileType}");
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
            Debug.Log($"{token.playerName} pas geçti: {tile.tileName}");
            yield break;
        }

        if (token.money < tile.basePrice)
        {
            Debug.Log($"{token.playerName} parası yetmiyor!");
            yield break;
        }

        // Tatil bölgesi mi Property mi? Zorluk ve kategori belirle
        int difficulty;
        Category questionCategory;

        if (tile.tileType == TileType.Vacation)
        {
            difficulty = 5;  // Impossible
            questionCategory = AllPlayableCategories[Random.Range(0, AllPlayableCategories.Length)];
            Debug.Log($"Tatil bölgesi sorusu — rastgele kategori: {questionCategory}, zorluk: Impossible");
        }
        else
        {
            difficulty = 2;  // Easy
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

            // Görsel güncelle: arazi alındı belirtisi koy
            UpdateTileVisual(tile, token.playerColor);

            if (tile.tileType == TileType.Vacation)
            {
                token.vacationCount++;
                Debug.Log($"{token.playerName} tatil bölgesi aldı! Toplam: {token.vacationCount}");
            }

            UpdateMoneyDisplay();
            Debug.Log($"{token.playerName} {tile.tileName}'yi satın aldı! (-{tile.basePrice} ₺)");
        }
        else
        {
            Debug.Log($"{token.playerName} soruyu bilemedi, arazi alamadı");
        }
    }

    IEnumerator HandleOwnProperty(PlayerToken token, Tile tile)
    {
        // Otel varsa daha fazla yükseltilemez
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
            Debug.Log($"{token.playerName} bina dikmeyi pas geçti");
            yield break;
        }

        // Maliyet hesapla (BuildingPanelUI ile aynı formül)
        int costPerLevel = tile.basePrice / 2;
        int totalCost = chosenLevel == 5 ? costPerLevel * 6 : costPerLevel * chosenLevel;

        if (token.money < totalCost)
        {
            Debug.Log($"{token.playerName} parası yetmiyor!");
            yield break;
        }

        // Zorluk ve kategori — tatil bölgesi farklı muamele
        int difficulty;
        Category questionCategory;

        if (tile.tileType == TileType.Vacation)
        {
            difficulty = 5;  // Impossible
            questionCategory = AllPlayableCategories[Random.Range(0, AllPlayableCategories.Length)];
            Debug.Log($"Tatil bölgesinde bina dikme — rastgele kategori: {questionCategory}, zorluk: Impossible");
        }
        else
        {
            difficulty = chosenLevel;  // 1=Beginner, 5=Impossible
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

            // Görsel güncelle: yeni bina yerleştir
            UpdateTileVisual(tile, token.playerColor);

            string levelName = chosenLevel == 5 ? "Otel" : $"{chosenLevel} Kat";
            Debug.Log($"{token.playerName} {tile.tileName}'de {levelName} dikti! (-{totalCost} ₺)");
        }
        else
        {
            Debug.Log($"{token.playerName} soruyu bilemedi, bina dikilemedi");
        }
    }

    void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % playerTokens.Count;
        StartNextTurn();
    }
    void UpdateTileVisual(Tile tile, Color ownerColor)
    {
        // Önce mevcut binayı sil (varsa)
        if (tile.currentBuilding != null)
        {
            Destroy(tile.currentBuilding);
            tile.currentBuilding = null;
        }

        // Sahip yoksa hiçbir şey koyma
        if (tile.ownerId == -1)
            return;

        // Hangi prefab kullanılacak?
        GameObject prefabToUse = GetBuildingPrefab(tile.buildingLevel);
        if (prefabToUse == null) return;

        // Tile üzerine yerleştir
        Vector3 buildingPos = tile.transform.position + new Vector3(0, buildingHeightOffset, 0);
        GameObject building = Instantiate(prefabToUse, buildingPos, Quaternion.identity);
        building.transform.SetParent(tile.transform);
        building.name = $"Building_{tile.tileName}_Level{tile.buildingLevel}";

        // Oyuncunun rengine boya
        Renderer renderer = building.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = ownerColor;
        }

        tile.currentBuilding = building;
    }
    IEnumerator HandleOpponentProperty(PlayerToken token, Tile tile)
    {
        int rentPrice = CalculateRent(tile);
        int buyPrice = tile.basePrice * 2;

        PlayerToken owner = GetPlayerById(tile.ownerId);
        if (owner == null)
        {
            Debug.LogError($"Sahip bulunamadı: {tile.ownerId}");
            yield break;
        }

        Debug.Log($"{token.playerName} → {tile.tileName} (sahibi: {owner.playerName}, kira: {rentPrice}, çalma: {buyPrice})");

        bool decisionMade = false;
        int decision = 0;  // 1 = pay rent, 2 = buy

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
            // Kira öde
            yield return StartCoroutine(PayRent(token, owner, rentPrice));
            yield break;
        }

        if (decision == 2)
        {
            // Çalmak istiyor
            yield return StartCoroutine(TryToStealProperty(token, owner, tile, buyPrice));
            yield break;
        }
    }

    IEnumerator PayRent(PlayerToken payer, PlayerToken owner, int rentAmount)
    {
        // Para yetiyor mu?
        if (payer.money < rentAmount)
        {
            // Şimdilik para 0'a düş
            Debug.Log($"{payer.playerName} parası yetmiyor! Tüm parası gidiyor: {payer.money} ₺");
            owner.money += payer.money;
            payer.money = 0;
        }
        else
        {
            payer.money -= rentAmount;
            owner.money += rentAmount;
            Debug.Log($"{payer.playerName} kira ödedi: {rentAmount} ₺ → {owner.playerName}");
        }

        UpdateMoneyDisplay();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator TryToStealProperty(PlayerToken stealer, PlayerToken owner, Tile tile, int buyPrice)
    {
        // Para yetiyor mu?
        if (stealer.money < buyPrice)
        {
            Debug.Log($"{stealer.playerName} çalmaya parası yetmiyor!");
            yield break;
        }

        // Hard zorlukta soru sor
        int difficulty = 4;  // Hard
        Category questionCategory;

        if (tile.tileType == TileType.Vacation)
        {
            // Vacation için rastgele kategori
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
            // Çalma başarılı
            stealer.money -= buyPrice;
            owner.money += buyPrice;  // Eski sahip parayı alır

            // Sahip değiştir (binalar kalır)
            tile.ownerId = stealer.playerId;

            // Vacation sayacı güncelle
            if (tile.tileType == TileType.Vacation)
            {
                owner.vacationCount--;
                stealer.vacationCount++;
                Debug.Log($"Tatil bölgesi el değiştirdi. {owner.playerName}: {owner.vacationCount}, {stealer.playerName}: {stealer.vacationCount}");
            }

            // Görsel güncelle (yeni renk, binalar kalır)
            UpdateTileVisual(tile, stealer.playerColor);

            UpdateMoneyDisplay();
            Debug.Log($"{stealer.playerName} {tile.tileName}'yi çaldı! ({owner.playerName}'den, -{buyPrice} ₺)");
        }
        else
        {
            Debug.Log($"{stealer.playerName} soruyu bilemedi, çalamadı");
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
    GameObject GetBuildingPrefab(int buildingLevel)
    {
        switch (buildingLevel)
        {
            case 0: return ownershipMarkerPrefab;  // Sahip ama bina yok
            case 1: return building1Prefab;
            case 2: return building2Prefab;
            case 3: return building3Prefab;
            case 4: return building4Prefab;
            case 5: return hotelPrefab;
            default: return null;
        }
    }
    int CalculateRent(Tile tile)
    {
        if (tile.buildingLevel == 0)
        {
            // Boş arazi: %25
            return tile.basePrice / 4;
        }
        else
        {
            // Binalı: %50 × buildingLevel
            return (tile.basePrice / 2) * tile.buildingLevel;
        }
    }
    IEnumerator HandleGoToStart(PlayerToken token)
    {
        Debug.Log($"{token.playerName} → Başlangıca Dön karesine düştü, başlangıca gidiyor (para almadan)");

        // 1 saniye göster ki oyuncu farkına varsın
        yield return new WaitForSeconds(1f);

        // Oyuncuyu başlangıca götür (BAŞLANGIÇTAN GEÇTİĞİ İÇİN PARA VERME)
        token.currentTileIndex = 0;

        // Pozisyonu güncelle
        Vector3 startPos = boardGenerator.GetTileWorldPosition(0);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = startPos + new Vector3(offsetX, tokenHeight, 0);

        Debug.Log($"{token.playerName} başlangıca döndü (para alınmadı)");

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator SendToJail(PlayerToken token)
    {
        Debug.Log($"{token.playerName} hapse gönderildi!");

        // Hapishane karesinin index'i: 10
        int jailIndex = 10;
        token.currentTileIndex = jailIndex;
        token.isInJail = true;
        token.jailTurnsLeft = gameSettings.jailDuration;  // GameSettings'ten al (3 tur)

        // Pozisyonu güncelle
        Vector3 jailPos = boardGenerator.GetTileWorldPosition(jailIndex);
        float offsetX = (token.playerId - (playerTokens.Count - 1) / 2f) * 0.3f;
        token.transform.position = jailPos + new Vector3(offsetX, tokenHeight, 0);

        UpdateMoneyDisplay();

        yield return new WaitForSeconds(1f);
    }
}