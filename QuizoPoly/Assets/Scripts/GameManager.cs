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
        diceManager.RollBothDice();

        while (diceManager.IsRolling())
            yield return null;

        int diceTotal = diceManager.GetTotal();
        var token = playerTokens[currentPlayerIndex];

        Debug.Log($"{token.playerName} attı: {diceTotal}");

        MovePlayer(token, diceTotal);

        yield return new WaitForSeconds(1f);

        Tile landedTile = boardGenerator.GetTile(token.currentTileIndex);
        Debug.Log($"{token.playerName} → {landedTile.tileName} ({landedTile.tileType})");

        // InfoPanel'i göster (oyuncuya nereye geldiğini söyle)
        UIManager.Instance.ShowInfoPanel(landedTile);
        yield return new WaitForSeconds(2f);  // 2 saniye göster
        UIManager.Instance.HideInfoPanel();

        // Kare etkileşimi
        yield return StartCoroutine(HandleTileLanding(token, landedTile));

        EndTurn();
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
        // Property veya Vacation — ikisi de satın alınabilir
        if (tile.tileType == TileType.Property || tile.tileType == TileType.Vacation)
        {
            if (tile.ownerId == -1)
            {
                // Boş arazi
                yield return StartCoroutine(HandleEmptyProperty(token, tile));
            }
            else if (tile.ownerId == token.playerId)
            {
                // Kendi arazisi - bina dik
                yield return StartCoroutine(HandleOwnProperty(token, tile));
            }
            else
            {
                // Başkasının arazisi - kira (sonraki parça)
                Debug.Log($"{token.playerName} başkasının arazisine düştü: {tile.tileName} (sahibi player {tile.ownerId})");
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            Debug.Log($"Özel kare: {tile.tileType}");
            yield return new WaitForSeconds(1f);
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

    Color GetPlayerColor(int playerId)
    {
        foreach (var token in playerTokens)
        {
            if (token.playerId == playerId)
                return token.playerColor;
        }
        return Color.white;
    }
}