using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnOrderManager : MonoBehaviour
{
    [Header("Zar Sistemi")]
    public DiceManager diceManager;

    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text instructionText;
    public TMP_Text resultsText;
    public Button rollButton;

    private List<RollResult> allResults = new List<RollResult>();
    private List<int> finalOrder = new List<int>();
    private List<int> currentRound = new List<int>();
    private List<int> remainingPlayers = new List<int>();
    private int currentRollerIndex = 0;
    private bool isRerollMode = false;

    private class RollResult
    {
        public int playerIndex;
        public string playerName;
        public Color playerColor;
        public int diceValue;
    }

    void Start()
    {
        if (GameSetupData.Players == null || GameSetupData.Players.Length == 0)
        {
            Debug.LogError("Oyuncu verisi yok!");
            return;
        }

        titleText.text = "TURN ORDER";

        for (int i = 0; i < GameSetupData.Players.Length; i++)
        {
            currentRound.Add(i);
            remainingPlayers.Add(i);
        }

        // RollButton'a kod ile listener bağla
        rollButton.onClick.RemoveAllListeners();
        rollButton.onClick.AddListener(OnRollButtonClicked);

        UpdateInstructions();
        UpdateResultsDisplay();
    }

    void UpdateInstructions()
    {
        int playerIndex = currentRound[currentRollerIndex];
        var player = GameSetupData.Players[playerIndex];
        string colorHex = ColorUtility.ToHtmlStringRGB(player.playerColor);

        if (isRerollMode)
            instructionText.text = $"REROLL! <color=#{colorHex}>{player.playerName}</color>, roll again!";
        else
            instructionText.text = $"<color=#{colorHex}>{player.playerName}</color>, roll the dice!";

        rollButton.interactable = true;
    }

    void OnRollButtonClicked()
    {
        rollButton.interactable = false;
        StartCoroutine(RollForCurrentPlayer());
    }

    IEnumerator RollForCurrentPlayer()
    {
        diceManager.RollBothDice();

        while (diceManager.IsRolling())
            yield return null;

        int total = diceManager.GetTotal();
        int actualPlayerIndex = currentRound[currentRollerIndex];
        var player = GameSetupData.Players[actualPlayerIndex];

        var existing = allResults.FirstOrDefault(r => r.playerIndex == actualPlayerIndex);
        if (existing != null)
        {
            existing.diceValue = total;
        }
        else
        {
            allResults.Add(new RollResult
            {
                playerIndex = actualPlayerIndex,
                playerName = player.playerName,
                playerColor = player.playerColor,
                diceValue = total
            });
        }

        UpdateResultsDisplay();
        yield return new WaitForSeconds(1.5f);

        currentRollerIndex++;

        if (currentRollerIndex < currentRound.Count)
        {
            UpdateInstructions();
        }
        else
        {
            if (isRerollMode)
                RecallEligiblePlayers();

            ProcessResults();
        }
    }

    void RecallEligiblePlayers()
    {
        var rerollResults = allResults.Where(r => currentRound.Contains(r.playerIndex)).ToList();
        if (rerollResults.Count == 0) return;

        int rerollHighest = rerollResults.Max(r => r.diceValue);

        List<int> toRecall = new List<int>();
        foreach (var fIndex in finalOrder)
        {
            var fResult = allResults.First(r => r.playerIndex == fIndex);
            if (fResult.diceValue <= rerollHighest)
                toRecall.Add(fIndex);
        }

        foreach (var idx in toRecall)
        {
            finalOrder.Remove(idx);
            remainingPlayers.Add(idx);
        }
    }

    void ProcessResults()
    {
        var remainingResults = allResults.Where(r => remainingPlayers.Contains(r.playerIndex)).ToList();
        int highest = remainingResults.Max(r => r.diceValue);
        var topPlayers = remainingResults.Where(r => r.diceValue == highest).ToList();

        if (topPlayers.Count == 1)
        {
            int winnerIndex = topPlayers[0].playerIndex;
            finalOrder.Add(winnerIndex);
            remainingPlayers.Remove(winnerIndex);

            if (remainingPlayers.Count == 0)
                DetermineFinalOrder();
            else if (remainingPlayers.Count == 1)
            {
                finalOrder.Add(remainingPlayers[0]);
                remainingPlayers.Clear();
                DetermineFinalOrder();
            }
            else
                ProcessResults();
        }
        else
        {
            currentRound = topPlayers.Select(p => p.playerIndex).ToList();
            isRerollMode = true;
            currentRollerIndex = 0;
            UpdateInstructions();
        }
    }

    void DetermineFinalOrder()
    {
        PlayerSetupInfo[] orderedPlayers = new PlayerSetupInfo[finalOrder.Count];
        for (int i = 0; i < finalOrder.Count; i++)
        {
            orderedPlayers[i] = GameSetupData.Players[finalOrder[i]];
        }

        GameSetupData.Players = orderedPlayers;

        instructionText.text = "Order determined! Starting game...";
        UpdateResultsDisplay(showRanking: true);

        StartCoroutine(StartMainGame());
    }

    void UpdateResultsDisplay(bool showRanking = false)
    {
        if (allResults.Count == 0)
        {
            resultsText.text = "<b>RESULTS:</b>\n";
            return;
        }

        string header = showRanking ? "FINAL ORDER:" : "RESULTS:";
        string text = $"<b>{header}</b>\n\n";

        if (showRanking)
        {
            for (int i = 0; i < finalOrder.Count; i++)
            {
                var r = allResults.First(x => x.playerIndex == finalOrder[i]);
                string colorHex = ColorUtility.ToHtmlStringRGB(r.playerColor);
                text += $"{i + 1}. <color=#{colorHex}>{r.playerName}</color>: <b>{r.diceValue}</b>\n";
            }
        }
        else
        {
            for (int i = 0; i < finalOrder.Count; i++)
            {
                var r = allResults.First(x => x.playerIndex == finalOrder[i]);
                string colorHex = ColorUtility.ToHtmlStringRGB(r.playerColor);
                text += $"{i + 1}. <color=#{colorHex}>{r.playerName}</color>: <b>{r.diceValue}</b>\n";
            }

            var unranked = allResults.Where(r => !finalOrder.Contains(r.playerIndex))
                                      .OrderByDescending(r => r.diceValue);
            foreach (var r in unranked)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(r.playerColor);
                text += $"<color=#{colorHex}>{r.playerName}</color>: <b>{r.diceValue}</b>\n";
            }
        }

        resultsText.text = text;
    }

    IEnumerator StartMainGame()
    {
        yield return new WaitForSeconds(3f);

        // Listener'ı temizle, GameManager kendi bağlasın
        rollButton.onClick.RemoveAllListeners();

        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            Debug.LogError("GameManager bulunamadı!");

        // Bu script'i devre dışı bırak
        this.enabled = false;
    }
}