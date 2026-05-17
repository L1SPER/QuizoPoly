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

        // İleride: kare etkileşimi (boş arazi paneli vs.)

        yield return new WaitForSeconds(1f);

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

    void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % playerTokens.Count;
        StartNextTurn();
    }
}